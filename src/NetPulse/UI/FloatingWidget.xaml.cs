using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Threading;
using Microsoft.Win32;
using NetPulse.Core;
using NetPulse.UI.Controls;
using ShapePath = System.Windows.Shapes.Path;

namespace NetPulse.UI;

public partial class FloatingWidget : Window
{
    private const int EdgeSnapPx = 16;
    private const int DefaultMarginPx = 12;
    private const int TaskbarSettleDelayMs = 350;

    private readonly AppSettings _settings;
    private readonly Action<AppSettings> _persistSettings;
    private readonly Action _openSettings;
    private readonly Action _onQuit;
    private readonly Action _onToggleAutostart;
    private readonly Func<bool> _isAutostartEnabled;
    private DispatcherTimer? _reanchorTimer;

    public FloatingWidget(
        AppSettings settings,
        Action<AppSettings> persistSettings,
        Action openSettings,
        Action onQuit,
        Action onToggleAutostart,
        Func<bool> isAutostartEnabled)
    {
        _settings = settings;
        _persistSettings = persistSettings;
        _openSettings = openSettings;
        _onQuit = onQuit;
        _onToggleAutostart = onToggleAutostart;
        _isAutostartEnabled = isAutostartEnabled;

        InitializeComponent();

        Loaded += OnLoaded;
        SourceInitialized += OnSourceInitialized;
        MouseLeftButtonDown += OnDragStart;
        WidgetContextMenu.Opened += OnContextMenuOpening;
        SystemEvents.DisplaySettingsChanged += OnDisplaySettingsChanged;
        SystemEvents.UserPreferenceChanged += OnUserPreferenceChanged;

        BuildContextMenu();
        ApplyLayoutAndTheme();
    }

    public void Apply(SpeedSnapshot snapshot)
    {
        if (snapshot.At == DateTimeOffset.MinValue) return;

        (string downValue, string downUnit) = ByteRateFormatter.Format(snapshot.DownBytesPerSecond, _settings.UnitSystem);
        (string upValue, string upUnit) = ByteRateFormatter.Format(snapshot.UpBytesPerSecond, _settings.UnitSystem);

        DownValueSingle.Text = downValue;
        DownUnitSingle.Text = downUnit;
        UpValueSingle.Text = upValue;
        UpUnitSingle.Text = upUnit;

        DownValueTwo.Text = downValue;
        DownUnitTwo.Text = downUnit;
        UpValueTwo.Text = upValue;
        UpUnitTwo.Text = upUnit;

        if (_settings.PinToTaskbar)
        {
            ReanchorIntoTaskbar();
        }
    }

    public void ToggleVisibility()
    {
        if (IsVisible) Hide();
        else Show();
        _settings.WidgetVisible = IsVisible;
        _persistSettings(_settings);
    }

    public void ApplyLayoutAndTheme()
    {
        bool single = _settings.LayoutMode == LayoutMode.SingleRow;
        SingleRowLayout.Visibility = single ? Visibility.Visible : Visibility.Collapsed;
        TwoRowLayout.Visibility = single ? Visibility.Collapsed : Visibility.Visible;
        ApplyCompactStyling();
        ApplyTheme();
    }

    public void RePinIfRequested()
    {
        if (!_settings.PinToTaskbar) return;
        ReanchorIntoTaskbar();
    }

    private void ApplyCompactStyling()
    {
        if (_settings.PinToTaskbar)
        {
            RootBorder.CornerRadius = new CornerRadius(6);
            RootBorder.Padding = new Thickness(10, 2, 10, 2);
        }
        else
        {
            RootBorder.CornerRadius = new CornerRadius(12);
            RootBorder.Padding = new Thickness(14, 6, 14, 6);
        }
    }

    private void ApplyTheme()
    {
        bool dark = ResolveDarkMode();
        WidgetTheme theme = new(_settings, dark);

        foreach (TextBlock block in EnumerateValueBlocks())
        {
            block.FontFamily = theme.Font;
            block.FontSize = theme.FontSize;
            block.Foreground = theme.Primary;
        }
        foreach (TextBlock block in EnumerateUnitBlocks())
        {
            block.FontFamily = theme.Font;
            block.FontSize = Math.Max(9, theme.FontSize - 3);
            block.Foreground = theme.Secondary;
        }
        DownArrowSingle.Stroke = theme.DownAccent;
        UpArrowSingle.Stroke = theme.UpAccent;
        DownArrowTwo.Stroke = theme.DownAccent;
        UpArrowTwo.Stroke = theme.UpAccent;
    }

    private bool ResolveDarkMode() => _settings.Theme switch
    {
        ThemePreference.Dark => true,
        ThemePreference.Light => false,
        _ => SystemTheme.IsDarkMode(),
    };

    private IEnumerable<TextBlock> EnumerateValueBlocks()
    {
        yield return DownValueSingle;
        yield return UpValueSingle;
        yield return DownValueTwo;
        yield return UpValueTwo;
    }

    private IEnumerable<TextBlock> EnumerateUnitBlocks()
    {
        yield return DownUnitSingle;
        yield return UpUnitSingle;
        yield return DownUnitTwo;
        yield return UpUnitTwo;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        RestorePosition();
    }

    private void OnSourceInitialized(object? sender, EventArgs e)
    {
        IntPtr hwnd = new WindowInteropHelper(this).Handle;
        NetPulse.Native.User32.ApplyToolWindowStyle(hwnd);
        BackdropBrush.Color = BackdropApplicator.ChooseBackground(hwnd, ResolveDarkMode());
    }

    private void OnDragStart(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton != MouseButton.Left) return;
        if (e.OriginalSource is ShapePath) return;
        if (_settings.PinToTaskbar) return;

        try { DragMove(); }
        catch (InvalidOperationException) { return; }

        SnapToEdges();
        PersistPosition();
    }

    private void OnDisplaySettingsChanged(object? sender, EventArgs e) => ScheduleReanchor();

    private void OnUserPreferenceChanged(object sender, UserPreferenceChangedEventArgs e)
    {
        if (e.Category == UserPreferenceCategory.General || e.Category == UserPreferenceCategory.Desktop)
        {
            ScheduleReanchor();
        }
    }

    private void ScheduleReanchor()
    {
        _reanchorTimer ??= new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(TaskbarSettleDelayMs) };
        _reanchorTimer.Tick -= OnReanchorTick;
        _reanchorTimer.Tick += OnReanchorTick;
        _reanchorTimer.Stop();
        _reanchorTimer.Start();
    }

    private void OnReanchorTick(object? sender, EventArgs e)
    {
        _reanchorTimer?.Stop();
        if (_settings.PinToTaskbar) ReanchorIntoTaskbar();
        else RestorePosition();
    }

    private void RestorePosition()
    {
        if (_settings.PinToTaskbar)
        {
            ReanchorIntoTaskbar();
        }
        else
        {
            WidgetPositioner.Placement placement = WidgetPositioner.OffsetFromBottomRight(
                ActualWidth,
                ActualHeight,
                _settings.WidgetDistanceFromRight,
                _settings.WidgetDistanceFromBottom);
            Left = placement.Left;
            Top = placement.Top;
        }
    }

    private void ReanchorIntoTaskbar()
    {
        WidgetPositioner.Placement placement = WidgetPositioner.DockInsideTaskbar(ActualWidth, ActualHeight, this);
        Left = placement.Left;
        Top = placement.Top;
    }

    private void SnapToEdges()
    {
        Rect workArea = SystemParameters.WorkArea;
        if (Math.Abs(Left - workArea.Left) < EdgeSnapPx) Left = workArea.Left + DefaultMarginPx;
        if (Math.Abs(workArea.Right - (Left + ActualWidth)) < EdgeSnapPx)
            Left = workArea.Right - ActualWidth - DefaultMarginPx;
        if (Math.Abs(Top - workArea.Top) < EdgeSnapPx) Top = workArea.Top + DefaultMarginPx;
        if (Math.Abs(workArea.Bottom - (Top + ActualHeight)) < EdgeSnapPx)
            Top = workArea.Bottom - ActualHeight - DefaultMarginPx;
    }

    private void PersistPosition()
    {
        Rect workArea = SystemParameters.WorkArea;
        _settings.WidgetDistanceFromRight = Math.Max(0, workArea.Right - (Left + ActualWidth));
        _settings.WidgetDistanceFromBottom = Math.Max(0, workArea.Bottom - (Top + ActualHeight));
        _persistSettings(_settings);
    }

    private void BuildContextMenu()
    {
        WidgetContextMenu.Items.Clear();

        MenuItem settingsItem = new() { Header = "Settings…" };
        settingsItem.Click += (_, _) => _openSettings();

        MenuItem pinItem = new()
        {
            Header = "Dock inside taskbar",
            IsCheckable = true,
            IsChecked = _settings.PinToTaskbar,
        };
        pinItem.Click += (_, _) =>
        {
            _settings.PinToTaskbar = pinItem.IsChecked;
            _persistSettings(_settings);
            ApplyCompactStyling();
            RestorePosition();
        };

        MenuItem autostartItem = new()
        {
            Header = "Start with Windows",
            IsCheckable = true,
            IsChecked = _isAutostartEnabled(),
        };
        autostartItem.Click += (_, _) => _onToggleAutostart();

        MenuItem quitItem = new() { Header = "Quit NetPulse" };
        quitItem.Click += (_, _) => _onQuit();

        WidgetContextMenu.Items.Add(settingsItem);
        WidgetContextMenu.Items.Add(pinItem);
        WidgetContextMenu.Items.Add(autostartItem);
        WidgetContextMenu.Items.Add(new Separator());
        WidgetContextMenu.Items.Add(quitItem);
    }

    private void OnContextMenuOpening(object sender, RoutedEventArgs e)
    {
        foreach (object item in WidgetContextMenu.Items)
        {
            if (item is MenuItem { Header: "Dock inside taskbar" } pin)
            {
                pin.IsChecked = _settings.PinToTaskbar;
            }
            else if (item is MenuItem { Header: "Start with Windows" } autostart)
            {
                autostart.IsChecked = _isAutostartEnabled();
            }
        }
    }

    protected override void OnClosed(EventArgs e)
    {
        SystemEvents.DisplaySettingsChanged -= OnDisplaySettingsChanged;
        SystemEvents.UserPreferenceChanged -= OnUserPreferenceChanged;
        _reanchorTimer?.Stop();
        base.OnClosed(e);
    }
}
