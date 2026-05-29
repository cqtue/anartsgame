using System.Collections.Generic;

namespace anartsgame.models;

public enum ResearchType
{
    ImprovedProduction,
    EfficientConstruction,
    FastLearning,
    ExtendedRadius,
    AdvancedMining,
    OrganicBoost
}

public class Research
{
    public ResearchType Type { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public Dictionary<ResourceType, int> Cost { get; set; }
    public double Duration { get; set; }
    public double Progress { get; set; }
    public bool IsCompleted { get; set; }
    public bool IsResearching { get; set; }

    public Research(ResearchType type)
    {
        Type = type;
        Progress = 0;
        IsCompleted = false;
        IsResearching = false;
        Cost = new Dictionary<ResourceType, int>();

        switch (type)
        {
            case ResearchType.ImprovedProduction:
                Name = "Покращене виробництво";
                Description = "Збільшує швидкість виробництва всіх будівель на 15%";
                Duration = 30.0;
                Cost[ResourceType.Metal] = 100;
                Cost[ResourceType.Organic] = 100;
                break;

            case ResearchType.EfficientConstruction:
                Name = "Ефективне будівництво";
                Description = "Зменшує вартість будівництва на 20%";
                Duration = 45.0;
                Cost[ResourceType.Metal] = 150;
                Cost[ResourceType.Wood] = 100;
                break;

            case ResearchType.FastLearning:
                Name = "Швидке навчання";
                Description = "Зменшує час досліджень на 25%";
                Duration = 60.0;
                Cost[ResourceType.Organic] = 200;
                Cost[ResourceType.Meat] = 150;
                break;

            case ResearchType.ExtendedRadius:
                Name = "Розширений радіус";
                Description = "Збільшує радіус будівництва на 30%";
                Duration = 40.0;
                Cost[ResourceType.Metal] = 120;
                Cost[ResourceType.Organic] = 80;
                break;

            case ResearchType.AdvancedMining:
                Name = "Покращена видобування";
                Description = "Шахти виробляють на 50% більше металу";
                Duration = 50.0;
                Cost[ResourceType.Metal] = 200;
                Cost[ResourceType.Meat] = 100;
                Cost[ResourceType.Wood] = 150;
                break;

            case ResearchType.OrganicBoost:
                Name = "Органічний бум";
                Description = "Фабрики виробляють на 40% більше органіки";
                Duration = 35.0;
                Cost[ResourceType.Organic] = 180;
                Cost[ResourceType.Meat] = 120;
                break;
        }
    }
}
