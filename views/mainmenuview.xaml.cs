using System.Windows.Controls;

namespace anartsgame.views;

public partial class MainMenuView : UserControl
{
    public MainMenuView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, System.Windows.RoutedEventArgs e)
    {
        services.MusicService.Instance.PlayMainMenu();
    }
}
