namespace AppFitNutri.Models;

public class Profissional
{
    public Guid Id { get; set; }
    public string NomeCompleto { get; set; } = string.Empty;
    public int TipoProfissional { get; set; }
    public string? CRN { get; set; }
    public string Cidade { get; set; } = string.Empty;
    public int Estado { get; set; }

    // Propriedades computadas para exibição
    public string CidadeEstado => $"{Cidade}, {GetEstadoSigla(Estado)}";
    public string RegistroProfissional => !string.IsNullOrEmpty(CRN) ? $"CRN: {CRN}" : string.Empty;
    public bool TemRegistro => !string.IsNullOrEmpty(CRN);

    private static string GetEstadoSigla(int estado) => estado switch
    {
        1 => "AC", 2 => "AL", 3 => "AP", 4 => "AM", 5 => "BA", 6 => "CE", 7 => "DF",
        8 => "ES", 9 => "GO", 10 => "MA", 11 => "MT", 12 => "MS", 13 => "MG", 14 => "PA",
        15 => "PB", 16 => "PR", 17 => "PE", 18 => "PI", 19 => "RJ", 20 => "RN", 21 => "RS",
        22 => "RO", 23 => "RR", 24 => "SC", 25 => "SP", 26 => "SE", 27 => "TO",
        _ => "??"
    };
}
