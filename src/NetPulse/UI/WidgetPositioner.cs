using System.Windows;
using System.Windows.Media;
using NetPulse.Native;

namespace NetPulse.UI;

internal static class WidgetPositioner
{
    private const double TrayLeftPadPx = 6;
    private const double TrayBottomPadPx = 6;

    public readonly record struct Placement(double Left, double Top);

    public static Placement DockInsideTaskbar(double widgetWidth, double widgetHeight, Visual? dpiSource)
    {
        Shell.TaskbarInfo? maybeBar = Shell.TryGetTaskbarInfo();
        if (maybeBar is not { } bar)
        {
            return BottomRightOfWorkArea(widgetWidth, widgetHeight);
        }

        Shell.ScreenRect? maybeTray = Shell.TryGetTrayNotifyRect();
        double scale = GetDpiScaleFactor(dpiSource);

        return bar.Edge switch
        {
            Shell.TaskbarEdge.Bottom or Shell.TaskbarEdge.Top => DockHorizontally(bar, maybeTray, widgetWidth, widgetHeight, scale),
            Shell.TaskbarEdge.Left or Shell.TaskbarEdge.Right => DockVertically(bar, maybeTray, widgetWidth, widgetHeight, scale),
            _ => BottomRightOfWorkArea(widgetWidth, widgetHeight),
        };
    }

    public static Placement OffsetFromBottomRight(
        double widgetWidth,
        double widgetHeight,
        double distanceFromRight,
        double distanceFromBottom)
    {
        Rect workArea = SystemParameters.WorkArea;
        double left = workArea.Right - widgetWidth - distanceFromRight;
        double top = workArea.Bottom - widgetHeight - distanceFromBottom;
        return Clamp(left, top, widgetWidth, widgetHeight);
    }

    private static Placement DockHorizontally(
        Shell.TaskbarInfo bar,
        Shell.ScreenRect? maybeTray,
        double widgetWidth,
        double widgetHeight,
        double scale)
    {
        double barTopDip = bar.Top / scale;
        double barBottomDip = bar.Bottom / scale;
        double barHeightDip = barBottomDip - barTopDip;
        double top = barTopDip + Math.Max(0, (barHeightDip - widgetHeight) / 2);

        double left;
        if (maybeTray is { } tray)
        {
            double trayLeftDip = tray.Left / scale;
            left = trayLeftDip - widgetWidth - TrayLeftPadPx;
        }
        else
        {
            left = (bar.Right / scale) - widgetWidth - TrayLeftPadPx;
        }

        return Clamp(left, top, widgetWidth, widgetHeight);
    }

    private static Placement DockVertically(
        Shell.TaskbarInfo bar,
        Shell.ScreenRect? maybeTray,
        double widgetWidth,
        double widgetHeight,
        double scale)
    {
        double barLeftDip = bar.Left / scale;
        double barRightDip = bar.Right / scale;
        double barWidthDip = barRightDip - barLeftDip;
        double left = barLeftDip + Math.Max(0, (barWidthDip - widgetWidth) / 2);

        double top;
        if (maybeTray is { } tray)
        {
            double trayTopDip = tray.Top / scale;
            top = trayTopDip - widgetHeight - TrayBottomPadPx;
        }
        else
        {
            top = (bar.Bottom / scale) - widgetHeight - TrayBottomPadPx;
        }

        return Clamp(left, top, widgetWidth, widgetHeight);
    }

    private static Placement BottomRightOfWorkArea(double widgetWidth, double widgetHeight)
    {
        Rect workArea = SystemParameters.WorkArea;
        double left = workArea.Right - widgetWidth - TrayLeftPadPx;
        double top = workArea.Bottom - widgetHeight - TrayBottomPadPx;
        return Clamp(left, top, widgetWidth, widgetHeight);
    }

    private static Placement Clamp(double left, double top, double width, double height)
    {
        double maxLeft = Math.Max(0, SystemParameters.VirtualScreenWidth - width);
        double maxTop = Math.Max(0, SystemParameters.VirtualScreenHeight - height);
        double clampedLeft = Math.Clamp(left, 0, maxLeft);
        double clampedTop = Math.Clamp(top, 0, maxTop);
        return new Placement(clampedLeft, clampedTop);
    }

    private static double GetDpiScaleFactor(Visual? visual)
    {
        if (visual is null) return 1.0;
        PresentationSource? source = PresentationSource.FromVisual(visual);
        return source?.CompositionTarget?.TransformToDevice.M11 ?? 1.0;
    }
}
