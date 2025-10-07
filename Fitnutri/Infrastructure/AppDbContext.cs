using Fitnutri.Domain;
using Microsoft.EntityFrameworkCore;

namespace Fitnutri.Infrastructure;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Perfil> Perfis => Set<Perfil>();

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

        var p = modelBuilder.Entity<Perfil>();
        p.HasKey(x => x.Id);
        p.Property(x => x.Nome).HasMaxLength(64).IsRequired();
        p.Property(x => x.Tipo).HasConversion<int>().IsRequired();
    }
}
