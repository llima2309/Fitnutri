namespace Fitnutri.Domain;

public enum DietType
{
    Keto = 0,
    LowCarb = 1,
    Vegan = 2,
    Celiac = 3,
    Vegetarian = 4
}

/// <summary>
/// Representa uma dieta criada por um nutricionista
/// </summary>
public class Diet
{
    public Guid Id { get; set; }
    
    /// <summary>
    /// ID do profissional (nutricionista) que criou a dieta
    /// </summary>
    public Guid ProfissionalId { get; set; }
    
    /// <summary>
    /// Nome/Título da dieta
    /// </summary>
    public string Title { get; set; } = string.Empty;
    
    /// <summary>
    /// Descrição da dieta
    /// </summary>
    public string Description { get; set; } = string.Empty;
    
    /// <summary>
    /// Tipo da dieta (Keto, LowCarb, etc)
    /// </summary>
    public DietType Type { get; set; }
    
    /// <summary>
    /// Data de criação
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Data de última atualização
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
    
    /// <summary>
    /// Refeições da dieta (7 dias x 5 refeições)
    /// </summary>
    public List<DietDayMeal> DayMeals { get; set; } = new();
    
    /// <summary>
    /// Pacientes que estão usando esta dieta
    /// </summary>
    public List<PatientDiet> PatientDiets { get; set; } = new();
}

/// <summary>
/// Representa as refeições de um dia específico da semana
/// </summary>
public class DietDayMeal
{
    public Guid Id { get; set; }
    
    public Guid DietId { get; set; }
    public Diet Diet { get; set; } = null!;
    
    /// <summary>
    /// Dia da semana (SEG, TER, QUA, QUI, SEX, SAB, DOM)
    /// </summary>
    public string Day { get; set; } = string.Empty;
    
    /// <summary>
    /// Cor para exibição no app
    /// </summary>
    public string Color { get; set; } = string.Empty;
    
    /// <summary>
    /// Café da manhã
    /// </summary>
    public string Breakfast { get; set; } = string.Empty;
    
    /// <summary>
    /// Lanche da manhã
    /// </summary>
    public string MorningSnack { get; set; } = string.Empty;
    
    /// <summary>
    /// Almoço
    /// </summary>
    public string Lunch { get; set; } = string.Empty;
    
    /// <summary>
    /// Lanche da tarde
    /// </summary>
    public string AfternoonSnack { get; set; } = string.Empty;
    
    /// <summary>
    /// Jantar
    /// </summary>
    public string Dinner { get; set; } = string.Empty;
}

/// <summary>
/// Representa a associação entre um paciente e uma dieta
/// </summary>
public class PatientDiet
{
    public Guid Id { get; set; }
    
    /// <summary>
    /// ID do paciente (User.Id)
    /// </summary>
    public Guid PatientUserId { get; set; }
    
    /// <summary>
    /// ID da dieta
    /// </summary>
    public Guid DietId { get; set; }
    public Diet Diet { get; set; } = null!;
    
    /// <summary>
    /// Data em que a dieta foi atribuída ao paciente
    /// </summary>
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Data de início da dieta
    /// </summary>
    public DateOnly StartDate { get; set; }
    
    /// <summary>
    /// Data de término da dieta (opcional)
    /// </summary>
    public DateOnly? EndDate { get; set; }
    
    /// <summary>
    /// Se a dieta está ativa para o paciente
    /// </summary>
    public bool IsActive { get; set; } = true;
}

