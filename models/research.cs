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
                Name = services.LocalizationService.Instance["Research_ImprovedProduction"];
                Description = services.LocalizationService.Instance["ResearchDesc_ImprovedProduction"];
                Duration = 30.0;
                Cost[ResourceType.Metal] = 100;
                Cost[ResourceType.Organic] = 100;
                break;

            case ResearchType.EfficientConstruction:
                Name = services.LocalizationService.Instance["Research_EfficientConstruction"];
                Description = services.LocalizationService.Instance["ResearchDesc_EfficientConstruction"];
                Duration = 45.0;
                Cost[ResourceType.Metal] = 150;
                Cost[ResourceType.Wood] = 100;
                break;

            case ResearchType.FastLearning:
                Name = services.LocalizationService.Instance["Research_FastLearning"];
                Description = services.LocalizationService.Instance["ResearchDesc_FastLearning"];
                Duration = 60.0;
                Cost[ResourceType.Organic] = 200;
                Cost[ResourceType.Meat] = 150;
                break;

            case ResearchType.ExtendedRadius:
                Name = services.LocalizationService.Instance["Research_ExtendedRadius"];
                Description = services.LocalizationService.Instance["ResearchDesc_ExtendedRadius"];
                Duration = 40.0;
                Cost[ResourceType.Metal] = 120;
                Cost[ResourceType.Organic] = 80;
                break;

            case ResearchType.AdvancedMining:
                Name = services.LocalizationService.Instance["Research_AdvancedMining"];
                Description = services.LocalizationService.Instance["ResearchDesc_AdvancedMining"];
                Duration = 50.0;
                Cost[ResourceType.Metal] = 200;
                Cost[ResourceType.Meat] = 100;
                Cost[ResourceType.Wood] = 150;
                break;

            case ResearchType.OrganicBoost:
                Name = services.LocalizationService.Instance["Research_OrganicBoost"];
                Description = services.LocalizationService.Instance["ResearchDesc_OrganicBoost"];
                Duration = 35.0;
                Cost[ResourceType.Organic] = 180;
                Cost[ResourceType.Meat] = 120;
                break;
        }
    }
}
