using System.Globalization;
using System.Windows;
using NetPulse.Core;
using Color = System.Windows.Media.Color;
using FontFamily = System.Windows.Media.FontFamily;
using SolidColorBrush = System.Windows.Media.SolidColorBrush;

namespace NetPulse.UI;

internal sealed class WidgetTheme
{
    public FontFamily Font { get; }
    public double FontSize { get; }
    public SolidColorBrush Primary { get; }
    public SolidColorBrush Secondary { get; }
    public SolidColorBrush DownAccent { get; } = new(Color.FromRgb(0x34, 0xC7, 0x59));
    public SolidColorBrush UpAccent { get; } = new(Color.FromRgb(0xFF, 0x9F, 0x0A));
    public bool IsDark { get; }

    public WidgetTheme(AppSettings settings, bool darkMode)
    {
        IsDark = darkMode;
        Font = ResolveFont(settings);
        FontSize = ResolveFontSize(settings);
        Primary = new SolidColorBrush(ResolveTextPrimary(settings, darkMode));
        Secondary = new SolidColorBrush(ResolveTextSecondary(darkMode));
    }

    private static FontFamily ResolveFont(AppSettings settings)
    {
        if (settings.UseSystemFont)
        {
            return SystemFonts.MessageFontFamily;
        }
        return string.IsNullOrWhiteSpace(settings.FontFamily)
            ? SystemFonts.MessageFontFamily
            : new FontFamily(settings.FontFamily);
    }

    private static double ResolveFontSize(AppSettings settings)
    {
        if (settings.UseSystemFont) return Math.Max(9, SystemFonts.MessageFontSize + 2);
        return Math.Clamp(settings.FontSize, 9, 32);
    }

    private static Color ResolveTextPrimary(AppSettings settings, bool darkMode)
    {
        if (!string.Equals(settings.FontColorHex, "Auto", StringComparison.OrdinalIgnoreCase)
            && TryParseColor(settings.FontColorHex, out Color parsed))
        {
            return parsed;
        }
        return darkMode ? Color.FromRgb(0xF2, 0xF2, 0xF7) : Color.FromRgb(0x1C, 0x1C, 0x1E);
    }

    private static Color ResolveTextSecondary(bool darkMode) =>
        darkMode ? Color.FromRgb(0x8E, 0x8E, 0x93) : Color.FromRgb(0x6E, 0x6E, 0x73);

    private static bool TryParseColor(string hex, out Color color)
    {
        color = default;
        if (string.IsNullOrWhiteSpace(hex)) return false;

        string trimmed = hex.Trim().TrimStart('#');
        if (trimmed.Length is not (6 or 8)) return false;

        if (!uint.TryParse(trimmed, NumberStyles.HexNumber, CultureInfo.InvariantCulture, out uint raw))
        {
            return false;
        }

        if (trimmed.Length == 6)
        {
            color = Color.FromRgb((byte)(raw >> 16), (byte)(raw >> 8), (byte)raw);
        }
        else
        {
            color = Color.FromArgb((byte)(raw >> 24), (byte)(raw >> 16), (byte)(raw >> 8), (byte)raw);
        }
        return true;
    }
}
