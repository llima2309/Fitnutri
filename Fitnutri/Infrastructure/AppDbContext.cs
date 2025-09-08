using Fitnutri.Domain;
using Microsoft.EntityFrameworkCore;

namespace Fitnutri.Infrastructure;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

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
        u.Property(x => x.EmailVerificationToken).HasMaxLength(200);

        // approval
        u.Property(x => x.Status).HasConversion<int>().IsRequired();
        u.Property(x => x.ApprovedAt);
        u.Property(x => x.ApprovedBy).HasMaxLength(128);

        u.Property(x => x.Role).HasConversion<int>().IsRequired();
    }
}
