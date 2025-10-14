namespace Fitnutri.Domain;

public enum Genero
{
    Masculino = 1,
    Feminino = 2,
    Outro = 3,
    PrefiroNaoInformar = 4
}

public enum Estado
{
    AC = 1, // Acre
    AL = 2, // Alagoas
    AP = 3, // Amapá
    AM = 4, // Amazonas
    BA = 5, // Bahia
    CE = 6, // Ceará
    DF = 7, // Distrito Federal
    ES = 8, // Espírito Santo
    GO = 9, // Goiás
    MA = 10, // Maranhão
    MT = 11, // Mato Grosso
    MS = 12, // Mato Grosso do Sul
    MG = 13, // Minas Gerais
    PA = 14, // Pará
    PB = 15, // Paraíba
    PR = 16, // Paraná
    PE = 17, // Pernambuco
    PI = 18, // Piauí
    RJ = 19, // Rio de Janeiro
    RN = 20, // Rio Grande do Norte
    RS = 21, // Rio Grande do Sul
    RO = 22, // Rondônia
    RR = 23, // Roraima
    SC = 24, // Santa Catarina
    SP = 25, // São Paulo
    SE = 26, // Sergipe
    TO = 27  // Tocantins
}

public class UserProfile
{
    public Guid Id { get; set; }
    
    // Informações Pessoais
    public string NomeCompleto { get; set; } = default!;
    public string CPF { get; set; } = default!;
    public string? RG { get; set; }
    public Genero Genero { get; set; }
    public DateTime DataNascimento { get; set; }
    public string? Telefone { get; set; }
    
    // Informações Profissionais (para nutricionistas)
    public string? CRN { get; set; }
    
    // Endereço
    public string CEP { get; set; } = default!;
    public Estado Estado { get; set; }
    public string Endereco { get; set; } = default!;
    public string Numero { get; set; } = default!;
    public string Cidade { get; set; } = default!;
    public string? Complemento { get; set; }
    public string? Bairro { get; set; }
    
    // Dados adicionais do ViaCEP (para cache)
    public string? UF { get; set; }
    public string? IBGE { get; set; }
    public string? DDD { get; set; }
    
    // Timestamps
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Relacionamento com User (One-to-One)
    public Guid UserId { get; set; }
    public User User { get; set; } = default!;
}
