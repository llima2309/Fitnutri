using Fitnutri.Contracts;
using Fitnutri.Domain;
using Fitnutri.Infrastructure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Fitnutri.Application;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class VideoCallController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IHubContext<VideoCallHub> _hubContext;
    private readonly ILogger<VideoCallController> _logger;

    public VideoCallController(
        AppDbContext db,
        IHubContext<VideoCallHub> hubContext,
        ILogger<VideoCallController> logger)
    {
        _db = db;
        _hubContext = hubContext;
        _logger = logger;
    }

    /// <summary>
    /// Inicia uma videochamada para um agendamento confirmado
    /// </summary>
    [HttpPost("initiate")]
    public async Task<IActionResult> InitiateCall([FromBody] VideoCallInitiateRequest request, CancellationToken ct)
    {
        var userIdClaim = User.FindFirst("sub")?.Value;
        if (string.IsNullOrWhiteSpace(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { error = "Usuário não autenticado." });
        }

        var agendamento = await _db.Agendamentos
            .FirstOrDefaultAsync(a => a.Id == request.AgendamentoId, ct);

        if (agendamento == null)
        {
            return NotFound(new { error = "Agendamento não encontrado." });
        }

        // Verifica se o usuário é o profissional ou o cliente
        if (agendamento.ProfissionalId != userId && agendamento.ClienteUserId != userId)
        {
            return Forbid();
        }

        // Verifica se o agendamento está confirmado
        if (agendamento.Status != AgendamentoStatus.Confirmado)
        {
            return BadRequest(new { error = "Agendamento não está confirmado." });
        }

        // Verifica se já existe uma chamada ativa
        if (!string.IsNullOrWhiteSpace(agendamento.CallToken) && agendamento.CallEndedAt == null)
        {
            return Ok(new VideoCallResponse(
                agendamento.Id,
                agendamento.CallToken,
                agendamento.CallStartedAt!.Value,
                "/videocall"
            ));
        }

        // Gera token único para a chamada
        var callToken = Guid.NewGuid().ToString("N");
        agendamento.CallToken = callToken;
        agendamento.CallStartedAt = DateTime.UtcNow;
        agendamento.CallEndedAt = null;

        await _db.SaveChangesAsync(ct);

        _logger.LogInformation(
            "Videochamada iniciada para agendamento {AgendamentoId} pelo usuário {UserId}",
            agendamento.Id, userId);

        return Ok(new VideoCallResponse(
            agendamento.Id,
            callToken,
            agendamento.CallStartedAt.Value,
            "/videocall"
        ));
    }

    /// <summary>
    /// Encerra uma videochamada ativa
    /// </summary>
    [HttpPost("end")]
    public async Task<IActionResult> EndCall([FromBody] VideoCallEndRequest request, CancellationToken ct)
    {
        var userIdClaim = User.FindFirst("sub")?.Value;
        if (string.IsNullOrWhiteSpace(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { error = "Usuário não autenticado." });
        }

        var agendamento = await _db.Agendamentos
            .FirstOrDefaultAsync(a => a.Id == request.AgendamentoId, ct);

        if (agendamento == null)
        {
            return NotFound(new { error = "Agendamento não encontrado." });
        }

        // Verifica se o usuário é o profissional ou o cliente
        if (agendamento.ProfissionalId != userId && agendamento.ClienteUserId != userId)
        {
            return Forbid();
        }

        // Verifica se há uma chamada ativa
        if (string.IsNullOrWhiteSpace(agendamento.CallToken) || agendamento.CallEndedAt != null)
        {
            return BadRequest(new { error = "Não há chamada ativa para este agendamento." });
        }

        // Encerra a chamada
        agendamento.CallEndedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);

        // Notifica todos os participantes via SignalR
        await _hubContext.Clients.Group($"call_{agendamento.Id}")
            .SendAsync("CallEnded", ct);

        var duration = (agendamento.CallEndedAt.Value - agendamento.CallStartedAt!.Value).TotalMinutes;

        _logger.LogInformation(
            "Videochamada encerrada para agendamento {AgendamentoId} pelo usuário {UserId}. Duração: {Duration} minutos",
            agendamento.Id, userId, duration);

        return Ok(new
        {
            message = "Chamada encerrada com sucesso.",
            durationMinutes = (int)duration
        });
    }

    /// <summary>
    /// Obtém o status de uma videochamada
    /// </summary>
    [HttpGet("status/{agendamentoId}")]
    public async Task<IActionResult> GetCallStatus(Guid agendamentoId, CancellationToken ct)
    {
        var userIdClaim = User.FindFirst("sub")?.Value;
        if (string.IsNullOrWhiteSpace(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { error = "Usuário não autenticado." });
        }

        var agendamento = await _db.Agendamentos
            .FirstOrDefaultAsync(a => a.Id == agendamentoId, ct);

        if (agendamento == null)
        {
            return NotFound(new { error = "Agendamento não encontrado." });
        }

        // Verifica se o usuário é o profissional ou o cliente
        if (agendamento.ProfissionalId != userId && agendamento.ClienteUserId != userId)
        {
            return Forbid();
        }

        var isActive = !string.IsNullOrWhiteSpace(agendamento.CallToken) 
                       && agendamento.CallStartedAt != null 
                       && agendamento.CallEndedAt == null;

        int? duration = null;
        if (agendamento.CallStartedAt != null && agendamento.CallEndedAt != null)
        {
            duration = (int)(agendamento.CallEndedAt.Value - agendamento.CallStartedAt.Value).TotalMinutes;
        }

        return Ok(new VideoCallStatusResponse(
            agendamento.Id,
            isActive,
            agendamento.CallStartedAt,
            agendamento.CallEndedAt,
            duration
        ));
    }

    /// <summary>
    /// Valida se o usuário pode entrar em uma chamada específica
    /// </summary>
    [HttpGet("validate/{agendamentoId}")]
    public async Task<IActionResult> ValidateAccess(Guid agendamentoId, CancellationToken ct)
    {
        var userIdClaim = User.FindFirst("sub")?.Value;
        if (string.IsNullOrWhiteSpace(userIdClaim) || !Guid.TryParse(userIdClaim, out var userId))
        {
            return Unauthorized(new { error = "Usuário não autenticado." });
        }

        var agendamento = await _db.Agendamentos
            .FirstOrDefaultAsync(a => a.Id == agendamentoId, ct);

        if (agendamento == null)
        {
            return NotFound(new { error = "Agendamento não encontrado." });
        }

        var hasAccess = agendamento.ProfissionalId == userId || agendamento.ClienteUserId == userId;
        var userType = agendamento.ProfissionalId == userId ? "profissional" : "cliente";

        if (!hasAccess)
        {
            return Forbid();
        }

        var isActive = !string.IsNullOrWhiteSpace(agendamento.CallToken) 
                       && agendamento.CallStartedAt != null 
                       && agendamento.CallEndedAt == null;

        return Ok(new
        {
            hasAccess = true,
            userType,
            isActive,
            callToken = agendamento.CallToken,
            agendamentoId = agendamento.Id,
            profissionalId = agendamento.ProfissionalId,
            clienteId = agendamento.ClienteUserId
        });
    }
}

