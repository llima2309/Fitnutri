using Fitnutri.Domain;
using Microsoft.EntityFrameworkCore;

namespace Fitnutri.Infrastructure;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Perfil> Perfis => Set<Perfil>();
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var u = modelBuilder.Entity<User>();

        u.HasKey(x => x.Id);
        u.Property(x => x.UserName).HasMaxLength(32).IsRequired();
        u.Property(x => x.Email).HasMaxLength(256).IsRequired();
        u.HasIndex(x => x.UserName).IsUnique();
        u.HasIndex(x => x.Email).IsUnique();

        u.Property(x => x.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");

        // email verification
        u.Property(x => x.EmailConfirmed).IsRequired();
        u.Property(x => x.EmailVerificationCode);
        u.Property(x => x.EmailVerificationExpiresAt);

        // password reset - CONFIGURAÇÃO DOS NOVOS CAMPOS
        u.Property(x => x.PasswordResetToken).HasMaxLength(512);
        u.Property(x => x.PasswordResetExpiresAt);

        // approval
        u.Property(x => x.Status).HasConversion<int>().IsRequired();
        u.Property(x => x.ApprovedAt);
        u.Property(x => x.ApprovedBy).HasMaxLength(128);

        u.Property(x => x.Role).HasConversion<int>().IsRequired();
        
        // Configuração do relacionamento one-to-many User -> Perfil
        u.HasOne(x => x.Perfil)
            .WithMany(p => p.Usuarios)
            .HasForeignKey(x => x.PerfilId)
            .OnDelete(DeleteBehavior.Restrict);

        // Configuração do relacionamento one-to-one User -> UserProfile
        u.HasOne(x => x.Profile)
            .WithOne(p => p.User)
            .HasForeignKey<UserProfile>(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        var p = modelBuilder.Entity<Perfil>();
        p.HasKey(x => x.Id);
        p.Property(x => x.Nome).HasMaxLength(64).IsRequired();
        p.Property(x => x.Tipo).HasConversion<int>().IsRequired();

        // Configuração da entidade UserProfile
        var up = modelBuilder.Entity<UserProfile>();
        up.HasKey(x => x.Id);
        
        // Informações pessoais
        up.Property(x => x.NomeCompleto).HasMaxLength(256).IsRequired();
        up.Property(x => x.CPF).HasMaxLength(14).IsRequired();
        up.HasIndex(x => x.CPF).IsUnique();
        up.Property(x => x.RG).HasMaxLength(20);
        up.Property(x => x.Genero).HasConversion<int>().IsRequired();
        up.Property(x => x.DataNascimento).IsRequired();
        
        // Informações profissionais
        up.Property(x => x.CRN).HasMaxLength(20);
        
        // Endereço
        up.Property(x => x.CEP).HasMaxLength(10).IsRequired();
        up.Property(x => x.Estado).HasConversion<int>().IsRequired();
        up.Property(x => x.Endereco).HasMaxLength(256).IsRequired();
        up.Property(x => x.Numero).HasMaxLength(20).IsRequired();
        up.Property(x => x.Cidade).HasMaxLength(128).IsRequired();
        up.Property(x => x.Complemento).HasMaxLength(128);
        up.Property(x => x.Bairro).HasMaxLength(128);
        
        // Dados do ViaCEP
        up.Property(x => x.UF).HasMaxLength(2);
        up.Property(x => x.IBGE).HasMaxLength(20);
        up.Property(x => x.DDD).HasMaxLength(3);
        
        // Timestamps
        up.Property(x => x.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
        up.Property(x => x.UpdatedAt);
    }
}
