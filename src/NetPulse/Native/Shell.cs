using System.Runtime.InteropServices;

namespace NetPulse.Native;

internal static partial class Shell
{
    public enum TaskbarEdge
    {
        Left = 0,
        Top = 1,
        Right = 2,
        Bottom = 3,
    }

    public readonly record struct TaskbarInfo(TaskbarEdge Edge, int Left, int Top, int Right, int Bottom)
    {
        public int Width => Right - Left;
        public int Height => Bottom - Top;
    }

    public readonly record struct ScreenRect(int Left, int Top, int Right, int Bottom)
    {
        public int Width => Right - Left;
        public int Height => Bottom - Top;
    }

    private const uint AbmGetTaskbarPos = 0x00000005;

    [StructLayout(LayoutKind.Sequential)]
    private struct AppBarData
    {
        public uint cbSize;
        public IntPtr hWnd;
        public uint uCallbackMessage;
        public uint uEdge;
        public Rect rc;
        public IntPtr lParam;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Rect
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    [LibraryImport("shell32.dll")]
    private static partial UIntPtr SHAppBarMessage(uint dwMessage, ref AppBarData pData);

    [LibraryImport("user32.dll", EntryPoint = "FindWindowW", StringMarshalling = StringMarshalling.Utf16)]
    private static partial IntPtr FindWindow(string? className, string? windowTitle);

    [LibraryImport("user32.dll", EntryPoint = "FindWindowExW", StringMarshalling = StringMarshalling.Utf16)]
    private static partial IntPtr FindWindowEx(IntPtr parent, IntPtr child, string? className, string? windowTitle);

    [LibraryImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static partial bool GetWindowRect(IntPtr hwnd, out Rect rect);

    public static TaskbarInfo? TryGetTaskbarInfo()
    {
        AppBarData data = default;
        data.cbSize = (uint)Marshal.SizeOf<AppBarData>();
        UIntPtr result = SHAppBarMessage(AbmGetTaskbarPos, ref data);
        if (result == UIntPtr.Zero) return null;

        return new TaskbarInfo(
            (TaskbarEdge)data.uEdge,
            data.rc.Left,
            data.rc.Top,
            data.rc.Right,
            data.rc.Bottom);
    }

    public static ScreenRect? TryGetTrayNotifyRect()
    {
        IntPtr tray = FindWindow("Shell_TrayWnd", null);
        if (tray == IntPtr.Zero) return null;

        IntPtr notify = FindWindowEx(tray, IntPtr.Zero, "TrayNotifyWnd", null);
        if (notify == IntPtr.Zero) return null;

        if (!GetWindowRect(notify, out Rect rect)) return null;
        return new ScreenRect(rect.Left, rect.Top, rect.Right, rect.Bottom);
    }
}
