using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using WinForms = System.Windows.Forms;

namespace NetPulse.UI;

public sealed class TrayIconHost : IDisposable
{
    private readonly WinForms.NotifyIcon _notifyIcon;
    private readonly Action _onToggleWidget;
    private readonly Action _onOpenSettings;
    private readonly Action _onQuit;
    private readonly Action<bool> _onSetAutostart;
    private readonly Func<bool> _getAutostartEnabled;
    private readonly WinForms.ToolStripMenuItem _autostartItem;
    private Icon? _ownedIcon;
    private bool _disposed;

    public TrayIconHost(
        Action onToggleWidget,
        Action onOpenSettings,
        Action onQuit,
        Action<bool> onSetAutostart,
        Func<bool> getAutostartEnabled)
    {
        _onToggleWidget = onToggleWidget;
        _onOpenSettings = onOpenSettings;
        _onQuit = onQuit;
        _onSetAutostart = onSetAutostart;
        _getAutostartEnabled = getAutostartEnabled;

        _ownedIcon = BuildFallbackIcon();

        _autostartItem = new WinForms.ToolStripMenuItem("Start with Windows")
        {
            CheckOnClick = true,
            Checked = _getAutostartEnabled(),
        };
        _autostartItem.CheckedChanged += (_, _) => _onSetAutostart(_autostartItem.Checked);

        _notifyIcon = new WinForms.NotifyIcon
        {
            Icon = _ownedIcon,
            Text = "NetPulse",
            Visible = true,
            ContextMenuStrip = BuildMenu(),
        };
        _notifyIcon.MouseClick += OnIconClick;
    }

    private WinForms.ContextMenuStrip BuildMenu()
    {
        WinForms.ContextMenuStrip menu = new();
        menu.Opening += (_, _) => _autostartItem.Checked = _getAutostartEnabled();

        WinForms.ToolStripMenuItem toggleItem = new("Show / Hide widget");
        toggleItem.Click += (_, _) => _onToggleWidget();

        WinForms.ToolStripMenuItem settingsItem = new("Settings…");
        settingsItem.Click += (_, _) => _onOpenSettings();

        WinForms.ToolStripMenuItem quitItem = new("Quit NetPulse");
        quitItem.Click += (_, _) => _onQuit();

        menu.Items.Add(toggleItem);
        menu.Items.Add(settingsItem);
        menu.Items.Add(_autostartItem);
        menu.Items.Add(new WinForms.ToolStripSeparator());
        menu.Items.Add(quitItem);
        return menu;
    }

    private void OnIconClick(object? sender, WinForms.MouseEventArgs e)
    {
        if (e.Button == WinForms.MouseButtons.Left) _onToggleWidget();
    }

    private static Icon BuildFallbackIcon()
    {
        const int size = 16;
        using Bitmap bmp = new(size, size, PixelFormat.Format32bppArgb);
        using (Graphics g = Graphics.FromImage(bmp))
        {
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using SolidBrush bg = new(Color.FromArgb(0x20, 0x20, 0x20));
            g.FillRectangle(bg, 0, 0, size, size);
            using Pen down = new(Color.FromArgb(0x34, 0xC7, 0x59), 2f);
            using Pen up = new(Color.FromArgb(0xFF, 0x9F, 0x0A), 2f);
            g.DrawLine(down, 4, 2, 4, 8);
            g.DrawLine(down, 4, 8, 2, 6);
            g.DrawLine(down, 4, 8, 6, 6);
            g.DrawLine(up, 11, 14, 11, 8);
            g.DrawLine(up, 11, 8, 9, 10);
            g.DrawLine(up, 11, 8, 13, 10);
        }
        IntPtr hIcon = bmp.GetHicon();
        Icon shared = Icon.FromHandle(hIcon);
        using MemoryStream ms = new();
        shared.Save(ms);
        ms.Position = 0;
        return new Icon(ms);
    }

    public void Dispose()
    {
        if (_disposed) return;
        _disposed = true;
        _notifyIcon.Visible = false;
        _notifyIcon.ContextMenuStrip?.Dispose();
        _notifyIcon.Dispose();
        _ownedIcon?.Dispose();
    }
}
