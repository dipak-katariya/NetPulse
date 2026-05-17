using NetPulse.Native;
using Color = System.Windows.Media.Color;

namespace NetPulse.UI.Controls;

internal static class BackdropApplicator
{
    private const uint AcrylicDarkArgb = 0x99202020u;
    private const uint AcrylicLightArgb = 0x99F2F2F7u;
    private static readonly Color SolidDark = Color.FromArgb(0xCC, 0x20, 0x20, 0x20);
    private static readonly Color SolidLight = Color.FromArgb(0xCC, 0xF2, 0xF2, 0xF7);
    private static readonly Color Transparent = Color.FromArgb(0, 0, 0, 0);

    public static Color ChooseBackground(IntPtr hwnd, bool dark)
    {
        Dwm.TrySetDarkMode(hwnd, dark);
        Dwm.TrySetRoundCorners(hwnd, Dwm.DwmWindowCornerPreference.Round);

        if (Dwm.TrySetBackdrop(hwnd, Dwm.DwmSystemBackdropType.TransientWindow))
        {
            Dwm.ExtendFrameSheet(hwnd);
            return Transparent;
        }

        if (WindowComposition.TryApplyAcrylic(hwnd, dark ? AcrylicDarkArgb : AcrylicLightArgb))
        {
            return Transparent;
        }

        return dark ? SolidDark : SolidLight;
    }
}
