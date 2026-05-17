using System.Reflection;
using System.Windows;
using NetPulse.Core;
using ComboBox = System.Windows.Controls.ComboBox;
using ComboBoxItem = System.Windows.Controls.ComboBoxItem;

namespace NetPulse.UI;

public partial class SettingsWindow : Window
{
    private readonly AppSettings _settings;
    private readonly Func<string?> _resolveSignatureSubject;

    public bool Saved { get; private set; }

    public SettingsWindow(AppSettings settings, Func<string?> resolveSignatureSubject)
    {
        _settings = settings;
        _resolveSignatureSubject = resolveSignatureSubject;

        InitializeComponent();
        PopulateFontFamilies();
        LoadFromSettings();
        UpdateAboutSignature();
    }

    private void PopulateFontFamilies()
    {
        foreach (System.Windows.Media.FontFamily family in System.Windows.Media.Fonts.SystemFontFamilies
                     .OrderBy(f => f.Source, StringComparer.OrdinalIgnoreCase))
        {
            FontFamilyCombo.Items.Add(family.Source);
        }
    }

    private void LoadFromSettings()
    {
        SelectComboByTag(LayoutCombo, _settings.LayoutMode.ToString());
        SelectComboByTag(UnitsCombo, _settings.UnitSystem.ToString());
        SelectComboByTag(ThemeCombo, _settings.Theme.ToString());

        PinToTaskbarCheck.IsChecked = _settings.PinToTaskbar;
        UseSystemFontCheck.IsChecked = _settings.UseSystemFont;
        FontFamilyCombo.Text = _settings.FontFamily;
        FontSizeCombo.Text = _settings.FontSize.ToString(System.Globalization.CultureInfo.InvariantCulture);
        FontColorBox.Text = _settings.FontColorHex;

        UpdateFontControlsEnabled();

        AutostartAlwaysRadio.IsChecked = _settings.AutostartMode == AutostartMode.AlwaysRun;
        AutostartOneTimeRadio.IsChecked = _settings.AutostartMode == AutostartMode.OneTimeOnly;
        AutostartDisabledRadio.IsChecked = _settings.AutostartMode == AutostartMode.Disabled;
        StartHiddenCheck.IsChecked = _settings.StartHidden;

        Version version = Assembly.GetExecutingAssembly().GetName().Version ?? new Version(0, 1, 0);
        AboutVersionText.Text = $"NetPulse {version.ToString(3)}";
    }

    private void UpdateAboutSignature()
    {
        string? subject = _resolveSignatureSubject();
        AboutSignatureText.Text = string.IsNullOrEmpty(subject)
            ? "Signature: not signed"
            : $"Signature: {subject}";
    }

    private void OnUseSystemFontClick(object sender, RoutedEventArgs e) => UpdateFontControlsEnabled();

    private void UpdateFontControlsEnabled()
    {
        bool custom = UseSystemFontCheck.IsChecked != true;
        FontFamilyCombo.IsEnabled = custom;
        FontSizeCombo.IsEnabled = custom;
        FontColorBox.IsEnabled = custom;
    }

    private void OnCancelClick(object sender, RoutedEventArgs e)
    {
        Saved = false;
        Close();
    }

    private void OnSaveClick(object sender, RoutedEventArgs e)
    {
        if (!TryReadInto(_settings, out string? error))
        {
            MessageBox.Show(this, error, "NetPulse", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        Saved = true;
        Close();
    }

    private bool TryReadInto(AppSettings target, out string? error)
    {
        error = null;

        target.LayoutMode = ReadEnum(LayoutCombo, LayoutMode.SingleRow);
        target.UnitSystem = ReadEnum(UnitsCombo, UnitSystem.Bytes);
        target.Theme = ReadEnum(ThemeCombo, ThemePreference.System);
        target.PinToTaskbar = PinToTaskbarCheck.IsChecked == true;

        target.UseSystemFont = UseSystemFontCheck.IsChecked == true;
        target.FontFamily = string.IsNullOrWhiteSpace(FontFamilyCombo.Text) ? "Segoe UI" : FontFamilyCombo.Text.Trim();

        if (!double.TryParse(FontSizeCombo.Text, System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture, out double size)
            || size < 9 || size > 32)
        {
            error = "Font size must be a number between 9 and 32.";
            return false;
        }
        target.FontSize = size;

        string color = FontColorBox.Text?.Trim() ?? "Auto";
        if (!string.Equals(color, "Auto", StringComparison.OrdinalIgnoreCase) && !IsValidHex(color))
        {
            error = "Font color must be 'Auto', or a hex value like #F2F2F7 or #CCF2F2F7.";
            return false;
        }
        target.FontColorHex = color;

        target.AutostartMode = AutostartAlwaysRadio.IsChecked == true
            ? AutostartMode.AlwaysRun
            : AutostartOneTimeRadio.IsChecked == true
                ? AutostartMode.OneTimeOnly
                : AutostartMode.Disabled;

        target.StartHidden = StartHiddenCheck.IsChecked == true;
        return true;
    }

    private static T ReadEnum<T>(ComboBox combo, T fallback) where T : struct, Enum
    {
        if (combo.SelectedItem is ComboBoxItem { Tag: string tag } && Enum.TryParse(tag, out T value))
        {
            return value;
        }
        return fallback;
    }

    private static void SelectComboByTag(ComboBox combo, string tag)
    {
        foreach (object item in combo.Items)
        {
            if (item is ComboBoxItem ci && string.Equals(ci.Tag as string, tag, StringComparison.Ordinal))
            {
                combo.SelectedItem = ci;
                return;
            }
        }
        if (combo.Items.Count > 0) combo.SelectedIndex = 0;
    }

    private static bool IsValidHex(string color)
    {
        string trimmed = color.TrimStart('#');
        if (trimmed.Length is not (6 or 8)) return false;
        foreach (char c in trimmed)
        {
            if (!Uri.IsHexDigit(c)) return false;
        }
        return true;
    }
}
