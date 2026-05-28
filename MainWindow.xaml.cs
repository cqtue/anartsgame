using System.Windows;
using System.Windows.Media.Animation;
using anartsgame.views;
using anartsgame.services;

namespace anartsgame;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Opacity = 0;
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object sender, RoutedEventArgs e)
    {
        SettingsService.Instance.ApplyFullscreenMode();

        await FadeIn();
        await ShowSplashScreen();
        await ShowMainMenu();
    }

    private Task FadeIn()
    {
        var tcs = new TaskCompletionSource();
        var animation = new DoubleAnimation
        {
            From = 0,
            To = 1,
            Duration = TimeSpan.FromSeconds(1.5),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
        };
        animation.Completed += (s, e) => tcs.SetResult();
        BeginAnimation(OpacityProperty, animation);
        return tcs.Task;
    }

    private Task ShowSplashScreen()
    {
        var tcs = new TaskCompletionSource();
        var splashView = new SplashView();
        splashView.LoadingComplete += (s, e) => tcs.SetResult();
        ContentGrid.Children.Clear();
        ContentGrid.Children.Add(splashView);
        return tcs.Task;
    }

    private async Task ShowMainMenu()
    {
        var mainMenuView = new MainMenuView { Opacity = 0 };
        ContentGrid.Children.Clear();
        ContentGrid.Children.Add(mainMenuView);

        await Task.Delay(300);

        var animation = new DoubleAnimation
        {
            From = 0,
            To = 1,
            Duration = TimeSpan.FromSeconds(1),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
        };
        mainMenuView.BeginAnimation(OpacityProperty, animation);
    }
}
