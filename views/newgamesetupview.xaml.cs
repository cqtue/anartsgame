using System.Windows.Controls;
using System.Windows.Input;

namespace anartsgame.views;

public partial class NewGameSetupView : UserControl
{
    public NewGameSetupView()
    {
        InitializeComponent();
        KeyDown += OnKeyDown;
        Focusable = true;
        Loaded += (s, e) => Focus();
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            services.NavigationService.Instance.NavigateToMainMenu();
            e.Handled = true;
        }
    }
}
