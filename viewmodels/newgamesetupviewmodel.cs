using System.Windows.Input;
using anartsgame.services;

namespace anartsgame.viewmodels;

public class NewGameSetupViewModel : BaseViewModel
{
    private int _metalAmount = 20;
    private int _organicAmount = 10;
    private int _meatAmount = 10;
    private int _woodAmount = 20;
    private bool _disableSaving = false;
    private bool _enableConsole = false;

    public int MetalAmount
    {
        get => _metalAmount;
        set => SetProperty(ref _metalAmount, value);
    }

    public int OrganicAmount
    {
        get => _organicAmount;
        set => SetProperty(ref _organicAmount, value);
    }

    public int MeatAmount
    {
        get => _meatAmount;
        set => SetProperty(ref _meatAmount, value);
    }

    public int WoodAmount
    {
        get => _woodAmount;
        set => SetProperty(ref _woodAmount, value);
    }

    public bool DisableSaving
    {
        get => _disableSaving;
        set => SetProperty(ref _disableSaving, value);
    }

    public bool EnableConsole
    {
        get => _enableConsole;
        set => SetProperty(ref _enableConsole, value);
    }

    public ICommand StartGameCommand { get; }

    public NewGameSetupViewModel()
    {
        StartGameCommand = new RelayCommand(_ => StartGame());
    }

    private void StartGame()
    {
        var settings = new GameSettings
        {
            MetalAmount = _metalAmount,
            OrganicAmount = _organicAmount,
            MeatAmount = _meatAmount,
            WoodAmount = _woodAmount,
            DisableSaving = _disableSaving,
            EnableConsole = _enableConsole
        };

        NavigationService.Instance.NavigateToGame(settings);
    }
}

public class GameSettings
{
    public int MetalAmount { get; set; }
    public int OrganicAmount { get; set; }
    public int MeatAmount { get; set; }
    public int WoodAmount { get; set; }
    public bool DisableSaving { get; set; }
    public bool EnableConsole { get; set; }
}
