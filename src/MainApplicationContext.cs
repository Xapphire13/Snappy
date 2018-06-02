using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Snappy.Properties;
using static PInvoke.User32;

namespace Snappy
{
    public class MainApplicationContext : ApplicationContext
    {
        private static readonly string[] protectedProcesses = new[]
        {
            "SearchUI"
        };
        
        private NotifyIcon trayIcon;
        private HotKeyHandler hotKeyHandler;

        private enum Direction
        {
            Up,
            Down
        }

        public MainApplicationContext()
        {
            this.trayIcon = new NotifyIcon
            {
                Icon = SystemIcons.WinLogo,
                Text = Resources.ApplicationName,
                Visible = true,
                ContextMenu = new ContextMenu(new [] {
                    new MenuItem(Resources.Actions_Website, (object sender, EventArgs e) => Process.Start("https://github.com/Xapphire13/Snappy")),
                    new MenuItem(Resources.Actions_Quit, this.Quit)
                })
            };

            var mods = HotKeyHandler.MOD_WIN | HotKeyHandler.MOD_ALT | HotKeyHandler.MOD_NOREPEAT;
            this.hotKeyHandler = new HotKeyHandler(new (uint, VirtualKey, Action)[]
            {
                ( mods, VirtualKey.VK_UP, () => this.ResizeWindow(Direction.Up) ),
                ( mods, VirtualKey.VK_DOWN, () => this.ResizeWindow(Direction.Down) )
            });
        }

        private void ResizeWindow(Direction direction)
        {
            var window = GetForegroundWindow();
            WINDOWINFO info = new WINDOWINFO();
            if (window != null && GetWindowInfo(window, ref info))
            {
                GetWindowThreadProcessId(window, out int procId);
                var proc = Process.GetProcessById(procId);

                Debug.WriteLine($"{nameof(proc.ProcessName)}: {proc.ProcessName}");

                // Don't resize windows from protected processes
                if (MainApplicationContext.protectedProcesses.Contains(proc.ProcessName))
                {
                    return;
                }

                var area = Screen.GetWorkingArea(new Point(info.rcClient.left, info.rcClient.top));
                var leftBorder = info.rcClient.left - info.rcWindow.left;
                var topBorder = info.rcClient.top - info.rcWindow.top;
                var rightBorder = info.rcWindow.right - info.rcClient.right;
                var bottomBorder = info.rcWindow.bottom - info.rcClient.bottom;

                ShowWindow(window, WindowShowStyle.SW_RESTORE);
                if (direction == Direction.Up)
                {
                    SetWindowPos(
                        window,
                        IntPtr.Zero,
                        area.Left - leftBorder,
                        area.Top - topBorder,
                        area.Width + leftBorder + rightBorder,
                        (area.Height / 2) + topBorder + bottomBorder,
                        SetWindowPosFlags.SWP_SHOWWINDOW);
                }
                else
                {
                    SetWindowPos(
                        window,
                        IntPtr.Zero,
                        area.Left - leftBorder,
                        area.Bottom - (area.Height / 2) - topBorder,
                        area.Width + leftBorder + rightBorder,
                        (area.Height / 2) + topBorder + bottomBorder,
                        SetWindowPosFlags.SWP_SHOWWINDOW);
                }
            }
        }

        private void Quit(object sender, EventArgs e)
        {
            this.trayIcon.Visible = false;
            Application.Exit();
        }
    }
}
