using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using anartsgame.models;

namespace anartsgame.views;

public partial class TradeView : UserControl
{
    public event Action<ResourceType, int, ResourceType>? TradeExecuted;

    private Dictionary<ResourceType, int> _resources = new();
    private bool _hasMarketplace = false;
    private int _tradeStage = 1;
    private ResourceType? _tradeFromResource = null;
    private int _tradeFromAmount = 0;
    private ResourceType? _tradeToResource = null;

    public TradeView()
    {
        InitializeComponent();
        InitializeLocalizedText();
    }

    private void InitializeLocalizedText()
    {
        var buttonStyle = TryFindResource("GameButtonStyle") as Style;
        if (buttonStyle != null)
        {
            Stage2BackButton.Style = buttonStyle;
            Stage3BackButton.Style = buttonStyle;
            Stage2NextButton.Style = buttonStyle;
            Stage3ConfirmButton.Style = buttonStyle;
        }
    }

    public void UpdateTrade(Dictionary<ResourceType, int> resources, bool hasMarketplace)
    {
        _resources = resources;
        _hasMarketplace = hasMarketplace;
        _tradeStage = 1;
        _tradeFromResource = null;
        _tradeFromAmount = 0;
        _tradeToResource = null;
        RefreshTradePanel();
    }

    private void RefreshTradePanel()
    {
        if (!_hasMarketplace)
        {
            NoMarketplaceMessage.Text = services.LocalizationService.Instance["Trade_NeedMarket"];
            NoMarketplaceMessage.Visibility = Visibility.Visible;
            TradeContentPanel.Visibility = Visibility.Collapsed;
            return;
        }

        NoMarketplaceMessage.Visibility = Visibility.Collapsed;
        TradeContentPanel.Visibility = Visibility.Visible;

        Stage1Panel.Visibility = _tradeStage == 1 ? Visibility.Visible : Visibility.Collapsed;
        Stage2Panel.Visibility = _tradeStage == 2 ? Visibility.Visible : Visibility.Collapsed;
        Stage3Panel.Visibility = _tradeStage == 3 ? Visibility.Visible : Visibility.Collapsed;

        switch (_tradeStage)
        {
            case 1:
                ShowStage1();
                break;
            case 2:
                ShowStage2();
                break;
            case 3:
                ShowStage3();
                break;
        }
    }

    private void ShowStage1()
    {
        Stage1Info.Text = services.LocalizationService.Instance["Trade_Step1"];
        Stage1Rate.Text = services.LocalizationService.Instance["Trade_Rate"];
        Stage1Resources.Children.Clear();

        foreach (ResourceType resource in Enum.GetValues(typeof(ResourceType)))
        {
            if (resource == ResourceType.Diamonds)
                continue;

            if (!_resources.ContainsKey(resource) || _resources[resource] <= 100)
                continue;

            var resourceButton = new Button
            {
                Content = $"{GetResourceName(resource)}\n({services.LocalizationService.Instance["Trade_Has"]} {_resources[resource]})",
                Height = 60,
                Margin = new Thickness(0, 0, 0, 10),
                Style = (Style)FindResource("GameButtonStyle"),
                FontSize = 12
            };
            resourceButton.Click += (s, e) =>
            {
                _tradeFromResource = resource;
                _tradeFromAmount = 100;
                _tradeStage = 2;
                RefreshTradePanel();
            };
            Stage1Resources.Children.Add(resourceButton);
        }
    }

    private void ShowStage2()
    {
        if (!_tradeFromResource.HasValue) return;

        Stage2BackButton.Content = services.LocalizationService.Instance["Trade_Back"];
        Stage2Info.Text = $"{services.LocalizationService.Instance["Trade_Step2"]} {GetResourceName(_tradeFromResource.Value)}";
        Stage2Available.Text = $"{services.LocalizationService.Instance["Trade_Available"]} {_resources[_tradeFromResource.Value]}";
        Stage2Amount.Text = $"{services.LocalizationService.Instance["Trade_Amount"]} {_tradeFromAmount}";

        Stage2Slider.Minimum = 1;
        Stage2Slider.Maximum = _resources[_tradeFromResource.Value];
        Stage2Slider.Value = _tradeFromAmount;

        Stage2NextButton.Content = services.LocalizationService.Instance["Trade_Next"];
    }

    private void ShowStage3()
    {
        if (!_tradeFromResource.HasValue) return;

        Stage3BackButton.Content = services.LocalizationService.Instance["Trade_Back"];
        Stage3Info.Text = services.LocalizationService.Instance["Trade_Step3"];
        Stage3Giving.Text = $"{services.LocalizationService.Instance["Trade_Giving"]} {_tradeFromAmount} {GetResourceName(_tradeFromResource.Value)}";

        if (_tradeToResource.HasValue)
        {
            int receiveAmount = (int)(_tradeFromAmount * 0.6);

            Stage3Calculation.Visibility = Visibility.Visible;
            Stage3Receive.Text = $"{services.LocalizationService.Instance["Trade_Receive"]} {receiveAmount} {GetResourceName(_tradeToResource.Value)}";
            Stage3Rate.Text = $"{services.LocalizationService.Instance["Trade_RateDisplay"]} {_tradeFromAmount} → {receiveAmount} ({(int)(0.6 * 100)}%)";

            Stage3ConfirmButton.Content = services.LocalizationService.Instance["Trade_Confirm"];
            Stage3ConfirmButton.Visibility = Visibility.Visible;
            Stage3Resources.Visibility = Visibility.Collapsed;
        }
        else
        {
            Stage3Calculation.Visibility = Visibility.Collapsed;
            Stage3ConfirmButton.Visibility = Visibility.Collapsed;
            Stage3Resources.Visibility = Visibility.Visible;
            Stage3Resources.Children.Clear();

            foreach (ResourceType resource in Enum.GetValues(typeof(ResourceType)))
            {
                if (resource == ResourceType.Diamonds)
                    continue;

                if (resource == _tradeFromResource.Value)
                    continue;

                int receiveAmount = (int)(_tradeFromAmount * 0.6);

                var resourceButton = new Button
                {
                    Content = $"{GetResourceName(resource)}\n({services.LocalizationService.Instance["Trade_WillReceive"]} {receiveAmount})",
                    Height = 60,
                    Margin = new Thickness(0, 0, 0, 10),
                    Style = (Style)FindResource("GameButtonStyle"),
                    FontSize = 12
                };
                resourceButton.Click += (s, e) =>
                {
                    _tradeToResource = resource;
                    RefreshTradePanel();
                };
                Stage3Resources.Children.Add(resourceButton);
            }
        }
    }

    private void Stage2BackButton_Click(object sender, RoutedEventArgs e)
    {
        _tradeStage = 1;
        _tradeFromResource = null;
        _tradeFromAmount = 0;
        RefreshTradePanel();
    }

    private void Stage2Slider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        _tradeFromAmount = (int)Stage2Slider.Value;
        Stage2Amount.Text = $"{services.LocalizationService.Instance["Trade_Amount"]} {_tradeFromAmount}";
    }

    private void Stage2NextButton_Click(object sender, RoutedEventArgs e)
    {
        _tradeStage = 3;
        RefreshTradePanel();
    }

    private void Stage3BackButton_Click(object sender, RoutedEventArgs e)
    {
        _tradeStage = 2;
        _tradeToResource = null;
        RefreshTradePanel();
    }

    private void Stage3ConfirmButton_Click(object sender, RoutedEventArgs e)
    {
        if (_tradeFromResource.HasValue && _tradeToResource.HasValue)
        {
            TradeExecuted?.Invoke(_tradeFromResource.Value, _tradeFromAmount, _tradeToResource.Value);
            _tradeStage = 1;
            _tradeFromResource = null;
            _tradeFromAmount = 0;
            _tradeToResource = null;
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
