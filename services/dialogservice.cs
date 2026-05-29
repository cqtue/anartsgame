using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Animation;

namespace anartsgame.services;

public class DialogService
{
    private static DialogService? _instance;
    private static readonly object _lock = new();

    public static DialogService Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    _instance ??= new DialogService();
                }
            }
            return _instance;
        }
    }

    private DialogService() { }

    public void ShowConfirmDialog(string message, Action onConfirm, Action onCancel)
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            var mainWindow = Application.Current.MainWindow as MainWindow;
            if (mainWindow == null) return;

            var overlay = new Border
            {
                Background = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromArgb(200, 10, 10, 10)),
                Opacity = 0
            };

            var dialog = new Border
            {
                Background = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(10, 10, 10)),
                BorderBrush = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(0, 255, 0)),
                BorderThickness = new Thickness(2),
                Width = 500,
                Height = 200,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Opacity = 0
            };

            var stackPanel = new StackPanel
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };

            var messageText = new TextBlock
            {
                Text = message,
                FontFamily = new System.Windows.Media.FontFamily(new Uri("pack://application:,,,/styles/Watc2.TTF"), "./#Watc"),
                FontSize = 16,
                Foreground = new System.Windows.Media.SolidColorBrush(
                    System.Windows.Media.Color.FromRgb(0, 255, 0)),
                TextAlignment = TextAlignment.Center,
                Margin = new Thickness(20, 20, 20, 30),
                TextWrapping = TextWrapping.Wrap
            };

            var buttonPanel = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center
            };

            var confirmButton = new Button
            {
                Content = LocalizationService.Instance["Dialog_Yes"],
                Width = 120,
                Margin = new Thickness(0, 0, 20, 0)
            };
            confirmButton.SetResourceReference(Button.StyleProperty, "MenuButtonStyle");

            var cancelButton = new Button
            {
                Content = LocalizationService.Instance["Dialog_No"],
                Width = 120
            };
            cancelButton.SetResourceReference(Button.StyleProperty, "MenuButtonStyle");

            confirmButton.Click += async (s, e) =>
            {
                await FadeOutDialog(overlay, dialog);
                mainWindow.DialogOverlay.Children.Clear();
                onConfirm();
            };

            cancelButton.Click += async (s, e) =>
            {
                await FadeOutDialog(overlay, dialog);
                mainWindow.DialogOverlay.Children.Clear();
                onCancel();
            };

            buttonPanel.Children.Add(confirmButton);
            buttonPanel.Children.Add(cancelButton);

            stackPanel.Children.Add(messageText);
            stackPanel.Children.Add(buttonPanel);

            dialog.Child = stackPanel;

            mainWindow.DialogOverlay.Children.Clear();
            mainWindow.DialogOverlay.Children.Add(overlay);
            mainWindow.DialogOverlay.Children.Add(dialog);

            FadeInDialog(overlay, dialog);
        });
    }

    private void FadeInDialog(Border overlay, Border dialog)
    {
        var overlayAnimation = new DoubleAnimation
        {
            From = 0,
            To = 1,
            Duration = TimeSpan.FromSeconds(0.2),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
        };

        var dialogAnimation = new DoubleAnimation
        {
            From = 0,
            To = 1,
            Duration = TimeSpan.FromSeconds(0.3),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
        };

        overlay.BeginAnimation(UIElement.OpacityProperty, overlayAnimation);
        dialog.BeginAnimation(UIElement.OpacityProperty, dialogAnimation);
    }

    private Task FadeOutDialog(Border overlay, Border dialog)
    {
        var tcs = new TaskCompletionSource();

        var overlayAnimation = new DoubleAnimation
        {
            From = 1,
            To = 0,
            Duration = TimeSpan.FromSeconds(0.2),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
        };

        var dialogAnimation = new DoubleAnimation
        {
            From = 1,
            To = 0,
            Duration = TimeSpan.FromSeconds(0.2),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseInOut }
        };

        dialogAnimation.Completed += (s, e) => tcs.SetResult();

        overlay.BeginAnimation(UIElement.OpacityProperty, overlayAnimation);
        dialog.BeginAnimation(UIElement.OpacityProperty, dialogAnimation);

        return tcs.Task;
    }
}
