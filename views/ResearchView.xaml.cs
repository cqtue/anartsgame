using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using anartsgame.models;

namespace anartsgame.views;

public partial class ResearchView : UserControl
{
    public event Action<Research>? ResearchStarted;

    private List<Research> _availableResearch = new();
    private Research? _currentResearch = null;
    private Dictionary<ResourceType, int> _resources = new();

    public ResearchView()
    {
        InitializeComponent();
        AvailableLabel.Text = services.LocalizationService.Instance["Research_Available"];
    }

    public void UpdateResearch(List<Research> availableResearch, Research? currentResearch, Dictionary<ResourceType, int> resources)
    {
        _availableResearch = availableResearch;
        _currentResearch = currentResearch;
        _resources = resources;
        RefreshResearchPanel();
    }

    private void RefreshResearchPanel()
    {
        if (_currentResearch != null && _currentResearch.IsResearching)
        {
            CurrentResearchPanel.Visibility = Visibility.Visible;
            CurrentResearchName.Text = _currentResearch.Name;
            CurrentResearchProgress.Value = _currentResearch.Progress * 100;
            CurrentResearchTime.Text = $"{services.LocalizationService.Instance["Research_Remaining"]} {(_currentResearch.Duration * (1 - _currentResearch.Progress)):F1}с";
        }
        else
        {
            CurrentResearchPanel.Visibility = Visibility.Collapsed;
        }

        ResearchListPanel.Children.Clear();

        foreach (var research in _availableResearch)
        {
            if (research.IsCompleted) continue;

            var researchPanel = new StackPanel
            {
                Margin = new Thickness(0, 0, 0, 15),
                Background = new SolidColorBrush(Color.FromArgb(30, 255, 255, 255))
            };

            var researchName = new TextBlock
            {
                Text = research.Name,
                Foreground = new SolidColorBrush(Color.FromRgb(200, 200, 200)),
                FontSize = 13,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness(5, 5, 5, 3)
            };
            researchPanel.Children.Add(researchName);

            var researchDesc = new TextBlock
            {
                Text = research.Description,
                Foreground = new SolidColorBrush(Color.FromRgb(180, 180, 180)),
                FontSize = 11,
                TextWrapping = TextWrapping.Wrap,
                Margin = new Thickness(5, 0, 5, 5)
            };
            researchPanel.Children.Add(researchDesc);

            var costText = "";
            foreach (var cost in research.Cost)
            {
                if (costText.Length > 0) costText += ", ";
                costText += $"{GetResourceName(cost.Key)}: {cost.Value}";
            }

            var costLabel = new TextBlock
            {
                Text = $"{services.LocalizationService.Instance["Research_Cost"]} {costText}",
                Foreground = new SolidColorBrush(Color.FromRgb(150, 150, 150)),
                FontSize = 10,
                Margin = new Thickness(5, 0, 5, 5)
            };
            researchPanel.Children.Add(costLabel);

            var researchButton = new Button
            {
                Content = $"{services.LocalizationService.Instance["Research_Button"]} ({research.Duration}с)",
                Height = 35,
                Margin = new Thickness(5, 0, 5, 5),
                FontSize = 11
            };

            var buttonStyle = TryFindResource("GameButtonStyle") as Style;
            if (buttonStyle != null)
            {
                researchButton.Style = buttonStyle;
            }

            bool canAfford = true;
            foreach (var cost in research.Cost)
            {
                if (!_resources.ContainsKey(cost.Key) || _resources[cost.Key] < cost.Value)
                {
                    canAfford = false;
                    break;
                }
            }

            if (!canAfford || (_currentResearch != null && _currentResearch.IsResearching))
            {
                researchButton.Opacity = 0.5;
                researchButton.IsEnabled = false;
            }

            researchButton.Click += (s, e) => ResearchStarted?.Invoke(research);
            researchPanel.Children.Add(researchButton);

            ResearchListPanel.Children.Add(researchPanel);
        }
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
