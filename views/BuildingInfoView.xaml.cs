using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using anartsgame.models;

namespace anartsgame.views;

public partial class BuildingInfoView : UserControl
{
    public event Action<Building>? UpgradeClicked;
    public event Action<Building>? DeleteClicked;
    public event Action<Building, ResourceType, int>? InvestmentStarted;
    public event Action<Building, int>? BatchProductionStarted;
    public event Action<Building>? BatchProductionCancelled;

    private Building? _building;
    private Dictionary<ResourceType, int> _resources = new();
    private double _buildCostMultiplier = 1.0;
    private bool _deleteConfirmationState = false;
    private List<ProgressBar> _progressBars = new();

    public BuildingInfoView()
    {
        InitializeComponent();
    }

    public void UpdateBuilding(Building building, Dictionary<ResourceType, int> resources, double buildCostMultiplier)
    {
        _building = building;
        _resources = resources;
        _buildCostMultiplier = buildCostMultiplier;
        _deleteConfirmationState = false;
        RefreshUI();
    }

    public void UpdateProgress()
    {
        if (_building == null) return;

        foreach (var progressBar in _progressBars)
        {
            progressBar.Value = _building.ProductionProgress * 100;
        }

        if (_building.IsBatchProducing)
        {
            BatchRemainingText.Text = $"{services.LocalizationService.Instance["BatchProduction_Remaining"]} {_building.BatchProductionRemaining}";
            BatchProgressBar.Value = _building.ProductionProgress * 100;
        }
    }

    private void RefreshUI()
    {
        if (_building == null) return;

        string description = GetBuildingDescription(_building.Type);
        DescriptionText.Text = description;

        if (_building.Type != BuildingType.Base)
        {
            LevelText.Text = $"{services.LocalizationService.Instance["BuildingPanel_Level"]} {_building.Level}";
            LevelText.Visibility = Visibility.Visible;

            if (_building.Type == BuildingType.Altar || _building.Type == BuildingType.Crystallizer)
            {
                ShowBatchProductionUI();
            }
            else
            {
                BatchProductionActivePanel.Visibility = Visibility.Collapsed;
                BatchProductionSetupPanel.Visibility = Visibility.Collapsed;
            }

            ShowButtonsPanel();
            ShowProductionProgress();

            if (_building.Type == BuildingType.Bank)
            {
                ShowInvestmentPanel();
            }
            else
            {
                InvestmentPanel.Visibility = Visibility.Collapsed;
            }
        }
        else
        {
            LevelText.Visibility = Visibility.Collapsed;
            ButtonsPanel.Visibility = Visibility.Collapsed;
            UpgradeCostText.Visibility = Visibility.Collapsed;
            ProductionLabel.Visibility = Visibility.Collapsed;
            ProductionBarsPanel.Children.Clear();
            InvestmentPanel.Visibility = Visibility.Collapsed;
            BatchProductionActivePanel.Visibility = Visibility.Collapsed;
            BatchProductionSetupPanel.Visibility = Visibility.Collapsed;
        }
    }

    private void ShowBatchProductionUI()
    {
        if (_building == null) return;

        if (_building.IsBatchProducing)
        {
            BatchProductionActivePanel.Visibility = Visibility.Visible;
            BatchProductionSetupPanel.Visibility = Visibility.Collapsed;

            BatchProducingLabel.Text = services.LocalizationService.Instance["BatchProduction_Producing"];
            BatchRemainingText.Text = $"{services.LocalizationService.Instance["BatchProduction_Remaining"]} {_building.BatchProductionRemaining}";
            BatchProgressBar.Value = _building.ProductionProgress * 100;
            BatchCancelButton.Content = services.LocalizationService.Instance["BatchProduction_Cancel"];
            BatchCancelWarning.Text = services.LocalizationService.Instance["BatchProduction_CancelWarning"];
        }
        else
        {
            BatchProductionActivePanel.Visibility = Visibility.Collapsed;
            BatchProductionSetupPanel.Visibility = Visibility.Visible;

            int maxAmount = CalculateMaxBatchProduction();

            BatchAmountLabel.Text = services.LocalizationService.Instance["BatchProduction_Amount"];
            BatchAmountSlider.Minimum = 0;
            BatchAmountSlider.Maximum = maxAmount;
            BatchAmountSlider.Value = Math.Min(1, maxAmount);
            BatchAmountText.Text = $"{(int)BatchAmountSlider.Value} / {maxAmount}";

            BatchInputLabel.Text = services.LocalizationService.Instance["BatchProduction_Input"];
            var costLines = new List<string>();
            foreach (var input in _building.BatchProductionInput)
            {
                costLines.Add($"{GetResourceName(input.Key)}: {input.Value}");
            }
            BatchInputText.Text = string.Join(", ", costLines);

            BatchStartButton.Content = services.LocalizationService.Instance["BatchProduction_Start"];
            BatchStartButton.IsEnabled = maxAmount > 0;
            BatchStartButton.Opacity = maxAmount > 0 ? 1.0 : 0.5;
        }
    }

    private void ShowButtonsPanel()
    {
        if (_building == null) return;

        ButtonsPanel.Visibility = Visibility.Visible;

        if (_building.CanUpgrade())
        {
            UpgradeContainer.Visibility = Visibility.Visible;

            var upgradeCost = _building.GetUpgradeCost();
            var adjustedCost = ApplyCostMultiplier(upgradeCost, _buildCostMultiplier);

            UpgradeButton.Content = services.LocalizationService.Instance["BuildingPanel_Upgrade"];

            bool canAfford = true;
            foreach (var cost in adjustedCost)
            {
                if (!_resources.ContainsKey(cost.Key) || _resources[cost.Key] < cost.Value)
                {
                    canAfford = false;
                    break;
                }
            }

            UpgradeButton.IsEnabled = canAfford;
            UpgradeButton.Opacity = canAfford ? 1.0 : 0.5;

            var costLines = new List<string>();
            foreach (var cost in adjustedCost)
            {
                costLines.Add($"{GetResourceName(cost.Key)}: {cost.Value}");
            }
            UpgradeCostText.Text = string.Join("\n", costLines);
            UpgradeCostText.Visibility = Visibility.Visible;
        }
        else
        {
            UpgradeContainer.Visibility = Visibility.Collapsed;
            UpgradeCostText.Visibility = Visibility.Collapsed;
        }

        DeleteButton.Content = _deleteConfirmationState
            ? services.LocalizationService.Instance["BuildingPanel_Sure"]
            : services.LocalizationService.Instance["BuildingPanel_Delete"];

        DeleteButton.Foreground = _deleteConfirmationState
            ? new SolidColorBrush(Color.FromRgb(255, 100, 100))
            : Brushes.White;
    }

    private void ShowProductionProgress()
    {
        if (_building == null) return;

        ProductionLabel.Text = services.LocalizationService.Instance["BuildingPanel_Production"];
        ProductionLabel.Visibility = Visibility.Visible;
        ProductionBarsPanel.Children.Clear();
        _progressBars.Clear();

        foreach (var output in _building.ProductionOutput)
        {
            var progressContainer = new StackPanel
            {
                Margin = new Thickness(0, 0, 0, 10)
            };

            string resourceName = GetResourceName(output.Key);
            var resourceLabel = new TextBlock
            {
                Text = $"{resourceName} +{output.Value}",
                Foreground = new SolidColorBrush(Color.FromRgb(180, 180, 180)),
                FontSize = 11,
                Margin = new Thickness(0, 0, 0, 5)
            };
            progressContainer.Children.Add(resourceLabel);

            var progressBar = new ProgressBar
            {
                Height = 8,
                Background = new SolidColorBrush(Color.FromRgb(40, 40, 40)),
                Foreground = new SolidColorBrush(Color.FromRgb(100, 150, 255)),
                Value = _building.ProductionProgress * 100,
                Maximum = 100
            };
            progressContainer.Children.Add(progressBar);
            _progressBars.Add(progressBar);

            ProductionBarsPanel.Children.Add(progressContainer);
        }
    }

    private void ShowInvestmentPanel()
    {
        if (_building == null) return;

        InvestmentPanel.Visibility = Visibility.Visible;
        InvestmentLabel.Text = services.LocalizationService.Instance["BuildingPanel_Investment"];

        if (_building.IsInvesting)
        {
            InvestmentActivePanel.Visibility = Visibility.Visible;
            InvestmentCooldownText.Visibility = Visibility.Collapsed;
            InvestmentButtonsPanel.Children.Clear();

            InvestmentActiveText.Text = $"{services.LocalizationService.Instance["BuildingPanel_Invested"]} {_building.InvestmentAmount} {GetResourceName(_building.InvestmentResource!.Value)}";
            InvestmentProgressBar.Value = _building.InvestmentProgress * 100;

            double investmentDuration = _building.Level == 1 ? 200.0 : 180.0;
            double timeRemaining = investmentDuration * (1 - _building.InvestmentProgress);
            InvestmentTimeText.Text = $"{services.LocalizationService.Instance["BuildingPanel_Remaining"]} {timeRemaining:F1}с";
        }
        else if (_building.InvestmentCooldown > 0)
        {
            InvestmentActivePanel.Visibility = Visibility.Collapsed;
            InvestmentCooldownText.Visibility = Visibility.Visible;
            InvestmentButtonsPanel.Children.Clear();

            InvestmentCooldownText.Text = $"{services.LocalizationService.Instance["BuildingPanel_Cooldown"]} {_building.InvestmentCooldown:F1}с";
        }
        else
        {
            InvestmentActivePanel.Visibility = Visibility.Collapsed;
            InvestmentCooldownText.Visibility = Visibility.Collapsed;
            InvestmentButtonsPanel.Children.Clear();

            foreach (ResourceType resourceType in Enum.GetValues(typeof(ResourceType)))
            {
                if (resourceType == ResourceType.Diamonds)
                    continue;

                if (_resources.ContainsKey(resourceType) && _resources[resourceType] > 100)
                {
                    var investButton = new Button
                    {
                        Content = $"{services.LocalizationService.Instance["BuildingPanel_Invest"]} {GetResourceName(resourceType)}",
                        Height = 50,
                        Margin = new Thickness(0, 0, 0, 5),
                        FontSize = 10
                    };

                    var buttonStyle = TryFindResource("GameButtonStyle") as Style;
                    if (buttonStyle != null)
                    {
                        investButton.Style = buttonStyle;
                    }

                    investButton.Click += (s, e) => InvestmentStarted?.Invoke(_building, resourceType, 100);
                    InvestmentButtonsPanel.Children.Add(investButton);
                }
            }
        }
    }

    private int CalculateMaxBatchProduction()
    {
        if (_building == null || _building.BatchProductionInput.Count == 0)
            return 0;

        int maxAmount = int.MaxValue;
        foreach (var input in _building.BatchProductionInput)
        {
            int available = _resources.ContainsKey(input.Key) ? _resources[input.Key] : 0;
            int possibleAmount = available / input.Value;
            maxAmount = Math.Min(maxAmount, possibleAmount);
        }

        return maxAmount;
    }

    private void UpgradeButton_Click(object sender, RoutedEventArgs e)
    {
        if (_building != null)
        {
            UpgradeClicked?.Invoke(_building);
        }
    }

    private void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        if (_building == null) return;

        if (!_deleteConfirmationState)
        {
            _deleteConfirmationState = true;
            DeleteButton.Content = services.LocalizationService.Instance["BuildingPanel_Sure"];
            DeleteButton.Foreground = new SolidColorBrush(Color.FromRgb(255, 100, 100));
        }
        else
        {
            DeleteClicked?.Invoke(_building);
        }
    }

    private void BatchAmountSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        int maxAmount = CalculateMaxBatchProduction();
        BatchAmountText.Text = $"{(int)BatchAmountSlider.Value} / {maxAmount}";
    }

    private void BatchStartButton_Click(object sender, RoutedEventArgs e)
    {
        if (_building != null)
        {
            int amount = (int)BatchAmountSlider.Value;
            if (amount > 0)
            {
                BatchProductionStarted?.Invoke(_building, amount);
            }
        }
    }

    private void BatchCancelButton_Click(object sender, RoutedEventArgs e)
    {
        if (_building != null)
        {
            BatchProductionCancelled?.Invoke(_building);
        }
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

    private string GetBuildingDescription(BuildingType type)
    {
        return type switch
        {
            BuildingType.Base => services.LocalizationService.Instance["BuildingDesc_Base"],
            BuildingType.Factory => services.LocalizationService.Instance["BuildingDesc_Factory"],
            BuildingType.Mine => services.LocalizationService.Instance["BuildingDesc_Mine"],
            BuildingType.MeatFactory => services.LocalizationService.Instance["BuildingDesc_MeatFactory"],
            BuildingType.Sawmill => services.LocalizationService.Instance["BuildingDesc_Sawmill"],
            BuildingType.Bank => services.LocalizationService.Instance["BuildingDesc_Bank"],
            BuildingType.Marketplace => services.LocalizationService.Instance["BuildingDesc_Marketplace"],
            BuildingType.Furnace => services.LocalizationService.Instance["BuildingDesc_Furnace"],
            BuildingType.Altar => services.LocalizationService.Instance["BuildingDesc_Altar"],
            BuildingType.Crystallizer => services.LocalizationService.Instance["BuildingDesc_Crystallizer"],
            _ => services.LocalizationService.Instance["BuildingDesc_Generic"]
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
