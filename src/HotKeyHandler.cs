using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using static PInvoke.User32;

namespace Snappy
{
    public class HotKeyHandler
    {
        public const uint MOD_WIN = 0x0008;
        public const uint MOD_ALT = 0x0001;
        public const uint MOD_NOREPEAT = 0x4000;

        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, VirtualKey vk);

        private Dictionary<int, Action> callbacks = new Dictionary<int, Action>();

        public HotKeyHandler((uint, VirtualKey, Action)[] registrations)
        {
            Task.Run(() =>
            {
                var id = 0;
                foreach (var registration in registrations)
                {
                    var (mods, key, callback) = registration;

                    RegisterHotKey(IntPtr.Zero, id, mods, key);
                    this.callbacks.Add(id, callback);
                    id++;
                }

                unsafe
                {
                    MSG msg = new MSG();
                    while (GetMessage(&msg, IntPtr.Zero, 0, 0) != 0)
                    {
                        this.callbacks.TryGetValue(msg.wParam.ToInt32(), out Action callback);

                        try
                        {
                            callback();
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine(ex);
                        }
                    }
                }
            });
        }

    }
}
