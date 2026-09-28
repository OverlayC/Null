using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace NullEx
{
    public class KeyBindRow : Panel
    {
        public static volatile bool AnyListening;

        public string Label;
        public Action<int> OnChanged;

        private int _key;
        private volatile bool _listening;
        private bool _hover;

        [DllImport("user32.dll")]
        static extern short GetAsyncKeyState(int vk);

        [DllImport("user32.dll")]
        static extern short GetKeyState(int vk);

        public KeyBindRow(string label, int initialKey, Action<int> onChange)
        {
            Label = label;
            _key = initialKey;
            OnChanged = onChange;

            Height = 22;
            BackColor = Color.Transparent;
            DoubleBuffered = true;
            Cursor = Cursors.Hand;

            SetStyle(ControlStyles.OptimizedDoubleBuffer
                   | ControlStyles.AllPaintingInWmPaint
                   | ControlStyles.UserPaint, true);

            Click += (s, e) => BeginListen();
            MouseDown += (s, e) =>
            {
                if (e.Button == MouseButtons.Right)
                {
                    _key = (int)VirtualKeys.None;
                    OnChanged?.Invoke(0);
                    Invalidate();
                }
            };
            MouseEnter += (s, e) => { _hover = true; Invalidate(); };
            MouseLeave += (s, e) => { _hover = false; Invalidate(); };
        }

        public int Key
        {
            get => _key;
            set { _key = value; Invalidate(); }
        }

        void BeginListen()
        {
            if (_listening) return;
            _listening = true;
            AnyListening = true;
            Invalidate();

            var t = new Thread(() =>
            {
                try
                {
                    while (AnyKeyDown()) Thread.Sleep(15);

                    while (_listening)
                    {
                        int vk = WaitForKeyDown();
                        if (vk == 0) { Thread.Sleep(15); continue; }

                        if (vk == (int)VirtualKeys.Esc)
                        {
                            _listening = false;
                            AnyListening = false;
                            SafeInvalidate();
                            return;
                        }

                        _key = vk;
                        _listening = false;
                        AnyListening = false;
                        OnChanged?.Invoke(vk);
                        SafeInvalidate();
                        return;
                    }
                }
                catch
                {
                    _listening = false;
                    AnyListening = false;
                    SafeInvalidate();
                }
            })
            { IsBackground = true };

            t.Start();
        }

        void SafeInvalidate()
        {
            try { BeginInvoke(new Action(Invalidate)); } catch { }
        }

        static bool AnyKeyDown()
        {
            for (int vk = 1; vk <= 0xFE; vk++)
            {
                if ((GetAsyncKeyState(vk) & 0x8000) != 0) return true;
            }
            return false;
        }

        static int WaitForKeyDown()
        {
            for (int vk = 1; vk <= 0xFE; vk++)
            {
                if (vk == (int)VirtualKeys.Esc) continue;
                if ((GetAsyncKeyState(vk) & 0x8000) != 0) return vk;
            }
            return 0;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;

            var bg = _listening ? Theme.AccentSoft : (_hover ? Theme.BgRowHv : Theme.BgRow);
            using (var brush = new SolidBrush(bg))
                g.FillRectangle(brush, ClientRectangle);
            using (var pen = new Pen(_listening ? Theme.Accent : Theme.BorderCol))
                g.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);

            using (var font = new Font("Segoe UI", 8.5f))
            using (var brush = new SolidBrush(Theme.TextDim))
                g.DrawString(Label, font, brush, 10, 3);

            string keyText = _listening
                ? "press key..."
                : ((VirtualKeys)_key).ToDisplayString();

            using (var font = new Font("Consolas", 8.5f, FontStyle.Bold))
            using (var brush = new SolidBrush(_listening ? Theme.Accent : Theme.TextMain))
            {
                var sz = g.MeasureString(keyText, font);
                g.DrawString(keyText, font, brush, Width - sz.Width - 10, 3);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (_listening) AnyListening = false;
            _listening = false;
            base.Dispose(disposing);
        }
    }
}