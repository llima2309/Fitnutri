using Fitnutri.Domain;

namespace Fitnutri.Contracts;

// ===== Requests =====

public record CreateDietRequest(
    string Title,
    string Description,
    DietType Type,
    List<DayMealRequest> DayMeals
);

public record UpdateDietRequest(
    string? Title,
    string? Description,
    DietType? Type,
    List<DayMealRequest>? DayMeals
);

public record DayMealRequest(
    string Day,
    string Color,
    string Breakfast,
    string MorningSnack,
    string Lunch,
    string AfternoonSnack,
    string Dinner
);

public record AssignDietToPatientRequest(
    Guid DietId,
    Guid PatientUserId,
    DateOnly StartDate,
    DateOnly? EndDate
);

// ===== Responses =====

public record DietResponse(
    Guid Id,
    Guid ProfissionalId,
    string Title,
    string Description,
    DietType Type,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    List<DayMealResponse> DayMeals,
    int PatientsCount = 0
);

public record DayMealResponse(
    Guid Id,
    string Day,
    string Color,
    MealResponse Meals
);

public record MealResponse(
    string Breakfast,
    string MorningSnack,
    string Lunch,
    string AfternoonSnack,
    string Dinner
);

public record PatientDietResponse(
    Guid Id,
    Guid PatientUserId,
    string PatientName,
    Guid DietId,
    string DietTitle,
    DateOnly StartDate,
    DateOnly? EndDate,
    bool IsActive,
    DateTime AssignedAt
);

public record DietSummaryResponse(
    Guid Id,
    string Title,
    string Description,
    DietType Type,
    int PatientsCount,
    DateTime CreatedAt
);

