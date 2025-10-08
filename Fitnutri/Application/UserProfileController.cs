using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Fitnutri.Infrastructure;
using Fitnutri.Contracts;
using Fitnutri.Domain;
using Fitnutri.Application.Services;
using System.Security.Claims;

namespace Fitnutri.Application;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UserProfileController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IViaCepService _viaCepService;
    private readonly ILogger<UserProfileController> _logger;

    public UserProfileController(
        AppDbContext context, 
        IViaCepService viaCepService,
        ILogger<UserProfileController> logger)
    {
        _context = context;
        _viaCepService = viaCepService;
        _logger = logger;
    }

    [HttpGet("cep/{cep}")]
    public async Task<ActionResult<AddressFromCepResponse>> GetAddressByCep(string cep)
    {
        var address = await _viaCepService.GetAddressByCepAsync(cep);
        
        if (address == null)
        {
            return NotFound(new { message = "CEP não encontrado ou inválido" });
        }

        return Ok(address);
    }

    [HttpGet("options/genero")]
    public ActionResult<IEnumerable<GeneroOption>> GetGeneroOptions()
    {
        var options = Enum.GetValues<Genero>()
            .Select(g => new GeneroOption((int)g, GetGeneroDisplayName(g)))
            .ToList();

        return Ok(options);
    }

    [HttpGet("options/estado")]
    public ActionResult<IEnumerable<EstadoOption>> GetEstadoOptions()
    {
        var options = Enum.GetValues<Estado>()
            .Select(e => new EstadoOption((int)e, GetEstadoDisplayName(e), e.ToString()))
            .ToList();

        return Ok(options);
    }

    [HttpGet]
    public async Task<ActionResult<UserProfileResponse>> GetProfile()
    {
        var userId = GetUserId();
        
        var profile = await _context.UserProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (profile == null)
        {
            return NotFound(new { message = "Perfil não encontrado" });
        }

        return Ok(MapToResponse(profile));
    }

    [HttpPost]
    public async Task<ActionResult<UserProfileResponse>> CreateProfile(CreateUserProfileRequest request)
    {
        var userId = GetUserId();
        
        // Debug: Log do UserId extraído do token
        _logger.LogInformation("==> UserId extraído do token: {UserId}", userId);
        
        // Verificar se o UserId é válido
        if (userId == Guid.Empty)
        {
            _logger.LogWarning("==> UserId inválido ou não encontrado no token");
            return BadRequest(new { message = "Token inválido ou UserId não encontrado" });
        }
        
        // Verificar se o usuário existe na tabela Users
        var userExists = await _context.Users.AnyAsync(u => u.Id == userId);
        if (!userExists)
        {
            _logger.LogWarning("==> Usuário com ID {UserId} não existe na tabela Users", userId);
            return BadRequest(new { message = "Usuário não encontrado" });
        }
        
        _logger.LogInformation("==> Usuário com ID {UserId} encontrado na tabela Users", userId);
        
        // Verificar se já existe perfil para este usuário
        var existingProfile = await _context.UserProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (existingProfile != null)
        {
            _logger.LogWarning("==> Usuário {UserId} já possui um perfil cadastrado", userId);
            return BadRequest(new { message = "Usuário já possui um perfil cadastrado" });
        }

        // Verificar se CPF já está em uso
        var existingCpf = await _context.UserProfiles
            .FirstOrDefaultAsync(p => p.CPF == request.CPF);

        if (existingCpf != null)
        {
            _logger.LogWarning("==> CPF {CPF} já está cadastrado para outro usuário", request.CPF);
            return BadRequest(new { message = "CPF já está cadastrado para outro usuário" });
        }

        // Debug: Log dos dados recebidos
        _logger.LogInformation("==> Dados recebidos para criação do perfil:");
        _logger.LogInformation("    Nome: {Nome}", request.NomeCompleto);
        _logger.LogInformation("    CPF: {CPF}", request.CPF);
        _logger.LogInformation("    RG: {RG}", request.RG);
        _logger.LogInformation("    Gênero: {Genero}", request.Genero);
        _logger.LogInformation("    Data Nascimento: {DataNascimento}", request.DataNascimento);

        // Buscar dados do CEP
        var addressData = await _viaCepService.GetAddressByCepAsync(request.CEP);

        var profile = new UserProfile
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            NomeCompleto = request.NomeCompleto,
            CPF = request.CPF,
            RG = request.RG,
            Genero = request.Genero,
            DataNascimento = request.DataNascimento,
            CRN = request.CRN,
            CEP = request.CEP,
            Estado = request.Estado,
            Endereco = request.Endereco,
            Numero = request.Numero,
            Cidade = request.Cidade,
            Complemento = request.Complemento,
            Bairro = request.Bairro,
            // Dados do ViaCEP (se disponível)
            UF = addressData?.UF,
            IBGE = addressData?.Estado, // Usando Estado como fallback já que IBGE não está no DTO
            DDD = addressData?.DDD
        };

        _logger.LogInformation("==> Criando perfil com ID {ProfileId} para usuário {UserId}", profile.Id, userId);

        _context.UserProfiles.Add(profile);
        
        try
        {
            await _context.SaveChangesAsync();
            _logger.LogInformation("==> Perfil criado com sucesso para usuário {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "==> ERRO ao salvar perfil para usuário {UserId}: {ErrorMessage}", userId, ex.Message);
            throw;
        }

        return CreatedAtAction(nameof(GetProfile), MapToResponse(profile));
    }

    [HttpPut]
    public async Task<ActionResult<UserProfileResponse>> UpdateProfile(UpdateUserProfileRequest request)
    {
        var userId = GetUserId();
        
        var profile = await _context.UserProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (profile == null)
        {
            return NotFound(new { message = "Perfil não encontrado" });
        }

        // Buscar dados do CEP se mudou
        AddressFromCepResponse? addressData = null;
        if (profile.CEP != request.CEP)
        {
            addressData = await _viaCepService.GetAddressByCepAsync(request.CEP);
        }

        // Atualizar dados
        profile.NomeCompleto = request.NomeCompleto;
        profile.RG = request.RG;
        profile.Genero = request.Genero;
        profile.DataNascimento = request.DataNascimento;
        profile.CRN = request.CRN;
        profile.CEP = request.CEP;
        profile.Estado = request.Estado;
        profile.Endereco = request.Endereco;
        profile.Numero = request.Numero;
        profile.Cidade = request.Cidade;
        profile.Complemento = request.Complemento;
        profile.Bairro = request.Bairro;
        profile.UpdatedAt = DateTime.UtcNow;

        // Atualizar dados do ViaCEP se houver mudança de CEP
        if (addressData != null)
        {
            profile.UF = addressData.UF;
            profile.IBGE = addressData.Estado; // Usando Estado como fallback já que IBGE não está no DTO
            profile.DDD = addressData.DDD;
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Perfil atualizado para usuário {UserId}", userId);

        return Ok(MapToResponse(profile));
    }

    [HttpDelete]
    public async Task<ActionResult> DeleteProfile()
    {
        var userId = GetUserId();
        
        var profile = await _context.UserProfiles
            .FirstOrDefaultAsync(p => p.UserId == userId);

        if (profile == null)
        {
            return NotFound(new { message = "Perfil não encontrado" });
        }

        _context.UserProfiles.Remove(profile);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Perfil removido para usuário {UserId}", userId);

        return NoContent();
    }

    private Guid GetUserId()
    {
        var userIdClaim = User.FindFirst("sub")?.Value;
        return userIdClaim == null ? Guid.Empty : Guid.Parse(userIdClaim!);
    }

    private static UserProfileResponse MapToResponse(UserProfile profile)
    {
        return new UserProfileResponse(
            profile.Id,
            profile.NomeCompleto,
            profile.CPF,
            profile.RG,
            profile.Genero,
            profile.DataNascimento,
            profile.CRN,
            profile.CEP,
            profile.Estado,
            profile.Endereco,
            profile.Numero,
            profile.Cidade,
            profile.Complemento,
            profile.Bairro,
            profile.UF,
            profile.IBGE,
            profile.DDD,
            profile.CreatedAt,
            profile.UpdatedAt
        );
    }

    private static string GetGeneroDisplayName(Genero genero) => genero switch
    {
        Genero.Masculino => "Masculino",
        Genero.Feminino => "Feminino",
        Genero.Outro => "Outro",
        Genero.PrefiroNaoInformar => "Prefiro não informar",
        _ => genero.ToString()
    };

    private static string GetEstadoDisplayName(Estado estado) => estado switch
    {
        Estado.AC => "Acre",
        Estado.AL => "Alagoas",
        Estado.AP => "Amapá",
        Estado.AM => "Amazonas",
        Estado.BA => "Bahia",
        Estado.CE => "Ceará",
        Estado.DF => "Distrito Federal",
        Estado.ES => "Espírito Santo",
        Estado.GO => "Goiás",
        Estado.MA => "Maranhão",
        Estado.MT => "Mato Grosso",
        Estado.MS => "Mato Grosso do Sul",
        Estado.MG => "Minas Gerais",
        Estado.PA => "Pará",
        Estado.PB => "Paraíba",
        Estado.PR => "Paraná",
        Estado.PE => "Pernambuco",
        Estado.PI => "Piauí",
        Estado.RJ => "Rio de Janeiro",
        Estado.RN => "Rio Grande do Norte",
        Estado.RS => "Rio Grande do Sul",
        Estado.RO => "Rondônia",
        Estado.RR => "Roraima",
        Estado.SC => "Santa Catarina",
        Estado.SP => "São Paulo",
        Estado.SE => "Sergipe",
        Estado.TO => "Tocantins",
        _ => estado.ToString()
    };
}
