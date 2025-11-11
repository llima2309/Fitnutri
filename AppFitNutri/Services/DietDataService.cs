using AppFitNutri.Models;

namespace AppFitNutri.Services;

public static class DietDataService
{
    public static DietPlan GetKetoDietPlan()
    {
        return new DietPlan
        {
            Type = DietType.Keto,
            Title = "Dieta Cetogênica",
            Description = "Prioriza a ingestão de gorduras e proteínas, minimizando drasticamente os carboidratos",
            DayMeals = new List<DayMeal>
            {
                new DayMeal
                {
                    Day = "SEG",
                    Color = "#8B5CF6",
                    Meals = new Meal
                    {
                        Breakfast = "Omelete com queijo, bacon e espinafre. Café preto.",
                        MorningSnack = "1/2 Abacate com sal e azeite.",
                        Lunch = "Salmão grelhado com brócolis na manteiga.",
                        AfternoonSnack = "Punhado de nozes e amêndoas.",
                        Dinner = "Frango assado com couve-flor gratinada em creme de leite."
                    }
                },
                new DayMeal
                {
                    Day = "TER",
                    Color = "#EC4899",
                    Meals = new Meal
                    {
                        Breakfast = "Ovos mexidos com queijo mussarela e tomate. Chá de ervas.",
                        MorningSnack = "Iogurte natural integral (sem açúcar) com sementes de chia.",
                        Lunch = "Carne moída refogada com abobrinha.",
                        AfternoonSnack = "6 morangos com creme de leite.",
                        Dinner = "Hambúrguer caseiro sem pão, salada de alface e pepino."
                    }
                },
                new DayMeal
                {
                    Day = "QUA",
                    Color = "#6366F1",
                    Meals = new Meal
                    {
                        Breakfast = "Café com óleo de coco (bulletproof coffee).",
                        MorningSnack = "Queijo tipo boursin ou cream cheese com aipo.",
                        Lunch = "Bife de alcatra grelhado com salada de rúcula.",
                        AfternoonSnack = "Vitamina de abacate (com água e adoçante natural).",
                        Dinner = "Sopa cremosa de abóbora com toque de gengibre e azeite."
                    }
                },
                new DayMeal
                {
                    Day = "QUI",
                    Color = "#3B82F6",
                    Meals = new Meal
                    {
                        Breakfast = "Panqueca low carb (farinha de amêndoa) com manteiga.",
                        MorningSnack = "Ovos de codorna cozidos.",
                        Lunch = "Sobrecoxa de frango assada com pimentão, cebola e azeite.",
                        AfternoonSnack = "10 framboesas e 1 fatia de queijo.",
                        Dinner = "Omelete de queijo e presunto (ou peito de peru)."
                    }
                },
                new DayMeal
                {
                    Day = "SEX",
                    Color = "#10B981",
                    Meals = new Meal
                    {
                        Breakfast = "Ovos fritos na manteiga com queijo coalho.",
                        MorningSnack = "Azeitonas e 3 fatias de salame.",
                        Lunch = "Almôndegas de carne com macarrão de abobrinha ao molho de tomate caseiro.",
                        AfternoonSnack = "Amêndoas e queijo parmesão em cubos.",
                        Dinner = "Tilápia ou outro peixe grelhado com aspargos."
                    }
                },
                new DayMeal
                {
                    Day = "SÁB",
                    Color = "#F97316",
                    Meals = new Meal
                    {
                        Breakfast = "Vitamina de morango, leite de coco e nozes.",
                        MorningSnack = "1/2 abacate com pasta de amendoim.",
                        Lunch = "Costelinha de porco assada com repolho refogado na manteiga.",
                        AfternoonSnack = "Palitos de pepino com patê de atum.",
                        Dinner = "Salada de atum, maionese caseira e vegetais low carb."
                    }
                },
                new DayMeal
                {
                    Day = "DOM",
                    Color = "#EF4444",
                    Meals = new Meal
                    {
                        Breakfast = "Café preto e pão de queijo low carb (farinha de queijo e ovos).",
                        MorningSnack = "Queijo ementhal ou prato em cubos.",
                        Lunch = "Churrasco (picanha, linguiça) com salada de folhas e azeite.",
                        AfternoonSnack = "Ovos mexidos com orégano.",
                        Dinner = "Caldo de frango com ovos e couve."
                    }
                }
            }
        };
    }

    public static DietPlan GetLowCarbDietPlan()
    {
        return new DietPlan
        {
            Type = DietType.LowCarb,
            Title = "Dieta Low Carb",
            Description = "Reduz carboidratos mantendo equilíbrio nutricional",
            DayMeals = new List<DayMeal>
            {
                new DayMeal
                {
                    Day = "SEG",
                    Color = "#8B5CF6",
                    Meals = new Meal
                    {
                        Breakfast = "Ovos mexidos com tomate e queijo. Café com leite.",
                        MorningSnack = "Iogurte natural com nozes.",
                        Lunch = "Frango grelhado com salada verde e batata doce pequena.",
                        AfternoonSnack = "Maçã com pasta de amendoim.",
                        Dinner = "Peixe assado com legumes no vapor."
                    }
                },
                new DayMeal
                {
                    Day = "TER",
                    Color = "#EC4899",
                    Meals = new Meal
                    {
                        Breakfast = "Tapioca com queijo branco e ovo. Chá verde.",
                        MorningSnack = "Cenoura baby com homus.",
                        Lunch = "Carne moída com abobrinha e arroz integral (3 colheres).",
                        AfternoonSnack = "Mix de castanhas.",
                        Dinner = "Omelete de legumes com salada."
                    }
                },
                new DayMeal
                {
                    Day = "QUA",
                    Color = "#6366F1",
                    Meals = new Meal
                    {
                        Breakfast = "Pão integral com abacate e ovo pochê.",
                        MorningSnack = "Queijo cottage com tomate cereja.",
                        Lunch = "Salmão grelhado com quinoa e brócolis.",
                        AfternoonSnack = "Smoothie de frutas vermelhas.",
                        Dinner = "Sopa de legumes com frango desfiado."
                    }
                },
                new DayMeal
                {
                    Day = "QUI",
                    Color = "#3B82F6",
                    Meals = new Meal
                    {
                        Breakfast = "Mingau de aveia com frutas vermelhas.",
                        MorningSnack = "Palitos de queijo minas.",
                        Lunch = "Bife com purê de abóbora e salada.",
                        AfternoonSnack = "Iogurte grego com sementes de chia.",
                        Dinner = "Frango ao curry com couve-flor."
                    }
                },
                new DayMeal
                {
                    Day = "SEX",
                    Color = "#10B981",
                    Meals = new Meal
                    {
                        Breakfast = "Panqueca de banana com aveia e canela.",
                        MorningSnack = "Pepino com cream cheese.",
                        Lunch = "Tilápia grelhada com legumes e arroz integral.",
                        AfternoonSnack = "Frutas vermelhas com chantilly.",
                        Dinner = "Almôndegas com abobrinha espaguete."
                    }
                },
                new DayMeal
                {
                    Day = "SÁB",
                    Color = "#F97316",
                    Meals = new Meal
                    {
                        Breakfast = "Vitamina de abacate com aveia.",
                        MorningSnack = "Azeitonas e queijo.",
                        Lunch = "Costeleta suína com salada e mandioca (pequena porção).",
                        AfternoonSnack = "Torrada integral com pasta de atum.",
                        Dinner = "Salada caprese com peito de frango."
                    }
                },
                new DayMeal
                {
                    Day = "DOM",
                    Color = "#EF4444",
                    Meals = new Meal
                    {
                        Breakfast = "Crepioca com queijo e tomate.",
                        MorningSnack = "Mamão com granola.",
                        Lunch = "Feijoada light com arroz integral e couve.",
                        AfternoonSnack = "Bolo de cenoura fit (1 fatia).",
                        Dinner = "Pizza de frigideira com massa de frango."
                    }
                }
            }
        };
    }

    public static DietPlan GetVeganDietPlan()
    {
        return new DietPlan
        {
            Type = DietType.Vegan,
            Title = "Dieta Vegana",
            Description = "100% plant-based, rica em vegetais e grãos",
            DayMeals = new List<DayMeal>
            {
                new DayMeal
                {
                    Day = "SEG",
                    Color = "#8B5CF6",
                    Meals = new Meal
                    {
                        Breakfast = "Smoothie de banana, espinafre e leite de amêndoas.",
                        MorningSnack = "Frutas vermelhas com sementes de chia.",
                        Lunch = "Grão-de-bico ao curry com arroz integral e salada.",
                        AfternoonSnack = "Homus com palitos de cenoura.",
                        Dinner = "Tofu grelhado com legumes salteados."
                    }
                },
                new DayMeal
                {
                    Day = "TER",
                    Color = "#EC4899",
                    Meals = new Meal
                    {
                        Breakfast = "Mingau de aveia com leite de coco e frutas.",
                        MorningSnack = "Mix de castanhas e frutas secas.",
                        Lunch = "Hambúrguer de lentilha com batata doce assada.",
                        AfternoonSnack = "Vitamina de açaí com banana.",
                        Dinner = "Macarrão integral ao sugo com cogumelos."
                    }
                },
                new DayMeal
                {
                    Day = "QUA",
                    Color = "#6366F1",
                    Meals = new Meal
                    {
                        Breakfast = "Panqueca de banana com pasta de amendoim.",
                        MorningSnack = "Iogurte de soja com granola.",
                        Lunch = "Feijoada vegana com arroz e couve.",
                        AfternoonSnack = "Barrinha de cereais vegana.",
                        Dinner = "Sopa de lentilha com legumes."
                    }
                },
                new DayMeal
                {
                    Day = "QUI",
                    Color = "#3B82F6",
                    Meals = new Meal
                    {
                        Breakfast = "Tapioca com pasta de amendoim e banana.",
                        MorningSnack = "Maçã com manteiga de amêndoas.",
                        Lunch = "Quinoa com legumes assados e tahine.",
                        AfternoonSnack = "Chips de grão-de-bico.",
                        Dinner = "Curry de grão-de-bico com arroz."
                    }
                },
                new DayMeal
                {
                    Day = "SEX",
                    Color = "#10B981",
                    Meals = new Meal
                    {
                        Breakfast = "Açaí com granola e frutas.",
                        MorningSnack = "Barra de proteína vegana.",
                        Lunch = "Wrap de falafel com salada e tahine.",
                        AfternoonSnack = "Smoothie verde com espinafre.",
                        Dinner = "Berinjela à parmegiana vegana."
                    }
                },
                new DayMeal
                {
                    Day = "SÁB",
                    Color = "#F97316",
                    Meals = new Meal
                    {
                        Breakfast = "Pão integral com abacate e tomate.",
                        MorningSnack = "Salada de frutas com coco ralado.",
                        Lunch = "Risoto de cogumelos com creme de castanha.",
                        AfternoonSnack = "Pipoca caseira com azeite.",
                        Dinner = "Pizza vegana com massa integral."
                    }
                },
                new DayMeal
                {
                    Day = "DOM",
                    Color = "#EF4444",
                    Meals = new Meal
                    {
                        Breakfast = "Crepioca com guacamole.",
                        MorningSnack = "Bolo de banana fit.",
                        Lunch = "Lasanha de berinjela vegana.",
                        AfternoonSnack = "Mousse de chocolate vegano.",
                        Dinner = "Salada caesar vegana com grão-de-bico crocante."
                    }
                }
            }
        };
    }

    public static DietPlan GetCeliacDietPlan()
    {
        return new DietPlan
        {
            Type = DietType.Celiac,
            Title = "Dieta para Celíacos",
            Description = "Isenta de glúten, ingredientes certificados",
            DayMeals = new List<DayMeal>
            {
                new DayMeal
                {
                    Day = "SEG",
                    Color = "#8B5CF6",
                    Meals = new Meal
                    {
                        Breakfast = "Tapioca com queijo e ovo. Suco natural.",
                        MorningSnack = "Frutas frescas variadas.",
                        Lunch = "Arroz com feijão, frango grelhado e salada.",
                        AfternoonSnack = "Iogurte natural com mel.",
                        Dinner = "Peixe assado com batata doce e legumes."
                    }
                },
                new DayMeal
                {
                    Day = "TER",
                    Color = "#EC4899",
                    Meals = new Meal
                    {
                        Breakfast = "Mingau de quinoa com frutas.",
                        MorningSnack = "Queijo branco com tomate.",
                        Lunch = "Polenta com molho de carne e legumes.",
                        AfternoonSnack = "Mix de castanhas sem glúten.",
                        Dinner = "Omelete com legumes e salada."
                    }
                },
                new DayMeal
                {
                    Day = "QUA",
                    Color = "#6366F1",
                    Meals = new Meal
                    {
                        Breakfast = "Vitamina de frutas com aveia sem glúten.",
                        MorningSnack = "Biscoito de arroz com pasta de amendoim.",
                        Lunch = "Arroz integral com lentilha e carne cozida.",
                        AfternoonSnack = "Pipoca caseira.",
                        Dinner = "Sopa de legumes com frango."
                    }
                },
                new DayMeal
                {
                    Day = "QUI",
                    Color = "#3B82F6",
                    Meals = new Meal
                    {
                        Breakfast = "Crepioca com banana e canela.",
                        MorningSnack = "Iogurte com frutas vermelhas.",
                        Lunch = "Batata assada recheada com carne moída.",
                        AfternoonSnack = "Bolo de fubá sem glúten.",
                        Dinner = "Risoto de frango com legumes."
                    }
                },
                new DayMeal
                {
                    Day = "SEX",
                    Color = "#10B981",
                    Meals = new Meal
                    {
                        Breakfast = "Pão de queijo com café com leite.",
                        MorningSnack = "Mamão com granola sem glúten.",
                        Lunch = "Carne assada com purê de mandioquinha.",
                        AfternoonSnack = "Smoothie de morango.",
                        Dinner = "Hambúrguer caseiro (sem pão) com salada."
                    }
                },
                new DayMeal
                {
                    Day = "SÁB",
                    Color = "#F97316",
                    Meals = new Meal
                    {
                        Breakfast = "Mingau de arroz com frutas.",
                        MorningSnack = "Queijo com geleia sem glúten.",
                        Lunch = "Feijoada com arroz branco e farofa de mandioca.",
                        AfternoonSnack = "Brigadeiro sem glúten.",
                        Dinner = "Pizza de arroz com recheio a gosto."
                    }
                },
                new DayMeal
                {
                    Day = "DOM",
                    Color = "#EF4444",
                    Meals = new Meal
                    {
                        Breakfast = "Tapioca doce com coco e leite condensado.",
                        MorningSnack = "Salada de frutas.",
                        Lunch = "Macarrão de arroz com molho bolonhesa.",
                        AfternoonSnack = "Bolo de cenoura sem glúten.",
                        Dinner = "Frango ao forno com batatas."
                    }
                }
            }
        };
    }

    public static DietPlan GetVegetarianDietPlan()
    {
        return new DietPlan
        {
            Type = DietType.Vegetarian,
            Title = "Dieta Vegetariana",
            Description = "Inclui laticínios e ovos, sem carnes",
            DayMeals = new List<DayMeal>
            {
                new DayMeal
                {
                    Day = "SEG",
                    Color = "#8B5CF6",
                    Meals = new Meal
                    {
                        Breakfast = "Ovos mexidos com queijo e pão integral.",
                        MorningSnack = "Iogurte com granola.",
                        Lunch = "Arroz integral, feijão, omelete e salada.",
                        AfternoonSnack = "Frutas com queijo cottage.",
                        Dinner = "Lasanha de berinjela com queijo."
                    }
                },
                new DayMeal
                {
                    Day = "TER",
                    Color = "#EC4899",
                    Meals = new Meal
                    {
                        Breakfast = "Panqueca de aveia com mel e frutas.",
                        MorningSnack = "Queijo branco com tomate.",
                        Lunch = "Quinoa com legumes grelhados e queijo feta.",
                        AfternoonSnack = "Vitamina de banana com aveia.",
                        Dinner = "Sopa de lentilha com ovo pochê."
                    }
                },
                new DayMeal
                {
                    Day = "QUA",
                    Color = "#6366F1",
                    Meals = new Meal
                    {
                        Breakfast = "Mingau de aveia com frutas vermelhas.",
                        MorningSnack = "Mix de castanhas.",
                        Lunch = "Grão-de-bico ao curry com arroz e raita.",
                        AfternoonSnack = "Pão integral com queijo cremoso.",
                        Dinner = "Pizza vegetariana caseira."
                    }
                },
                new DayMeal
                {
                    Day = "QUI",
                    Color = "#3B82F6",
                    Meals = new Meal
                    {
                        Breakfast = "Tapioca com queijo e ovo.",
                        MorningSnack = "Maçã com pasta de amendoim.",
                        Lunch = "Risoto de cogumelos com parmesão.",
                        AfternoonSnack = "Iogurte grego com mel.",
                        Dinner = "Hambúrguer de grão-de-bico com salada."
                    }
                },
                new DayMeal
                {
                    Day = "SEX",
                    Color = "#10B981",
                    Meals = new Meal
                    {
                        Breakfast = "Crepioca com queijo branco.",
                        MorningSnack = "Smoothie de frutas com leite.",
                        Lunch = "Macarrão ao pesto com tomate seco.",
                        AfternoonSnack = "Bolo de cenoura com cobertura.",
                        Dinner = "Quiche de legumes com salada."
                    }
                },
                new DayMeal
                {
                    Day = "SÁB",
                    Color = "#F97316",
                    Meals = new Meal
                    {
                        Breakfast = "Pão francês com manteiga e queijo.",
                        MorningSnack = "Salada de frutas com iogurte.",
                        Lunch = "Feijoada vegetariana com arroz e couve.",
                        AfternoonSnack = "Pudim de leite.",
                        Dinner = "Berinjela à parmegiana."
                    }
                },
                new DayMeal
                {
                    Day = "DOM",
                    Color = "#EF4444",
                    Meals = new Meal
                    {
                        Breakfast = "Açaí com granola e banana.",
                        MorningSnack = "Queijo minas com geleia.",
                        Lunch = "Strogonoff de cogumelos com arroz.",
                        AfternoonSnack = "Torta de limão.",
                        Dinner = "Wrap de queijo com legumes grelhados."
                    }
                }
            }
        };
    }

    public static DietPlan GetDietPlan(DietType dietType)
    {
        return dietType switch
        {
            DietType.Keto => GetKetoDietPlan(),
            DietType.LowCarb => GetLowCarbDietPlan(),
            DietType.Vegan => GetVeganDietPlan(),
            DietType.Celiac => GetCeliacDietPlan(),
            DietType.Vegetarian => GetVegetarianDietPlan(),
            _ => GetKetoDietPlan()
        };
    }
}

