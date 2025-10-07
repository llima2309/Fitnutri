namespace Fitnutri.Domain;

public enum PerfilTipo
{
    Administrador = 1,
    Nutricionista = 2,
    PersonalTrainer = 3,
    Paciente = 4
}

public class Perfil
{
    public Guid Id { get; set; }
    public PerfilTipo Tipo { get; set; }
    public string Nome { get; set; } = default!;
    // Relacionamento one-to-many: um perfil pode ter vários usuários
    public ICollection<User> Usuarios { get; set; } = new List<User>();
}
