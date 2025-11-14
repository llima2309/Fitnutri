namespace Fitnutri.Contracts;

public record VideoCallInitiateRequest(Guid AgendamentoId);

public record VideoCallResponse(
    Guid AgendamentoId,
    string CallToken,
    DateTime CallStartedAt,
    string HubUrl
);

public record VideoCallEndRequest(Guid AgendamentoId);

public record VideoCallStatusResponse(
    Guid AgendamentoId,
    bool IsActive,
    DateTime? CallStartedAt,
    DateTime? CallEndedAt,
    int? DurationMinutes
);

