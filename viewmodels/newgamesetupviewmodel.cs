using System.Windows.Input;
using anartsgame.services;

namespace anartsgame.viewmodels;

public class NewGameSetupViewModel : BaseViewModel
{
    public ICommand StartGameCommand { get; }

    public NewGameSetupViewModel()
    {
        StartGameCommand = new RelayCommand(_ => StartGame());
    }

    private void StartGame()
    {
        NavigationService.Instance.NavigateToGame();
    }
}
