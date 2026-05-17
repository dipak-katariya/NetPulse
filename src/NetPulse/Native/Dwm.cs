using System.Runtime.InteropServices;

namespace NetPulse.Native;

internal static partial class Dwm
{
    public enum DwmWindowAttribute : int
    {
        UseImmersiveDarkMode = 20,
        WindowCornerPreference = 33,
        SystemBackdropType = 38,
    }

    public enum DwmSystemBackdropType : int
    {
        Auto = 0,
        None = 1,
        MainWindow = 2,      // Mica
        TransientWindow = 3, // Acrylic
        TabbedWindow = 4,    // Mica Alt
    }

    public enum DwmWindowCornerPreference : int
    {
        Default = 0,
        DoNotRound = 1,
        Round = 2,
        RoundSmall = 3,
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Margins
    {
        public int LeftWidth;
        public int RightWidth;
        public int TopHeight;
        public int BottomHeight;
    }

    [LibraryImport("dwmapi.dll")]
    public static partial int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int value, int size);

    [LibraryImport("dwmapi.dll")]
    public static partial int DwmExtendFrameIntoClientArea(IntPtr hwnd, ref Margins margins);

    public static bool TrySetBackdrop(IntPtr hwnd, DwmSystemBackdropType backdrop)
    {
        if (Environment.OSVersion.Version.Build < 22000) return false;
        int value = (int)backdrop;
        return DwmSetWindowAttribute(hwnd, (int)DwmWindowAttribute.SystemBackdropType, ref value, sizeof(int)) == 0;
    }

    public static void TrySetRoundCorners(IntPtr hwnd, DwmWindowCornerPreference pref)
    {
        if (Environment.OSVersion.Version.Build < 22000) return;
        int value = (int)pref;
        DwmSetWindowAttribute(hwnd, (int)DwmWindowAttribute.WindowCornerPreference, ref value, sizeof(int));
    }

    public static void TrySetDarkMode(IntPtr hwnd, bool dark)
    {
        if (Environment.OSVersion.Version.Build < 17763) return;
        int value = dark ? 1 : 0;
        DwmSetWindowAttribute(hwnd, (int)DwmWindowAttribute.UseImmersiveDarkMode, ref value, sizeof(int));
    }

    public static void ExtendFrameSheet(IntPtr hwnd)
    {
        Margins m = new() { LeftWidth = -1, RightWidth = -1, TopHeight = -1, BottomHeight = -1 };
        DwmExtendFrameIntoClientArea(hwnd, ref m);
    }
}
