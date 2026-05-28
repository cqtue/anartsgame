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

    private double _originalMasterVolume;
    private double _originalSoundVolume;
    private double _originalMusicVolume;
    private bool _originalIsFullscreen;
    private bool _originalIsVsyncEnabled;

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

        SaveCommand = new RelayCommand(_ => SaveSettings());
        BackCommand = new RelayCommand(_ => NavigateBack());
    }

    private void CheckForChanges()
    {
        HasUnsavedChanges = _masterVolume != _originalMasterVolume ||
                           _soundVolume != _originalSoundVolume ||
                           _musicVolume != _originalMusicVolume ||
                           _isFullscreen != _originalIsFullscreen ||
                           _isVsyncEnabled != _originalIsVsyncEnabled;
    }

    private void SaveSettings()
    {
        _settingsService.MasterVolume = _masterVolume;
        _settingsService.SoundVolume = _soundVolume;
        _settingsService.MusicVolume = _musicVolume;
        _settingsService.IsFullscreen = _isFullscreen;
        _settingsService.IsVsyncEnabled = _isVsyncEnabled;
        _settingsService.Save();

        if (_isFullscreen != _originalIsFullscreen)
            _settingsService.ApplyFullscreenMode();

        _originalMasterVolume = _masterVolume;
        _originalSoundVolume = _soundVolume;
        _originalMusicVolume = _musicVolume;
        _originalIsFullscreen = _isFullscreen;
        _originalIsVsyncEnabled = _isVsyncEnabled;

        HasUnsavedChanges = false;
    }

    private void NavigateBack()
    {
        if (HasUnsavedChanges)
        {
            DialogService.Instance.ShowConfirmDialog(
                "у вас є незбережені зміни. відхилити їх?",
                () => NavigationService.Instance.NavigateToMainMenu(),
                () => { }
            );
        }
        else
        {
            NavigationService.Instance.NavigateToMainMenu();
        }
    }
}
