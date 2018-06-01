using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows.Forms;
using Snappy.Properties;
using static PInvoke.User32;

namespace Snappy
{
    public class MainApplicationContext : ApplicationContext
    {
        private const uint MOD_WIN = 0x0008;
        private const uint MOD_ALT = 0x0001;
        private const uint MOD_NOREPEAT = 0x4000;

        private NotifyIcon trayIcon;

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, VirtualKey vk);
        
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
                    new MenuItem(Resources.Actions_Quit, this.Quit)
                })
            };

            Task.Run(() =>
            {
                RegisterHotKey(IntPtr.Zero, 0, MOD_WIN | MOD_ALT | MOD_NOREPEAT, VirtualKey.VK_UP);
                RegisterHotKey(IntPtr.Zero, 1, MOD_WIN | MOD_ALT | MOD_NOREPEAT, VirtualKey.VK_DOWN);

                unsafe
                {
                    MSG msg = new MSG();
                    while (GetMessage(&msg, IntPtr.Zero, 0, 0) != 0)
                    {
                        var hWnd = GetForegroundWindow();
                        WINDOWINFO info = new WINDOWINFO();
                        var direction = msg.wParam.ToInt32() == 0x0 ? Direction.Up : Direction.Down;
                        if (hWnd != null && GetWindowInfo(hWnd, ref info))
                        {
                            var area = Screen.GetWorkingArea(new Point(info.rcClient.left, info.rcClient.top));
                            var leftBorder = info.rcClient.left - info.rcWindow.left;
                            var topBorder = info.rcClient.top - info.rcWindow.top;
                            var rightBorder = info.rcWindow.right - info.rcClient.right;
                            var bottomBorder = info.rcWindow.bottom - info.rcClient.bottom;

                            ShowWindow(hWnd, WindowShowStyle.SW_SHOWNORMAL);
                            if (direction == Direction.Up)
                            {
                                SetWindowPos(
                                    hWnd,
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
                                    hWnd,
                                    IntPtr.Zero,
                                    area.Left - leftBorder,
                                    area.Bottom - (area.Height / 2) - topBorder,
                                    area.Width + leftBorder + rightBorder,
                                    (area.Height / 2) + topBorder + bottomBorder,
                                    SetWindowPosFlags.SWP_SHOWWINDOW);
                            }
                        }
                    }
                }
            });
        }

        private void Quit(object sender, EventArgs e)
        {
            this.trayIcon.Visible = false;
            Application.Exit();
        }
    }
}
