using System.Diagnostics;
using System.Windows;
using NetPulse.Core;
using NetPulse.UI;
using Application = System.Windows.Application;

namespace NetPulse;

public partial class App : Application
{
    private SingleInstanceGuard? _instanceGuard;
    private SettingsService? _settingsService;
    private AppSettings? _settings;
    private SpeedSampler? _sampler;
    private FloatingWidget? _widget;
    private TrayIconHost? _tray;
    private AutostartManager? _autostart;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        _instanceGuard = new SingleInstanceGuard();
        if (!_instanceGuard.IsFirstInstance)
        {
            Shutdown();
            return;
        }

        _settingsService = new SettingsService();
        _settings = _settingsService.Load();
        _autostart = new AutostartManager();

        ApplyAutostartFromMode();

        bool launchedHidden = e.Args.Contains("--hidden", StringComparer.OrdinalIgnoreCase);
        bool launchedAutostart = e.Args.Contains("--autostart", StringComparer.OrdinalIgnoreCase);

        InitWidget();
        InitSampler();
        InitTray();

        if (_widget is not null && _settings.WidgetVisible && !launchedHidden && !(launchedAutostart && _settings.StartHidden))
        {
            _widget.Show();
        }

        if (!_settings.HasCompletedFirstRun)
        {
            PromptFirstRun();
        }
    }

    private void InitWidget()
    {
        _widget = new FloatingWidget(
            _settings!,
            PersistSettings,
            OpenSettings,
            onQuit: () => Shutdown(),
            onToggleAutostart: ToggleAutostartFromMenu,
            isAutostartEnabled: () => _autostart!.IsEnabled());
    }

    private void InitSampler()
    {
        _sampler = new SpeedSampler();
        _sampler.SnapshotReady += snapshot =>
        {
            FloatingWidget? widget = _widget;
            if (widget is null) return;
            Dispatcher.BeginInvoke(() => widget.Apply(snapshot));
        };
        _sampler.Start();
    }

    private void InitTray()
    {
        _tray = new TrayIconHost(
            onToggleWidget: () => _widget?.ToggleVisibility(),
            onOpenSettings: OpenSettings,
            onQuit: () => Shutdown(),
            onSetAutostart: SetAutostartFromTray,
            getAutostartEnabled: () => _autostart!.IsEnabled());
    }

    private void ApplyAutostartFromMode()
    {
        if (_settings is null || _autostart is null) return;

        switch (_settings.AutostartMode)
        {
            case AutostartMode.AlwaysRun:
                EnableAutostart();
                break;
            case AutostartMode.OneTimeOnly:
            case AutostartMode.Disabled:
                _autostart.Disable();
                break;
        }
    }

    private void EnableAutostart()
    {
        if (_settings is null || _autostart is null) return;
        string exe = Environment.ProcessPath ?? AppContext.BaseDirectory;
        _autostart.Enable(exe, startHidden: _settings.StartHidden);
    }

    private void PromptFirstRun()
    {
        if (_settings is null) return;

        MessageBoxResult result = MessageBox.Show(
            "Welcome to NetPulse.\n\nStart NetPulse automatically when you sign in to Windows?",
            "NetPulse",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        _settings.AutostartMode = result == MessageBoxResult.Yes
            ? AutostartMode.AlwaysRun
            : AutostartMode.OneTimeOnly;
        _settings.HasCompletedFirstRun = true;
        ApplyAutostartFromMode();
        PersistSettings(_settings);
    }

    private void OpenSettings()
    {
        if (_settings is null) return;

        SettingsWindow window = new(_settings, SignatureInspector.ResolveSubject)
        {
            Owner = _widget,
        };
        bool? closedNormally = window.ShowDialog();
        if (closedNormally is true && window.Saved)
        {
            PersistSettings(_settings);
            ApplyAutostartFromMode();
            _widget?.ApplyLayoutAndTheme();
            _widget?.RePinIfRequested();
        }
    }

    private void PersistSettings(AppSettings settings)
    {
        try { _settingsService?.Save(settings); }
        catch (IOException ex) { Debug.WriteLine($"Settings IO failure: {ex.Message}"); }
        catch (UnauthorizedAccessException ex) { Debug.WriteLine($"Settings access denied: {ex.Message}"); }
    }

    private void ToggleAutostartFromMenu()
    {
        if (_settings is null || _autostart is null) return;
        _settings.AutostartMode = _autostart.IsEnabled() ? AutostartMode.Disabled : AutostartMode.AlwaysRun;
        ApplyAutostartFromMode();
        PersistSettings(_settings);
    }

    private void SetAutostartFromTray(bool enable)
    {
        if (_settings is null) return;
        _settings.AutostartMode = enable ? AutostartMode.AlwaysRun : AutostartMode.Disabled;
        ApplyAutostartFromMode();
        PersistSettings(_settings);
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _sampler?.Dispose();
        _tray?.Dispose();
        _widget?.Close();
        _instanceGuard?.Dispose();
        base.OnExit(e);
    }
}
