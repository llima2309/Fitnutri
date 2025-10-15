using Fitnutri.Domain;

namespace Fitnutri.Contracts;

public record DisponibilidadeResponse(IReadOnlyList<string> Horarios);

public record CriarAgendamentoRequest(Guid ProfissionalId, DateOnly Data, TimeOnly Hora);

public record AtualizarAgendamentoRequest(
    DateOnly? Data,
    TimeOnly? Hora,
    int? DuracaoMinutos,
    AgendamentoStatus? Status
);

public record AgendamentoResponse(
    Guid Id,
    Guid ProfissionalId,
    Guid ClienteUserId,
    DateOnly Data,
    TimeOnly Hora,
    int DuracaoMinutos,
    AgendamentoStatus Status
);
