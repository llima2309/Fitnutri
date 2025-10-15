namespace Fitnutri.Domain;

public enum AgendamentoStatus
{
    Pendente = 0,
    Confirmado = 1,
    Cancelado = 2
}

public class Agendamento
{
    public Guid Id { get; set; }

    // Profissional (User.Id do profissional)
    public Guid ProfissionalId { get; set; }

    // Cliente (User.Id do paciente/cliente)
    public Guid ClienteUserId { get; set; }

    // Data e hora do agendamento (fuso local do profissional)
    public DateOnly Data { get; set; }
    public TimeOnly Hora { get; set; }

    public int DuracaoMinutos { get; set; } = 60;

    public AgendamentoStatus Status { get; set; } = AgendamentoStatus.Pendente;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

