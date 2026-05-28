using System.Windows;
using System.Windows.Input;

namespace anartsgame.viewmodels;

public class MainMenuViewModel : BaseViewModel
{
    private bool _hasSaveGame;

    public bool HasSaveGame
    {
        get => _hasSaveGame;
        set => SetProperty(ref _hasSaveGame, value);
    }

    public ICommand NewGameCommand { get; }
    public ICommand ContinueCommand { get; }
    public ICommand SettingsCommand { get; }
    public ICommand ExitCommand { get; }

    public MainMenuViewModel()
    {
        HasSaveGame = false;

        NewGameCommand = new RelayCommand(_ => StartNewGame());
        ContinueCommand = new RelayCommand(_ => ContinueGame(), _ => HasSaveGame);
        SettingsCommand = new RelayCommand(_ => OpenSettings());
        ExitCommand = new RelayCommand(_ => Application.Current.Shutdown());
    }

    private void StartNewGame()
    {
        services.NavigationService.Instance.NavigateToNewGameSetup();
    }

    private void ContinueGame()
    {
    }

    private void OpenSettings()
    {
        services.NavigationService.Instance.NavigateToSettings();
    }
}
