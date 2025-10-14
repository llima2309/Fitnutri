using System.ComponentModel.DataAnnotations;

namespace AppFitNutri.Core.Models;

public class UserProfile
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string NomeCompleto { get; set; } = string.Empty;
    public string CPF { get; set; } = string.Empty;
    public string? RG { get; set; }
    public Genero Genero { get; set; }
    public DateTime DataNascimento { get; set; }
    public string? CRN { get; set; }
    public string CEP { get; set; } = string.Empty;
    public Estado Estado { get; set; }
    public string Endereco { get; set; } = string.Empty;
    public string Numero { get; set; } = string.Empty;
    public string Cidade { get; set; } = string.Empty;
    public string? Complemento { get; set; }
    public string Bairro { get; set; } = string.Empty;
    public string? UF { get; set; }
    public string? IBGE { get; set; }
    public string? DDD { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class CreateUserProfileRequest
{
    [Required(ErrorMessage = "Nome completo é obrigatório")]
    public string NomeCompleto { get; set; } = string.Empty;

    [Required(ErrorMessage = "CPF é obrigatório")]
    [RegularExpression(@"^\d{3}\.\d{3}\.\d{3}-\d{2}$", ErrorMessage = "CPF deve estar no formato 000.000.000-00")]
    public string CPF { get; set; } = string.Empty;

    public string? RG { get; set; }

    [Required(ErrorMessage = "Gênero é obrigatório")]
    public Genero Genero { get; set; }

    [Required(ErrorMessage = "Data de nascimento é obrigatória")]
    public DateTime DataNascimento { get; set; }

    public string? Telefone { get; set; }

    public string? CRN { get; set; }

    [Required(ErrorMessage = "CEP é obrigatório")]
    [RegularExpression(@"^\d{5}-?\d{3}$", ErrorMessage = "CEP deve estar no formato 00000-000")]
    public string CEP { get; set; } = string.Empty;

    [Required(ErrorMessage = "Estado é obrigatório")]
    public Estado Estado { get; set; }

    [Required(ErrorMessage = "Endereço é obrigatório")]
    public string Endereco { get; set; } = string.Empty;

    [Required(ErrorMessage = "Número é obrigatório")]
    public string Numero { get; set; } = string.Empty;

    [Required(ErrorMessage = "Cidade é obrigatória")]
    public string Cidade { get; set; } = string.Empty;

    public string? Complemento { get; set; }

    [Required(ErrorMessage = "Bairro é obrigatório")]
    public string Bairro { get; set; } = string.Empty;
}

public class UpdateUserProfileRequest
{
    [Required(ErrorMessage = "Nome completo é obrigatório")]
    public string NomeCompleto { get; set; } = string.Empty;

    public string? RG { get; set; }

    [Required(ErrorMessage = "Gênero é obrigatório")]
    public Genero Genero { get; set; }

    [Required(ErrorMessage = "Data de nascimento é obrigatória")]
    public DateTime DataNascimento { get; set; }

    public string? Telefone { get; set; }

    public string? CRN { get; set; }

    [Required(ErrorMessage = "CEP é obrigatório")]
    [RegularExpression(@"^\d{5}-?\d{3}$", ErrorMessage = "CEP deve estar no formato 00000-000")]
    public string CEP { get; set; } = string.Empty;

    [Required(ErrorMessage = "Estado é obrigatório")]
    public Estado Estado { get; set; }

    [Required(ErrorMessage = "Endereço é obrigatório")]
    public string Endereco { get; set; } = string.Empty;

    [Required(ErrorMessage = "Número é obrigatório")]
    public string Numero { get; set; } = string.Empty;

    [Required(ErrorMessage = "Cidade é obrigatória")]
    public string Cidade { get; set; } = string.Empty;

    public string? Complemento { get; set; }

    [Required(ErrorMessage = "Bairro é obrigatório")]
    public string Bairro { get; set; } = string.Empty;
}

public class UserProfileResponse
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string NomeCompleto { get; set; } = string.Empty;
    public string CPF { get; set; } = string.Empty;
    public string? RG { get; set; }
    public Genero Genero { get; set; }
    public DateTime DataNascimento { get; set; }
    public string? Telefone { get; set; }
    public string? CRN { get; set; }
    public string CEP { get; set; } = string.Empty;
    public Estado Estado { get; set; }
    public string Endereco { get; set; } = string.Empty;
    public string Numero { get; set; } = string.Empty;
    public string Cidade { get; set; } = string.Empty;
    public string? Complemento { get; set; }
    public string Bairro { get; set; } = string.Empty;
    public string? UF { get; set; }
    public string? IBGE { get; set; }
    public string? DDD { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class AddressFromCepResponse
{
    public string CEP { get; set; } = string.Empty;
    public string Logradouro { get; set; } = string.Empty;
    public string? Complemento { get; set; }
    public string Bairro { get; set; } = string.Empty;
    public string Cidade { get; set; } = string.Empty;  // Campo correto da API
    public string UF { get; set; } = string.Empty;
    public string Estado { get; set; } = string.Empty;
    public string DDD { get; set; } = string.Empty;
}

public class GeneroOption
{
    public int Value { get; set; }
    public string Display { get; set; } = string.Empty;

    public GeneroOption(int value, string display)
    {
        Value = value;
        Display = display;
    }
}

public class EstadoOption
{
    public int Value { get; set; }
    public string Display { get; set; } = string.Empty;
    public string Sigla { get; set; } = string.Empty;

    public EstadoOption(int value, string display, string sigla)
    {
        Value = value;
        Display = display;
        Sigla = sigla;
    }
}

public enum Genero
{
    Masculino = 1,
    Feminino = 2,
    Outro = 3
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
