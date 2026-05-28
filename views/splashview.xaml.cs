using System.Windows.Controls;
using System.Windows.Threading;

namespace anartsgame.views;

public partial class SplashView : UserControl
{
    private readonly string[] _messages =
    {
        "> initializing game engine...",
        "> loading resources...",
        "> preparing world generator...",
        "> ready."
    };

    private int _currentMessageIndex;
    private int _currentCharIndex;
    private DispatcherTimer? _typewriterTimer;

    public event EventHandler? LoadingComplete;

    public SplashView()
    {
        InitializeComponent();
        Loaded += (s, e) => StartTypewriter();
    }

    private void StartTypewriter()
    {
        _typewriterTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(30)
        };
        _typewriterTimer.Tick += TypewriterTick;
        _typewriterTimer.Start();
    }

    private void TypewriterTick(object? sender, EventArgs e)
    {
        if (_currentMessageIndex >= _messages.Length)
        {
            _typewriterTimer?.Stop();
            Task.Delay(500).ContinueWith(_ => Dispatcher.Invoke(() => LoadingComplete?.Invoke(this, EventArgs.Empty)));
            return;
        }

        var currentMessage = _messages[_currentMessageIndex];

        if (_currentCharIndex < currentMessage.Length)
        {
            LoadingText.Text += currentMessage[_currentCharIndex];
            _currentCharIndex++;
        }
        else
        {
            LoadingText.Text += "\n";
            _currentMessageIndex++;
            _currentCharIndex = 0;

            if (_currentMessageIndex < _messages.Length)
            {
                _typewriterTimer!.Interval = TimeSpan.FromMilliseconds(30);
            }
        }
    }
}
