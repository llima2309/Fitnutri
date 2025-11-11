namespace AppFitNutri.Models;

public record AgendamentoDto(
    Guid Id,
    Guid ProfissionalId,
    Guid ClienteUserId,
    DateOnly Data,
    TimeOnly Hora,
    int DuracaoMinutos,
    int Status,
    string? ProfissionalNome = null,
    string? ProfissionalPerfil = null
);

