namespace AppFitNutri.Models;

public record DashboardNutricionistaDto(
    int TotalAgendamentosHoje,
    int TotalAgendamentosSemana,
    int TotalPacientes,
    int TotalDietasAtivas,
    List<AgendamentoDto> ProximosAgendamentos
);

