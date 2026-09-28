using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace NullEx
{
    public static class MenuKeys
    {
        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vk);

        private static Thread _thread;
        private static volatile bool _running;

        public static volatile int ToggleKey = (int)VirtualKeys.Insert;

        public static void Install()
        {
            if (_running) return;
            _running = true;

            _thread = new Thread(Pump) { IsBackground = true, Name = "MenuKeys" };
            _thread.Start();

            MainForm.Log($"MenuKeys: toggle bound to {(VirtualKeys)ToggleKey}");
        }

        public static void Uninstall()
        {
            _running = false;
        }

        private static void Pump()
        {
            bool wasDown = false;

            while (_running)
            {
                int vk = ToggleKey;

                if (vk == 0)
                {
                    wasDown = false;
                    Thread.Sleep(30);
                    continue;
                }

                bool pressed = (GetAsyncKeyState(vk) & 0x8000) != 0;

                if (pressed && !wasDown)
                {
                    wasDown = true;

                    if (!KeyBindRow.AnyListening)
                    {
                        try
                        {
                            var form = MainForm.Instance;
                            if (form != null && form.IsHandleCreated && !form.IsDisposed)
                            {
                                form.BeginInvoke(new Action(() =>
                                {
                                    try
                                    {
                                        if (form.Visible) form.BeginHideExternal();
                                        else form.BeginShowExternal();
                                    }
                                    catch (Exception ex)
                                    {
                                        MainForm.Log("Menu toggle failed: " + ex.Message);
                                    }
                                }));
                            }
                        }
                        catch { }
                    }
                }
                else if (!pressed) wasDown = false;

                Thread.Sleep(25);
            }
        }
    }
}