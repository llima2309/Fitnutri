using System;
using System.Collections.Generic;
using System.Text.Json;
using Fitnutri.Contracts;
using Fitnutri.Domain;
using FluentAssertions;
using Xunit;

namespace Fitnutri.test.Contracts;

public class AgendamentoContractsTests
{
    [Fact]
    public void DisponibilidadeResponse_ShouldSerializeAndDeserialize()
    {
        // Arrange
        var horarios = new List<string> { "09:00", "10:00", "11:00", "14:00", "15:00" };
        var response = new DisponibilidadeResponse(horarios);

        // Act
        var json = JsonSerializer.Serialize(response);
        var deserialized = JsonSerializer.Deserialize<DisponibilidadeResponse>(json);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.Horarios.Should().HaveCount(5);
        deserialized.Horarios.Should().BeEquivalentTo(horarios);
    }

    [Fact]
    public void CriarAgendamentoRequest_ShouldSerializeAndDeserialize()
    {
        // Arrange
        var profissionalId = Guid.NewGuid();
        var data = DateOnly.FromDateTime(DateTime.Today.AddDays(1));
        var hora = new TimeOnly(10, 30);
        var request = new CriarAgendamentoRequest(profissionalId, data, hora);

        // Act
        var json = JsonSerializer.Serialize(request);
        var deserialized = JsonSerializer.Deserialize<CriarAgendamentoRequest>(json);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.ProfissionalId.Should().Be(profissionalId);
        deserialized.Data.Should().Be(data);
        deserialized.Hora.Should().Be(hora);
    }

    [Fact]
    public void AtualizarAgendamentoRequest_ShouldSerializeAndDeserialize()
    {
        // Arrange
        var data = DateOnly.FromDateTime(DateTime.Today.AddDays(2));
        var hora = new TimeOnly(14, 0);
        var duracao = 90;
        var status = AgendamentoStatus.Confirmado;
        var request = new AtualizarAgendamentoRequest(data, hora, duracao, status);

        // Act
        var json = JsonSerializer.Serialize(request);
        var deserialized = JsonSerializer.Deserialize<AtualizarAgendamentoRequest>(json);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.Data.Should().Be(data);
        deserialized.Hora.Should().Be(hora);
        deserialized.DuracaoMinutos.Should().Be(duracao);
        deserialized.Status.Should().Be(status);
    }

    [Fact]
    public void AtualizarAgendamentoRequest_ShouldAllowNullValues()
    {
        // Arrange
        var request = new AtualizarAgendamentoRequest(null, null, null, null);

        // Act
        var json = JsonSerializer.Serialize(request);
        var deserialized = JsonSerializer.Deserialize<AtualizarAgendamentoRequest>(json);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.Data.Should().BeNull();
        deserialized.Hora.Should().BeNull();
        deserialized.DuracaoMinutos.Should().BeNull();
        deserialized.Status.Should().BeNull();
    }

    [Fact]
    public void AgendamentoResponse_ShouldSerializeAndDeserialize()
    {
        // Arrange
        var id = Guid.NewGuid();
        var profissionalId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var data = DateOnly.FromDateTime(DateTime.Today.AddDays(1));
        var hora = new TimeOnly(15, 0);
        var duracao = 60;
        var status = AgendamentoStatus.Pendente;
        var profissionalNome = "Dr. João Silva";
        var profissionalPerfil = "Nutricionista";

        var response = new AgendamentoResponse(
            id, profissionalId, clienteId, data, hora, duracao, status, 
            profissionalNome, profissionalPerfil);

        // Act
        var json = JsonSerializer.Serialize(response);
        var deserialized = JsonSerializer.Deserialize<AgendamentoResponse>(json);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.Id.Should().Be(id);
        deserialized.ProfissionalId.Should().Be(profissionalId);
        deserialized.ClienteUserId.Should().Be(clienteId);
        deserialized.Data.Should().Be(data);
        deserialized.Hora.Should().Be(hora);
        deserialized.DuracaoMinutos.Should().Be(duracao);
        deserialized.Status.Should().Be(status);
        deserialized.ProfissionalNome.Should().Be(profissionalNome);
        deserialized.ProfissionalPerfil.Should().Be(profissionalPerfil);
    }

    [Fact]
    public void AgendamentoResponse_ShouldAllowNullOptionalFields()
    {
        // Arrange
        var id = Guid.NewGuid();
        var profissionalId = Guid.NewGuid();
        var clienteId = Guid.NewGuid();
        var data = DateOnly.FromDateTime(DateTime.Today.AddDays(1));
        var hora = new TimeOnly(15, 0);
        var duracao = 60;
        var status = AgendamentoStatus.Pendente;

        var response = new AgendamentoResponse(
            id, profissionalId, clienteId, data, hora, duracao, status);

        // Act
        var json = JsonSerializer.Serialize(response);
        var deserialized = JsonSerializer.Deserialize<AgendamentoResponse>(json);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.ProfissionalNome.Should().BeNull();
        deserialized.ProfissionalPerfil.Should().BeNull();
    }

    [Theory]
    [InlineData(AgendamentoStatus.Pendente)]
    [InlineData(AgendamentoStatus.Confirmado)]
    [InlineData(AgendamentoStatus.Cancelado)]
    public void AgendamentoStatus_ShouldSerializeCorrectly(AgendamentoStatus status)
    {
        // Arrange
        var request = new AtualizarAgendamentoRequest(null, null, null, status);

        // Act
        var json = JsonSerializer.Serialize(request);
        var deserialized = JsonSerializer.Deserialize<AtualizarAgendamentoRequest>(json);

        // Assert
        deserialized!.Status.Should().Be(status);
    }

    [Fact]
    public void DisponibilidadeResponse_ShouldHandleEmptyList()
    {
        // Arrange
        var horarios = new List<string>();
        var response = new DisponibilidadeResponse(horarios);

        // Act
        var json = JsonSerializer.Serialize(response);
        var deserialized = JsonSerializer.Deserialize<DisponibilidadeResponse>(json);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.Horarios.Should().BeEmpty();
    }

    [Fact]
    public void CriarAgendamentoRequest_ShouldHandleDateTimeFormats()
    {
        // Arrange
        var profissionalId = Guid.NewGuid();
        var data = new DateOnly(2025, 12, 25); // Christmas
        var hora = new TimeOnly(9, 30, 0); // 9:30 AM
        var request = new CriarAgendamentoRequest(profissionalId, data, hora);

        // Act
        var json = JsonSerializer.Serialize(request);
        var deserialized = JsonSerializer.Deserialize<CriarAgendamentoRequest>(json);

        // Assert
        deserialized.Should().NotBeNull();
        deserialized!.Data.Year.Should().Be(2025);
        deserialized.Data.Month.Should().Be(12);
        deserialized.Data.Day.Should().Be(25);
        deserialized.Hora.Hour.Should().Be(9);
        deserialized.Hora.Minute.Should().Be(30);
    }

    [Fact]
    public void AgendamentoResponse_ShouldPreserveAllFields()
    {
        // This test ensures all fields are properly serialized/deserialized
        
        // Arrange
        var response = new AgendamentoResponse(
            Id: Guid.NewGuid(),
            ProfissionalId: Guid.NewGuid(),
            ClienteUserId: Guid.NewGuid(),
            Data: DateOnly.FromDateTime(DateTime.Today.AddDays(1)),
            Hora: new TimeOnly(10, 0),
            DuracaoMinutos: 90,
            Status: AgendamentoStatus.Confirmado,
            ProfissionalNome: "Dra. Maria Santos",
            ProfissionalPerfil: "Personal Trainer"
        );

        // Act
        var json = JsonSerializer.Serialize(response);
        var deserialized = JsonSerializer.Deserialize<AgendamentoResponse>(json);

        // Assert
        deserialized.Should().BeEquivalentTo(response);
    }
}
