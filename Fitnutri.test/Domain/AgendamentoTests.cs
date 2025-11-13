using System;
using Fitnutri.Domain;
using FluentAssertions;
using Xunit;

namespace Fitnutri.test.Domain;

public class AgendamentoTests
{
    [Fact]
    public void Agendamento_ShouldBeCreatedWithCorrectDefaults()
    {
        // Arrange
        var profissionalId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var data = DateOnly.FromDateTime(DateTime.Today.AddDays(1));
        var hora = new TimeOnly(10, 0);

        // Act
        var agendamento = new Agendamento
        {
            Id = Guid.NewGuid(),
            ProfissionalId = profissionalId,
            ClienteUserId = clienteId,
            Data = data,
            Hora = hora
        };

        // Assert
        agendamento.Id.Should().NotBe(Guid.Empty);
        agendamento.ProfissionalId.Should().Be(profissionalId);
        agendamento.ClienteUserId.Should().Be(clienteId);
        agendamento.Data.Should().Be(data);
        agendamento.Hora.Should().Be(hora);
        agendamento.DuracaoMinutos.Should().Be(60); // Valor padrão
        agendamento.Status.Should().Be(AgendamentoStatus.Pendente); // Status padrão
        agendamento.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
    }

    [Theory]
    [InlineData(AgendamentoStatus.Pendente)]
    [InlineData(AgendamentoStatus.Confirmado)]
    [InlineData(AgendamentoStatus.Cancelado)]
    public void Agendamento_ShouldAllowAllStatusValues(AgendamentoStatus status)
    {
        // Arrange & Act
        var agendamento = new Agendamento
        {
            Id = Guid.NewGuid(),
            ProfissionalId = Guid.NewGuid(),
            ClienteUserId = Guid.NewGuid(),
            Data = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            Hora = new TimeOnly(10, 0),
            Status = status
        };

        // Assert
        agendamento.Status.Should().Be(status);
    }

    [Theory]
    [InlineData(30)]
    [InlineData(60)]
    [InlineData(90)]
    [InlineData(120)]
    public void Agendamento_ShouldAllowDifferentDurations(int duracao)
    {
        // Arrange & Act
        var agendamento = new Agendamento
        {
            Id = Guid.NewGuid(),
            ProfissionalId = Guid.NewGuid(),
            ClienteUserId = Guid.NewGuid(),
            Data = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            Hora = new TimeOnly(10, 0),
            DuracaoMinutos = duracao
        };

        // Assert
        agendamento.DuracaoMinutos.Should().Be(duracao);
    }

    [Fact]
    public void Agendamento_ShouldAllowStatusUpdate()
    {
        // Arrange
        var agendamento = new Agendamento
        {
            Id = Guid.NewGuid(),
            ProfissionalId = Guid.NewGuid(),
            ClienteUserId = Guid.NewGuid(),
            Data = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            Hora = new TimeOnly(10, 0),
            Status = AgendamentoStatus.Pendente
        };

        // Act
        agendamento.Status = AgendamentoStatus.Confirmado;

        // Assert
        agendamento.Status.Should().Be(AgendamentoStatus.Confirmado);
    }

    [Fact]
    public void Agendamento_ShouldAllowCancellation()
    {
        // Arrange
        var agendamento = new Agendamento
        {
            Id = Guid.NewGuid(),
            ProfissionalId = Guid.NewGuid(),
            ClienteUserId = Guid.NewGuid(),
            Data = DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            Hora = new TimeOnly(10, 0),
            Status = AgendamentoStatus.Confirmado
        };

        // Act
        agendamento.Status = AgendamentoStatus.Cancelado;

        // Assert
        agendamento.Status.Should().Be(AgendamentoStatus.Cancelado);
    }

    [Fact]
    public void Agendamento_ShouldAcceptBusinessHours()
    {
        // Arrange & Act
        var agendamentos = new[]
        {
            new Agendamento { Hora = new TimeOnly(9, 0) },
            new Agendamento { Hora = new TimeOnly(12, 30) },
            new Agendamento { Hora = new TimeOnly(17, 0) }
        };

        // Assert
        agendamentos[0].Hora.Should().Be(new TimeOnly(9, 0));
        agendamentos[1].Hora.Should().Be(new TimeOnly(12, 30));
        agendamentos[2].Hora.Should().Be(new TimeOnly(17, 0));
    }

    [Fact]
    public void Agendamento_ShouldAcceptFutureDates()
    {
        // Arrange
        var futureDate = DateOnly.FromDateTime(DateTime.Today.AddDays(30));

        // Act
        var agendamento = new Agendamento
        {
            Data = futureDate
        };

        // Assert
        agendamento.Data.Should().Be(futureDate);
        agendamento.Data.Should().BeAfter(DateOnly.FromDateTime(DateTime.Today));
    }
}
