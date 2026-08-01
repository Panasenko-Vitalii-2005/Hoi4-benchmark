using System.Collections.ObjectModel;
using Microsoft.Win32;
using HOI4Benchmark.App.Commands;
using HOI4Benchmark.Application.Abstractions;
using HOI4Benchmark.Domain.Settings;
using HOI4Benchmark.App.Services;

namespace HOI4Benchmark.App.ViewModels;

public sealed class SettingsViewModel : ViewModelBase
{
    private const string DefaultAutosavePath =
        @"Documents\Paradox Interactive\Hearts of Iron IV\save games";

    private readonly ISettingsService _settingsService;
    private readonly IDiagnosticsBundleService _diagnosticsBundleService;

    private string _autosavePath = DefaultAutosavePath;
    private string _measurementIntervalSeconds = "1";
    private string _selectedExportFormat = "JSON";
    private bool _saveResultsAutomatically = true;
    private bool _watchSubdirectories;
    private bool _showNotifications = true;
    private string _statusMessage = "Loading settings...";
    private bool _hasUnsavedChanges;
    private bool _isLoading;

    public SettingsViewModel(
    ISettingsService settingsService,
    IDiagnosticsBundleService diagnosticsBundleService)
    {
        _settingsService = settingsService
        ?? throw new ArgumentNullException(
            nameof(settingsService));
        _diagnosticsBundleService =
    diagnosticsBundleService
    ?? throw new ArgumentNullException(
        nameof(diagnosticsBundleService));

        ExportFormats = ["JSON", "CSV"];

        SaveCommand = new RelayCommand(
            Save,
            CanSave);

        ResetCommand = new RelayCommand(
            ResetToDefaults,
            () => !IsLoading);

        CreateDiagnosticsBundleCommand =
    new RelayCommand(
        CreateDiagnosticsBundle,
        () => !IsLoading);

        LoadSettings();
    }

    public string Title => "Settings";

    public string Subtitle =>
        "Configure autosave monitoring and benchmark result storage";

    public ObservableCollection<string> ExportFormats { get; }

    public string AutosavePath
    {
        get => _autosavePath;
        set
        {
            if (SetProperty(ref _autosavePath, value))
            {
                MarkAsChanged();
            }
        }
    }

    public string MeasurementIntervalSeconds
    {
        get => _measurementIntervalSeconds;
        set
        {
            if (SetProperty(
                    ref _measurementIntervalSeconds,
                    value))
            {
                OnPropertyChanged(
                    nameof(IntervalValidationMessage));

                OnPropertyChanged(
                    nameof(HasIntervalError));

                MarkAsChanged();
            }
        }
    }

    public string SelectedExportFormat
    {
        get => _selectedExportFormat;
        set
        {
            if (SetProperty(
                    ref _selectedExportFormat,
                    value))
            {
                MarkAsChanged();
            }
        }
    }

    public bool SaveResultsAutomatically
    {
        get => _saveResultsAutomatically;
        set
        {
            if (SetProperty(
                    ref _saveResultsAutomatically,
                    value))
            {
                MarkAsChanged();
            }
        }
    }

    public bool WatchSubdirectories
    {
        get => _watchSubdirectories;
        set
        {
            if (SetProperty(
                    ref _watchSubdirectories,
                    value))
            {
                MarkAsChanged();
            }
        }
    }

    public bool ShowNotifications
    {
        get => _showNotifications;
        set
        {
            if (SetProperty(
                    ref _showNotifications,
                    value))
            {
                MarkAsChanged();
            }
        }
    }

    public bool HasUnsavedChanges
    {
        get => _hasUnsavedChanges;
        private set
        {
            if (SetProperty(
                    ref _hasUnsavedChanges,
                    value))
            {
                OnPropertyChanged(nameof(SettingsState));
                SaveCommand.RaiseCanExecuteChanged();
            }
        }
    }

    public bool IsLoading
{
    get => _isLoading;
    private set
    {
        if (SetProperty(ref _isLoading, value))
        {
            SaveCommand.RaiseCanExecuteChanged();
            ResetCommand.RaiseCanExecuteChanged();

            CreateDiagnosticsBundleCommand
                .RaiseCanExecuteChanged();
        }
    }
}

    public string SettingsState =>
        HasUnsavedChanges
            ? "Unsaved changes"
            : "All changes saved";

    public bool HasIntervalError =>
        !TryGetMeasurementInterval(out _);

    public string IntervalValidationMessage
    {
        get
        {
            if (string.IsNullOrWhiteSpace(
                    MeasurementIntervalSeconds))
            {
                return "Measurement interval is required.";
            }

            if (!int.TryParse(
                    MeasurementIntervalSeconds,
                    out int interval))
            {
                return "Measurement interval must be a whole number.";
            }

            if (interval is < 1 or > 60)
            {
                return "Measurement interval must be between 1 and 60 seconds.";
            }

            return string.Empty;
        }
    }

    public string StatusMessage
    {
        get => _statusMessage;
        private set =>
            SetProperty(ref _statusMessage, value);
    }

    public RelayCommand SaveCommand { get; }

    public RelayCommand ResetCommand { get; }

    public RelayCommand CreateDiagnosticsBundleCommand { get; }

    private bool CanSave()
    {
        return !IsLoading
               && HasUnsavedChanges
               && !string.IsNullOrWhiteSpace(AutosavePath)
               && TryGetMeasurementInterval(out _);
    }

    private async void LoadSettings()
    {
        try
        {
            IsLoading = true;

            BenchmarkSettings settings =
                await _settingsService.GetSettingsAsync();

            ApplySettings(settings);

            HasUnsavedChanges = false;
            StatusMessage = "Settings loaded.";
        }
        catch (Exception exception)
{
    StatusMessage =
        ErrorMessageProvider.GetMessage(
            "Could not load settings",
            exception);
}
        finally
        {
            IsLoading = false;
        }
    }

    private async void Save()
    {
        if (!TryGetMeasurementInterval(out int interval))
        {
            StatusMessage =
                "Settings could not be saved. Check the measurement interval.";

            return;
        }

        try
        {
            IsLoading = true;

            BenchmarkSettings currentSettings =
                await _settingsService.GetSettingsAsync();

            currentSettings.SavePath =
                AutosavePath.Trim();

            currentSettings.MeasurementIntervalSeconds =
                interval;

            currentSettings.DefaultExportFormat =
                SelectedExportFormat;

            currentSettings.SaveResultsAutomatically =
                SaveResultsAutomatically;

            currentSettings.WatchSubdirectories =
                WatchSubdirectories;

            currentSettings.ShowNotifications =
                ShowNotifications;

            await _settingsService.UpdateSettingsAsync(
                currentSettings);

            HasUnsavedChanges = false;

            StatusMessage =
                "Settings saved successfully.";
        }
        catch (Exception exception)
{
    StatusMessage =
        ErrorMessageProvider.GetMessage(
            "Could not save settings",
            exception);
}
        finally
        {
            IsLoading = false;
        }
    }

    private async void ResetToDefaults()
    {
        try
        {
            IsLoading = true;

            await _settingsService.ResetSettingsAsync();

            BenchmarkSettings settings =
                await _settingsService.GetSettingsAsync();

            ApplySettings(settings);

            HasUnsavedChanges = false;

            StatusMessage =
                "Default settings restored.";
        }
        catch (Exception exception)
{
    StatusMessage =
        ErrorMessageProvider.GetMessage(
            "Could not restore default settings",
            exception);
}
        finally
        {
            IsLoading = false;
        }
    }

    private void ApplySettings(
        BenchmarkSettings settings)
    {
        _autosavePath =
            string.IsNullOrWhiteSpace(settings.SavePath)
                ? DefaultAutosavePath
                : settings.SavePath;

        _measurementIntervalSeconds =
            settings.MeasurementIntervalSeconds.ToString();

        _selectedExportFormat =
            string.IsNullOrWhiteSpace(
                settings.DefaultExportFormat)
                ? "JSON"
                : settings.DefaultExportFormat;

        _saveResultsAutomatically =
            settings.SaveResultsAutomatically;

        _watchSubdirectories =
            settings.WatchSubdirectories;

        _showNotifications =
            settings.ShowNotifications;

        OnPropertyChanged(nameof(AutosavePath));
        OnPropertyChanged(
            nameof(MeasurementIntervalSeconds));

        OnPropertyChanged(nameof(SelectedExportFormat));
        OnPropertyChanged(
            nameof(SaveResultsAutomatically));

        OnPropertyChanged(nameof(WatchSubdirectories));
        OnPropertyChanged(nameof(ShowNotifications));
        OnPropertyChanged(
            nameof(IntervalValidationMessage));

        OnPropertyChanged(nameof(HasIntervalError));
    }

    private bool TryGetMeasurementInterval(
        out int interval)
    {
        return int.TryParse(
                   MeasurementIntervalSeconds,
                   out interval)
               && interval is >= 1 and <= 60;
    }

    private void MarkAsChanged()
    {
        if (IsLoading)
        {
            return;
        }

        HasUnsavedChanges = true;
        StatusMessage = "Settings have unsaved changes.";

        SaveCommand.RaiseCanExecuteChanged();
    }

  private async void CreateDiagnosticsBundle()
{
    var dialog = new SaveFileDialog
    {
        Title = "Save diagnostics bundle",
        FileName =
            $"hoi4-benchmark-diagnostics-" +
            $"{DateTime.Now:yyyy-MM-dd-HH-mm-ss}.zip",

        DefaultExt = ".zip",
        AddExtension = true,
        OverwritePrompt = true,
        Filter = "ZIP archives (*.zip)|*.zip"
    };

    if (dialog.ShowDialog() != true)
    {
        StatusMessage =
            "Diagnostics bundle creation was cancelled.";

        return;
    }

    try
    {
        IsLoading = true;

        await _diagnosticsBundleService
            .CreateBundleAsync(
                dialog.FileName);

        StatusMessage =
            $"Diagnostics bundle created: {dialog.FileName}";
    }
    catch (Exception exception)
    {
        StatusMessage =
            ErrorMessageProvider.GetMessage(
                "Could not create diagnostics bundle",
                exception);
    }
    finally
    {
        IsLoading = false;
    }
}
}