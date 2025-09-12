using System;
using System.Threading.Tasks;
using Fitnutri.Domain;
using Fitnutri.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Fitnutri.test;

public class PerfilTests
{
    private DbContextOptions<AppDbContext> GetInMemoryOptions()
    {
        return new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public async Task PodeCriarPerfil()
    {
        var options = GetInMemoryOptions();
        using var db = new AppDbContext(options);
        var perfil = new Perfil { Nome = "Teste", Tipo = PerfilTipo.Paciente };
        db.Perfis.Add(perfil);
        await db.SaveChangesAsync();
        Assert.True(perfil.Id != Guid.Empty);
        Assert.Equal("Teste", perfil.Nome);
    }

    [Fact]
    public async Task PodeListarPerfis()
    {
        var options = GetInMemoryOptions();
        using var db = new AppDbContext(options);
        db.Perfis.Add(new Perfil { Nome = "Nutricionista", Tipo = PerfilTipo.Nutricionista });
        db.Perfis.Add(new Perfil { Nome = "Paciente", Tipo = PerfilTipo.Paciente });
        await db.SaveChangesAsync();
        var perfis = await db.Perfis.ToListAsync();
        Assert.Equal(2, perfis.Count);
    }

    [Fact]
    public async Task PodeEditarPerfil()
    {
        var options = GetInMemoryOptions();
        using var db = new AppDbContext(options);
        var perfil = new Perfil { Nome = "Personal", Tipo = PerfilTipo.PersonalTrainer };
        db.Perfis.Add(perfil);
        await db.SaveChangesAsync();
        perfil.Nome = "PersonalTrainer";
        db.Perfis.Update(perfil);
        await db.SaveChangesAsync();
        var atualizado = await db.Perfis.FindAsync(perfil.Id);
        Assert.Equal("PersonalTrainer", atualizado.Nome);
    }

    [Fact]
    public async Task PodeExcluirPerfil()
    {
        var options = GetInMemoryOptions();
        using var db = new AppDbContext(options);
        var perfil = new Perfil { Nome = "Administrador", Tipo = PerfilTipo.Administrador };
        db.Perfis.Add(perfil);
        await db.SaveChangesAsync();
        db.Perfis.Remove(perfil);
        await db.SaveChangesAsync();
        var excluido = await db.Perfis.FindAsync(perfil.Id);
        Assert.Null(excluido);
    }
}

