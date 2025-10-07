using Fitnutri.Domain;

namespace Fitnutri.Contracts;

// DTOs para UserProfile
public record CreateUserProfileRequest(
    string NomeCompleto,
    string CPF,
    string? RG,
    Genero Genero,
    DateTime DataNascimento,
    string? CRN,
    string CEP,
    Estado Estado,
    string Endereco,
    string Numero,
    string Cidade,
    string? Complemento,
    string? Bairro
);

public record UpdateUserProfileRequest(
    string NomeCompleto,
    string? RG,
    Genero Genero,
    DateTime DataNascimento,
    string? CRN,
    string CEP,
    Estado Estado,
    string Endereco,
    string Numero,
    string Cidade,
    string? Complemento,
    string? Bairro
);

public record UserProfileResponse(
    Guid Id,
    string NomeCompleto,
    string CPF,
    string? RG,
    Genero Genero,
    DateTime DataNascimento,
    string? CRN,
    string CEP,
    Estado Estado,
    string Endereco,
    string Numero,
    string Cidade,
    string? Complemento,
    string? Bairro,
    string? UF,
    string? IBGE,
    string? DDD,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

// DTOs para ViaCEP
public record ViaCepResponse(
    string Cep,
    string Logradouro,
    string Complemento,
    string Unidade,
    string Bairro,
    string Localidade,
    string Uf,
    string Estado,
    string Regiao,
    string Ibge,
    string Gia,
    string Ddd,
    string Siafi
);

public record AddressFromCepResponse(
    string CEP,
    string Logradouro,
    string? Complemento,
    string Bairro,
    string Cidade,
    string UF,
    string Estado,
    string DDD
);

// DTOs para enums
public record GeneroOption(int Id, string Nome);
public record EstadoOption(int Id, string Nome, string Sigla);
