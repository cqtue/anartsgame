using System.IO;
using System.Text.Json;
using System.Windows;

namespace anartsgame.services;

public class SettingsService
{
    private static SettingsService? _instance;
    private static readonly object _lock = new();
    private readonly string _settingsPath;

    public static SettingsService Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    _instance ??= new SettingsService();
                }
            }
            return _instance;
        }
    }

    public double MasterVolume { get; set; } = 100;
    public double SoundVolume { get; set; } = 100;
    public double MusicVolume { get; set; } = 100;
    public bool IsFullscreen { get; set; }
    public bool IsVsyncEnabled { get; set; } = true;

    private SettingsService()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        var gameFolder = Path.Combine(appData, "anarts");
        Directory.CreateDirectory(gameFolder);
        _settingsPath = Path.Combine(gameFolder, "settings.json");
        Load();
    }

    public void Save()
    {
        var settings = new
        {
            MasterVolume,
            SoundVolume,
            MusicVolume,
            IsFullscreen,
            IsVsyncEnabled
        };

        var json = JsonSerializer.Serialize(settings, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(_settingsPath, json);
    }

    public void Load()
    {
        if (!File.Exists(_settingsPath))
            return;

        try
        {
            var json = File.ReadAllText(_settingsPath);
            var settings = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json);

            if (settings == null) return;

            if (settings.TryGetValue("MasterVolume", out var masterVolume))
                MasterVolume = masterVolume.GetDouble();

            if (settings.TryGetValue("SoundVolume", out var soundVolume))
                SoundVolume = soundVolume.GetDouble();

            if (settings.TryGetValue("MusicVolume", out var musicVolume))
                MusicVolume = musicVolume.GetDouble();

            if (settings.TryGetValue("IsFullscreen", out var isFullscreen))
                IsFullscreen = isFullscreen.GetBoolean();

            if (settings.TryGetValue("IsVsyncEnabled", out var isVsyncEnabled))
                IsVsyncEnabled = isVsyncEnabled.GetBoolean();
        }
        catch
        {
        }
    }

    public void ApplyFullscreenMode()
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            var mainWindow = Application.Current.MainWindow;
            if (mainWindow == null) return;

            if (IsFullscreen)
            {
                mainWindow.WindowStyle = WindowStyle.None;
                mainWindow.WindowState = WindowState.Maximized;
                mainWindow.ResizeMode = ResizeMode.NoResize;
            }
            else
            {
                mainWindow.WindowState = WindowState.Normal;
                mainWindow.WindowStyle = WindowStyle.None;
                mainWindow.Width = 1280;
                mainWindow.Height = 720;
                mainWindow.ResizeMode = ResizeMode.NoResize;
                mainWindow.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }
        });
    }
}
