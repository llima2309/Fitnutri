namespace AppFitNutri.Models;

public class AgendamentoItem
{
    public Guid Id { get; set; }
    public Guid ProfissionalId { get; set; }
    public DateOnly Data { get; set; }
    public TimeOnly Hora { get; set; }
    public int DuracaoMinutos { get; set; }
    public int Status { get; set; }

    // Propriedades computadas para exibição na UI
    public string DataTexto => Data.ToString("dd/MM");
    public string HoraTexto => Hora.ToString("HH:mm");
    public string StatusTexto => Status switch
    {
        0 => "Pendente",
        1 => "Confirmado",
        2 => "Cancelado",
        _ => "Desconhecido"
    };
    public bool PodeCancelar => Status != 2; // Não pode cancelar se já está cancelado
}
