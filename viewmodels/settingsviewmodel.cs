using System.Windows.Input;
using anartsgame.services;

namespace anartsgame.viewmodels;

public class SettingsViewModel : BaseViewModel
{
    private readonly SettingsService _settingsService;
    private double _masterVolume;
    private double _soundVolume;
    private double _musicVolume;
    private bool _isFullscreen;
    private bool _isVsyncEnabled;
    private bool _hasUnsavedChanges;
    private int _selectedLanguageIndex;
    private bool _isGameActive;

    private double _originalMasterVolume;
    private double _originalSoundVolume;
    private double _originalMusicVolume;
    private bool _originalIsFullscreen;
    private bool _originalIsVsyncEnabled;
    private int _originalSelectedLanguageIndex;

    public double MasterVolume
    {
        get => _masterVolume;
        set
        {
            if (SetProperty(ref _masterVolume, value))
                CheckForChanges();
        }
    }

    public double SoundVolume
    {
        get => _soundVolume;
        set
        {
            if (SetProperty(ref _soundVolume, value))
                CheckForChanges();
        }
    }

    public double MusicVolume
    {
        get => _musicVolume;
        set
        {
            if (SetProperty(ref _musicVolume, value))
                CheckForChanges();
        }
    }

    public bool IsFullscreen
    {
        get => _isFullscreen;
        set
        {
            if (SetProperty(ref _isFullscreen, value))
                CheckForChanges();
        }
    }

    public bool IsVsyncEnabled
    {
        get => _isVsyncEnabled;
        set
        {
            if (SetProperty(ref _isVsyncEnabled, value))
                CheckForChanges();
        }
    }

    public bool HasUnsavedChanges
    {
        get => _hasUnsavedChanges;
        set => SetProperty(ref _hasUnsavedChanges, value);
    }

    public int SelectedLanguageIndex
    {
        get => _selectedLanguageIndex;
        set
        {
            if (SetProperty(ref _selectedLanguageIndex, value))
                CheckForChanges();
        }
    }

    public List<string> AvailableLanguages { get; } = new() { "Українська", "English" };

    public bool IsGameActive
    {
        get => _isGameActive;
        set => SetProperty(ref _isGameActive, value);
    }

    public ICommand SaveCommand { get; }
    public ICommand BackCommand { get; }

    public SettingsViewModel()
    {
        _settingsService = SettingsService.Instance;

        _masterVolume = _originalMasterVolume = _settingsService.MasterVolume;
        _soundVolume = _originalSoundVolume = _settingsService.SoundVolume;
        _musicVolume = _originalMusicVolume = _settingsService.MusicVolume;
        _isFullscreen = _originalIsFullscreen = _settingsService.IsFullscreen;
        _isVsyncEnabled = _originalIsVsyncEnabled = _settingsService.IsVsyncEnabled;
        _selectedLanguageIndex = _originalSelectedLanguageIndex = AvailableLanguages.IndexOf(_settingsService.Language);
        _isGameActive = NavigationService.Instance.IsGameActive();

        SaveCommand = new RelayCommand(_ => SaveSettings());
        BackCommand = new RelayCommand(_ => NavigateBack());
    }

    private void CheckForChanges()
    {
        HasUnsavedChanges = _masterVolume != _originalMasterVolume ||
                           _soundVolume != _originalSoundVolume ||
                           _musicVolume != _originalMusicVolume ||
                           _isFullscreen != _originalIsFullscreen ||
                           _isVsyncEnabled != _originalIsVsyncEnabled ||
                           _selectedLanguageIndex != _originalSelectedLanguageIndex;
    }

    private void SaveSettings()
    {
        _settingsService.MasterVolume = _masterVolume;
        _settingsService.SoundVolume = _soundVolume;
        _settingsService.MusicVolume = _musicVolume;
        _settingsService.IsFullscreen = _isFullscreen;
        _settingsService.IsVsyncEnabled = _isVsyncEnabled;
        _settingsService.Language = AvailableLanguages[_selectedLanguageIndex];
        _settingsService.Save();

        if (_isFullscreen != _originalIsFullscreen)
            _settingsService.ApplyFullscreenMode();

        if (_selectedLanguageIndex != _originalSelectedLanguageIndex)
            LocalizationService.Instance.SetLanguage(AvailableLanguages[_selectedLanguageIndex]);

        MusicService.Instance.UpdateVolume();

        _originalMasterVolume = _masterVolume;
        _originalSoundVolume = _soundVolume;
        _originalMusicVolume = _musicVolume;
        _originalIsFullscreen = _isFullscreen;
        _originalIsVsyncEnabled = _isVsyncEnabled;
        _originalSelectedLanguageIndex = _selectedLanguageIndex;

        HasUnsavedChanges = false;
    }

    private void NavigateBack()
    {
        if (HasUnsavedChanges)
        {
            DialogService.Instance.ShowConfirmDialog(
                LocalizationService.Instance["Settings_UnsavedChanges"],
                () => PerformNavigation(),
                () => { }
            );
        }
        else
        {
            PerformNavigation();
        }
    }

    private void PerformNavigation()
    {
        if (NavigationService.Instance.ShouldReturnToGameWithPause())
        {
            var gameState = SaveLoadService.Instance.LoadGame();
            if (gameState != null)
            {
                var gameView = new views.GameView(loadFromSave: true);
                gameView.LoadGameState(gameState);
                gameView.ShowPauseMenuAfterLoad();
                NavigationService.Instance.NavigateTo(gameView);
            }
            else
            {
                NavigationService.Instance.NavigateToMainMenu();
            }
        }
        else
        {
            NavigationService.Instance.NavigateToMainMenu();
        }
    }
}
