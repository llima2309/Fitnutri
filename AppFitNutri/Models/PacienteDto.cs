namespace AppFitNutri.Models;

public record PacienteDto(
    Guid Id,
    string NomeCompleto,
    string? Email,
    string? Telefone,
    DateTime? UltimaConsulta
);

