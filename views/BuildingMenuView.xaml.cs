using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using anartsgame.models;

namespace anartsgame.views;

public partial class BuildingMenuView : UserControl
{
    public event Action<BuildingType>? BuildingSelected;

    private Dictionary<ResourceType, int> _resources = new();
    private double _costMultiplier = 1.0;

    public BuildingMenuView()
    {
        InitializeComponent();
    }

    public void UpdateResources(Dictionary<ResourceType, int> resources, double costMultiplier)
    {
        _resources = resources;
        _costMultiplier = costMultiplier;
        RefreshBuildingList();
    }

    private void RefreshBuildingList()
    {
        BuildingsPanel.Children.Clear();

        AddBuildingButton(BuildingType.Factory);
        AddBuildingButton(BuildingType.Mine);
        AddBuildingButton(BuildingType.MeatFactory);
        AddBuildingButton(BuildingType.Sawmill);
        AddBuildingButton(BuildingType.Bank);
        AddBuildingButton(BuildingType.Marketplace);
        AddBuildingButton(BuildingType.Furnace);
        AddBuildingButton(BuildingType.Altar);
        AddBuildingButton(BuildingType.Crystallizer);
    }

    private void AddBuildingButton(BuildingType buildingType)
    {
        var cost = Building.GetBuildCost(buildingType);
        var adjustedCost = ApplyCostMultiplier(cost, _costMultiplier);

        string buildingName = GetBuildingName(buildingType);
        string costText = FormatCost(adjustedCost);

        var button = new Button
        {
            Content = $"{buildingName}\n{costText}",
            Height = 70,
            Margin = new Thickness(0, 0, 0, 10),
            Padding = new Thickness(20, 0, 0, 0),
            HorizontalContentAlignment = HorizontalAlignment.Left
        };

        var buttonStyle = TryFindResource("GameButtonStyle") as Style;
        if (buttonStyle != null)
        {
            button.Style = buttonStyle;
        }

        button.Click += (s, e) => BuildingSelected?.Invoke(buildingType);
        BuildingsPanel.Children.Add(button);
    }

    private Dictionary<ResourceType, int> ApplyCostMultiplier(Dictionary<ResourceType, int> baseCost, double multiplier)
    {
        var adjusted = new Dictionary<ResourceType, int>();
        foreach (var cost in baseCost)
        {
            adjusted[cost.Key] = (int)(cost.Value * multiplier);
        }
        return adjusted;
    }

    private string FormatCost(Dictionary<ResourceType, int> cost)
    {
        var parts = new List<string>();
        foreach (var item in cost)
        {
            string resourceName = GetResourceName(item.Key);
            parts.Add($"{item.Value} {resourceName}");
        }
        return string.Join(", ", parts);
    }

    private string GetBuildingName(BuildingType type)
    {
        return type switch
        {
            BuildingType.Factory => services.LocalizationService.Instance["Building_Factory"],
            BuildingType.Mine => services.LocalizationService.Instance["Building_Mine"],
            BuildingType.MeatFactory => services.LocalizationService.Instance["Building_MeatFactory"],
            BuildingType.Sawmill => services.LocalizationService.Instance["Building_Sawmill"],
            BuildingType.Bank => services.LocalizationService.Instance["Building_Bank"],
            BuildingType.Marketplace => services.LocalizationService.Instance["Building_Marketplace"],
            BuildingType.Furnace => services.LocalizationService.Instance["Building_Furnace"],
            BuildingType.Altar => services.LocalizationService.Instance["Building_Altar"],
            BuildingType.Crystallizer => services.LocalizationService.Instance["Building_Crystallizer"],
            _ => services.LocalizationService.Instance["Building_Generic"]
        };
    }

    private string GetResourceName(ResourceType resourceType)
    {
        return resourceType switch
        {
            ResourceType.Metal => services.LocalizationService.Instance["Resource_Metal"],
            ResourceType.Organic => services.LocalizationService.Instance["Resource_Organic"],
            ResourceType.Meat => services.LocalizationService.Instance["Resource_Meat"],
            ResourceType.Wood => services.LocalizationService.Instance["Resource_Wood"],
            ResourceType.Coal => services.LocalizationService.Instance["Resource_Coal"],
            ResourceType.Bones => services.LocalizationService.Instance["Resource_Bones"],
            ResourceType.Diamonds => services.LocalizationService.Instance["Resource_Diamonds"],
            _ => services.LocalizationService.Instance["Resource_Generic"]
        };
    }
}
