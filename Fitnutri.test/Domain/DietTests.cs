using System;
using System.Collections.Generic;
using Fitnutri.Domain;
using FluentAssertions;
using Xunit;

namespace Fitnutri.test.Domain;

public class DietTests
{
    [Fact]
    public void Diet_ShouldBeCreatedWithRequiredFields()
    {
        // Arrange
        var profissionalId = Guid.NewGuid();
        var title = "Dieta Low Carb";
        var description = "Dieta com baixo teor de carboidratos";

        // Act
        var diet = new Diet
        {
            Id = Guid.NewGuid(),
            ProfissionalId = profissionalId,
            Title = title,
            Description = description,
            Type = DietType.LowCarb
        };

        // Assert
        diet.Id.Should().NotBe(Guid.Empty);
        diet.ProfissionalId.Should().Be(profissionalId);
        diet.Title.Should().Be(title);
        diet.Description.Should().Be(description);
        diet.Type.Should().Be(DietType.LowCarb);
        diet.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        diet.DayMeals.Should().NotBeNull().And.BeEmpty();
        diet.PatientDiets.Should().NotBeNull().And.BeEmpty();
    }

    [Theory]
    [InlineData(DietType.Keto)]
    [InlineData(DietType.LowCarb)]
    [InlineData(DietType.Vegan)]
    [InlineData(DietType.Celiac)]
    [InlineData(DietType.Vegetarian)]
    public void Diet_ShouldAcceptAllDietTypes(DietType type)
    {
        // Arrange & Act
        var diet = new Diet
        {
            Id = Guid.NewGuid(),
            ProfissionalId = Guid.NewGuid(),
            Title = "Teste",
            Description = "Teste",
            Type = type
        };

        // Assert
        diet.Type.Should().Be(type);
    }

    [Fact]
    public void Diet_ShouldAllowUpdates()
    {
        // Arrange
        var diet = new Diet
        {
            Id = Guid.NewGuid(),
            ProfissionalId = Guid.NewGuid(),
            Title = "Título Original",
            Description = "Descrição Original",
            Type = DietType.LowCarb
        };

        // Act
        diet.Title = "Título Atualizado";
        diet.Description = "Descrição Atualizada";
        diet.Type = DietType.Keto;
        diet.UpdatedAt = DateTime.UtcNow;

        // Assert
        diet.Title.Should().Be("Título Atualizado");
        diet.Description.Should().Be("Descrição Atualizada");
        diet.Type.Should().Be(DietType.Keto);
        diet.UpdatedAt.Should().NotBeNull();
    }
}

public class DietDayMealTests
{
    [Fact]
    public void DietDayMeal_ShouldBeCreatedWithAllMeals()
    {
        // Arrange
        var dietId = Guid.NewGuid();
        var day = "SEG";
        var color = "#FF5733";

        // Act
        var dayMeal = new DietDayMeal
        {
            Id = Guid.NewGuid(),
            DietId = dietId,
            Day = day,
            Color = color,
            Breakfast = "Ovos mexidos com abacate",
            MorningSnack = "Castanhas",
            Lunch = "Salmão grelhado com salada",
            AfternoonSnack = "Iogurte natural",
            Dinner = "Frango com legumes"
        };

        // Assert
        dayMeal.Id.Should().NotBe(Guid.Empty);
        dayMeal.DietId.Should().Be(dietId);
        dayMeal.Day.Should().Be(day);
        dayMeal.Color.Should().Be(color);
        dayMeal.Breakfast.Should().Be("Ovos mexidos com abacate");
        dayMeal.MorningSnack.Should().Be("Castanhas");
        dayMeal.Lunch.Should().Be("Salmão grelhado com salada");
        dayMeal.AfternoonSnack.Should().Be("Iogurte natural");
        dayMeal.Dinner.Should().Be("Frango com legumes");
    }

    [Theory]
    [InlineData("SEG")]
    [InlineData("TER")]
    [InlineData("QUA")]
    [InlineData("QUI")]
    [InlineData("SEX")]
    [InlineData("SAB")]
    [InlineData("DOM")]
    public void DietDayMeal_ShouldAcceptAllDaysOfWeek(string day)
    {
        // Arrange & Act
        var dayMeal = new DietDayMeal
        {
            Id = Guid.NewGuid(),
            DietId = Guid.NewGuid(),
            Day = day
        };

        // Assert
        dayMeal.Day.Should().Be(day);
    }

    [Fact]
    public void DietDayMeal_ShouldAllowEmptyMeals()
    {
        // Arrange & Act
        var dayMeal = new DietDayMeal
        {
            Id = Guid.NewGuid(),
            DietId = Guid.NewGuid(),
            Day = "SEG"
        };

        // Assert
        dayMeal.Breakfast.Should().Be(string.Empty);
        dayMeal.MorningSnack.Should().Be(string.Empty);
        dayMeal.Lunch.Should().Be(string.Empty);
        dayMeal.AfternoonSnack.Should().Be(string.Empty);
        dayMeal.Dinner.Should().Be(string.Empty);
    }
}

public class PatientDietTests
{
    [Fact]
    public void PatientDiet_ShouldBeCreatedWithRequiredFields()
    {
        // Arrange
        var patientId = Guid.NewGuid();
        var dietId = Guid.NewGuid();
        var startDate = DateOnly.FromDateTime(DateTime.Today);

        // Act
        var patientDiet = new PatientDiet
        {
            Id = Guid.NewGuid(),
            PatientUserId = patientId,
            DietId = dietId,
            StartDate = startDate
        };

        // Assert
        patientDiet.Id.Should().NotBe(Guid.Empty);
        patientDiet.PatientUserId.Should().Be(patientId);
        patientDiet.DietId.Should().Be(dietId);
        patientDiet.StartDate.Should().Be(startDate);
        patientDiet.AssignedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(1));
        patientDiet.IsActive.Should().BeTrue();
        patientDiet.EndDate.Should().BeNull();
    }

    [Fact]
    public void PatientDiet_ShouldAllowEndDate()
    {
        // Arrange
        var patientDiet = new PatientDiet
        {
            Id = Guid.NewGuid(),
            PatientUserId = Guid.NewGuid(),
            DietId = Guid.NewGuid(),
            StartDate = DateOnly.FromDateTime(DateTime.Today)
        };

        var endDate = DateOnly.FromDateTime(DateTime.Today.AddDays(30));

        // Act
        patientDiet.EndDate = endDate;

        // Assert
        patientDiet.EndDate.Should().Be(endDate);
    }

    [Fact]
    public void PatientDiet_ShouldAllowDeactivation()
    {
        // Arrange
        var patientDiet = new PatientDiet
        {
            Id = Guid.NewGuid(),
            PatientUserId = Guid.NewGuid(),
            DietId = Guid.NewGuid(),
            StartDate = DateOnly.FromDateTime(DateTime.Today),
            IsActive = true
        };

        // Act
        patientDiet.IsActive = false;
        patientDiet.EndDate = DateOnly.FromDateTime(DateTime.Today);

        // Assert
        patientDiet.IsActive.Should().BeFalse();
        patientDiet.EndDate.Should().NotBeNull();
    }

    [Fact]
    public void PatientDiet_ShouldValidateDateRange()
    {
        // Arrange
        var startDate = DateOnly.FromDateTime(DateTime.Today);
        var endDate = DateOnly.FromDateTime(DateTime.Today.AddDays(30));

        // Act
        var patientDiet = new PatientDiet
        {
            Id = Guid.NewGuid(),
            PatientUserId = Guid.NewGuid(),
            DietId = Guid.NewGuid(),
            StartDate = startDate,
            EndDate = endDate
        };

        // Assert
        patientDiet.StartDate.Should().BeBefore((DateOnly)patientDiet.EndDate);
    }
}
