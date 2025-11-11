using Fitnutri.Domain;
using Microsoft.EntityFrameworkCore;

namespace Fitnutri.Infrastructure;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Perfil> Perfis => Set<Perfil>();
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<Agendamento> Agendamentos => Set<Agendamento>();
    public DbSet<Diet> Diets => Set<Diet>();
    public DbSet<DietDayMeal> DietDayMeals => Set<DietDayMeal>();
    public DbSet<PatientDiet> PatientDiets => Set<PatientDiet>();

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
        up.Property(x => x.Telefone).HasMaxLength(20);
        
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

        // ===== Agendamento =====
        var ag = modelBuilder.Entity<Agendamento>();
        ag.HasKey(x => x.Id);
        ag.Property(x => x.ProfissionalId).IsRequired();
        ag.Property(x => x.ClienteUserId).IsRequired();
        ag.Property(x => x.Data).HasColumnType("date").IsRequired();
        ag.Property(x => x.Hora).HasColumnType("time").IsRequired();
        ag.Property(x => x.DuracaoMinutos).HasDefaultValue(60).IsRequired();
        ag.Property(x => x.Status).HasConversion<int>().IsRequired();
        ag.Property(x => x.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");

        // Evita double-booking do mesmo profissional no mesmo slot
        // Índice único filtrado: permite re-agendar se o anterior estiver Cancelado (Status = 2)
        ag.HasIndex(x => new { x.ProfissionalId, x.Data, x.Hora })
            .IsUnique()
            .HasFilter("[Status] <> 2");

        // ===== Diet =====
        var diet = modelBuilder.Entity<Diet>();
        diet.HasKey(x => x.Id);
        diet.Property(x => x.ProfissionalId).IsRequired();
        diet.Property(x => x.Title).HasMaxLength(200).IsRequired();
        diet.Property(x => x.Description).HasMaxLength(500).IsRequired();
        diet.Property(x => x.Type).HasConversion<int>().IsRequired();
        diet.Property(x => x.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
        diet.Property(x => x.UpdatedAt);
        
        // Índice para buscar dietas por profissional
        diet.HasIndex(x => x.ProfissionalId);
        
        // Relacionamento com DietDayMeals
        diet.HasMany(x => x.DayMeals)
            .WithOne(x => x.Diet)
            .HasForeignKey(x => x.DietId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Relacionamento com PatientDiets
        diet.HasMany(x => x.PatientDiets)
            .WithOne(x => x.Diet)
            .HasForeignKey(x => x.DietId)
            .OnDelete(DeleteBehavior.Restrict);

        // ===== DietDayMeal =====
        var dayMeal = modelBuilder.Entity<DietDayMeal>();
        dayMeal.HasKey(x => x.Id);
        dayMeal.Property(x => x.DietId).IsRequired();
        dayMeal.Property(x => x.Day).HasMaxLength(10).IsRequired();
        dayMeal.Property(x => x.Color).HasMaxLength(20).IsRequired();
        dayMeal.Property(x => x.Breakfast).HasMaxLength(500).IsRequired();
        dayMeal.Property(x => x.MorningSnack).HasMaxLength(500).IsRequired();
        dayMeal.Property(x => x.Lunch).HasMaxLength(500).IsRequired();
        dayMeal.Property(x => x.AfternoonSnack).HasMaxLength(500).IsRequired();
        dayMeal.Property(x => x.Dinner).HasMaxLength(500).IsRequired();
        
        // Índice para buscar refeições por dieta
        dayMeal.HasIndex(x => x.DietId);

        // ===== PatientDiet =====
        var patientDiet = modelBuilder.Entity<PatientDiet>();
        patientDiet.HasKey(x => x.Id);
        patientDiet.Property(x => x.PatientUserId).IsRequired();
        patientDiet.Property(x => x.DietId).IsRequired();
        patientDiet.Property(x => x.AssignedAt).HasDefaultValueSql("SYSUTCDATETIME()");
        patientDiet.Property(x => x.StartDate).HasColumnType("date").IsRequired();
        patientDiet.Property(x => x.EndDate).HasColumnType("date");
        patientDiet.Property(x => x.IsActive).IsRequired().HasDefaultValue(true);
        
        // Índices para consultas frequentes
        patientDiet.HasIndex(x => x.PatientUserId);
        patientDiet.HasIndex(x => x.DietId);
        patientDiet.HasIndex(x => new { x.PatientUserId, x.IsActive });
    }
}
