using System;
using System.IO;
using System.Text.Json;
using anartsgame.models;

namespace anartsgame.services;

public class SaveLoadService
{
    private static SaveLoadService? _instance;
    private static readonly object _lock = new();

    public static SaveLoadService Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    _instance ??= new SaveLoadService();
                }
            }
            return _instance;
        }
    }

    private readonly string _saveDirectory;
    private readonly string _saveFilePath;

    private SaveLoadService()
    {
        _saveDirectory = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "anartsgame"
        );
        _saveFilePath = Path.Combine(_saveDirectory, "savegame.json");

        if (!Directory.Exists(_saveDirectory))
        {
            Directory.CreateDirectory(_saveDirectory);
        }
    }

    public bool SaveExists()
    {
        return File.Exists(_saveFilePath);
    }

    public void SaveGame(GameState gameState)
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true
            };

            string json = JsonSerializer.Serialize(gameState, options);
            File.WriteAllText(_saveFilePath, json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error saving game: {ex.Message}");
            throw;
        }
    }

    public GameState? LoadGame()
    {
        try
        {
            if (!File.Exists(_saveFilePath))
            {
                return null;
            }

            string json = File.ReadAllText(_saveFilePath);
            return JsonSerializer.Deserialize<GameState>(json);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading game: {ex.Message}");
            return null;
        }
    }

    public void DeleteSave()
    {
        try
        {
            if (File.Exists(_saveFilePath))
            {
                File.Delete(_saveFilePath);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error deleting save: {ex.Message}");
        }
    }
}
