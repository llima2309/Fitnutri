using Fitnutri.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace Fitnutri.Infrastructure
{
    public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
    {
        public DbSet<User> Users => Set<User>();

        // ... rest of the file ...

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var u = modelBuilder.Entity<User>();

            u.HasKey(x => x.Id);
            u.Property(x => x.UserName).HasMaxLength(64).IsRequired();
            u.Property(x => x.Email).HasMaxLength(256).IsRequired();
            u.Property(x => x.PasswordHash).IsRequired();
            u.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()"); // This line requires Microsoft.EntityFrameworkCore.Metadata.Builders

            u.HasIndex(x => x.UserName).IsUnique();
            u.HasIndex(x => x.Email).IsUnique();
        }
    }
}
