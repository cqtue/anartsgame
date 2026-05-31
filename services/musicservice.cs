using System;
using System.IO;
using System.Windows.Media;
using System.Diagnostics;

namespace anartsgame.services;

public class MusicService
{
    private static MusicService? _instance;
    private static readonly object _lock = new();
    private MediaPlayer? _mediaPlayer;
    private string? _currentTrack;
    private bool _isLooping = true;

    public static MusicService Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    _instance ??= new MusicService();
                }
            }
            return _instance;
        }
    }

    private MusicService()
    {
        _mediaPlayer = new MediaPlayer();
        _mediaPlayer.MediaEnded += OnMediaEnded;
        _mediaPlayer.MediaFailed += OnMediaFailed;
        UpdateVolume();
    }

    private void OnMediaEnded(object? sender, EventArgs e)
    {
        if (_isLooping && _currentTrack != null)
        {
            _mediaPlayer?.Position = TimeSpan.Zero;
            _mediaPlayer?.Play();
        }
    }

    private void OnMediaFailed(object? sender, ExceptionEventArgs e)
    {
        Debug.WriteLine($"Music playback failed: {e.ErrorException?.Message}");
    }

    public void PlayMainMenu()
    {
        PlayTrack("mainmenu");
    }

    public void PlayGameTheme()
    {
        PlayTrack("gametheme");
    }

    private void PlayTrack(string trackName)
    {
        if (_currentTrack == trackName && _mediaPlayer != null)
        {
            return;
        }

        try
        {
            _currentTrack = trackName;

            if (_mediaPlayer == null)
            {
                _mediaPlayer = new MediaPlayer();
                _mediaPlayer.MediaEnded += OnMediaEnded;
                _mediaPlayer.MediaFailed += OnMediaFailed;
            }

            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var musicDir = Path.Combine(baseDir, "music");

            // Try different file formats
            string[] extensions = { ".mp3", ".ogg", ".wav" };
            string? foundPath = null;

            foreach (var ext in extensions)
            {
                var testPath = Path.Combine(musicDir, trackName + ext);
                if (File.Exists(testPath))
                {
                    foundPath = testPath;
                    Debug.WriteLine($"Found music file: {foundPath}");
                    break;
                }
            }

            if (foundPath != null)
            {
                _mediaPlayer.Open(new Uri(foundPath, UriKind.Absolute));
                UpdateVolume();
                _mediaPlayer.Play();
                Debug.WriteLine($"Playing: {foundPath}");
                Debug.WriteLine($"Volume: {_mediaPlayer.Volume}");
            }
            else
            {
                Debug.WriteLine($"Music file not found: {trackName} in {musicDir}");
                Debug.WriteLine($"Tried extensions: {string.Join(", ", extensions)}");

                // Temporary debug message
                System.Windows.MessageBox.Show($"Music file not found: {trackName}\nSearched in: {musicDir}\nTried: {string.Join(", ", extensions)}", "Music Debug - File Not Found");
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error playing music: {ex.Message}");
            System.Windows.MessageBox.Show($"Error playing music: {ex.Message}", "Music Debug - Error");
        }
    }

    public void Stop()
    {
        _mediaPlayer?.Stop();
        _currentTrack = null;
    }

    public void Pause()
    {
        _mediaPlayer?.Pause();
    }

    public void Resume()
    {
        _mediaPlayer?.Play();
    }

    public void UpdateVolume()
    {
        if (_mediaPlayer == null) return;

        var settings = SettingsService.Instance;
        double masterVolume = settings.MasterVolume / 100.0;
        double musicVolume = settings.MusicVolume / 100.0;
        double combinedVolume = masterVolume * musicVolume;

        _mediaPlayer.Volume = Math.Clamp(combinedVolume, 0.0, 1.0);
        Debug.WriteLine($"Music volume updated: {_mediaPlayer.Volume:F2} (Master: {masterVolume:F2}, Music: {musicVolume:F2})");
    }

    public void SetLooping(bool loop)
    {
        _isLooping = loop;
    }
}
