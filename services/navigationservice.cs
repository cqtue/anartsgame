using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;
using anartsgame.views;

namespace anartsgame.services;

public class NavigationService
{
    private static NavigationService? _instance;
    private static readonly object _lock = new();
    private bool _returnToGameWithPause = false;

    public static NavigationService Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    _instance ??= new NavigationService();
                }
            }
            return _instance;
        }
    }

    private NavigationService() { }

    public async void NavigateTo(UserControl view)
    {
        await Application.Current.Dispatcher.InvokeAsync(async () =>
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow == null) return;

            var currentView = mainWindow.ContentGrid.Children.Count > 0
                ? mainWindow.ContentGrid.Children[0] as UserControl
                : null;

            if (currentView != null)
            {
                var fadeOut = new DoubleAnimation
                {
                    From = 1,
                    To = 0,
                    Duration = TimeSpan.FromSeconds(0.3),
                    EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
                };

                var tcs = new TaskCompletionSource();
                fadeOut.Completed += (s, e) => tcs.SetResult();
                currentView.BeginAnimation(UIElement.OpacityProperty, fadeOut);
                await tcs.Task;
            }

            mainWindow.ContentGrid.Children.Clear();
            view.Opacity = 0;
            mainWindow.ContentGrid.Children.Add(view);

            var fadeIn = new DoubleAnimation
            {
                From = 0,
                To = 1,
                Duration = TimeSpan.FromSeconds(0.3),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
            };
            view.BeginAnimation(UIElement.OpacityProperty, fadeIn);
        });
    }

    public void NavigateToMainMenu() => NavigateTo(new MainMenuView());
    public void NavigateToSettings() => NavigateTo(new SettingsView());
    public void NavigateToNewGameSetup() => NavigateTo(new NewGameSetupView());
    public void NavigateToGame() => NavigateTo(new GameView());

    public void SetReturnToGameWithPause(bool value)
    {
        _returnToGameWithPause = value;
    }

    public bool ShouldReturnToGameWithPause()
    {
        bool value = _returnToGameWithPause;
        _returnToGameWithPause = false; // Reset after reading
        return value;
    }

    public bool IsGameActive()
    {
        return _returnToGameWithPause;
    }
}
