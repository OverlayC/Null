using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace NullEx
{
    public static class Overlay
    {
        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr SetParent(IntPtr child, IntPtr parent);

        [DllImport("user32.dll", SetLastError = true, EntryPoint = "SetWindowLongPtr")]
        static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("user32.dll", SetLastError = true, EntryPoint = "SetWindowLong")]
        static extern int SetWindowLong32(IntPtr hWnd, int nIndex, int dwNewLong);

        static IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
        {
            if (IntPtr.Size == 8)
                return SetWindowLongPtr64(hWnd, nIndex, dwNewLong);
            return new IntPtr(SetWindowLong32(hWnd, nIndex, dwNewLong.ToInt32()));
        }

        [DllImport("user32.dll")]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr after, int x, int y, int cx, int cy, uint flags);

        [DllImport("user32.dll")]
        static extern bool GetClientRect(IntPtr hWnd, out RECT r);

        [DllImport("user32.dll")]
        static extern bool ClientToScreen(IntPtr hWnd, ref POINT p);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT { public int Left, Top, Right, Bottom; }
        [StructLayout(LayoutKind.Sequential)]
        public struct POINT { public int X, Y; }

        const int GWL_STYLE = -16;
        const int GWL_EXSTYLE = -20;

        const int WS_CHILD = 0x40000000;
        const int WS_VISIBLE = 0x10000000;
        const int WS_CLIPSIBLINGS = 0x04000000;

        const int WS_EX_TOPMOST = 0x00000008;
        const int WS_EX_TRANSPARENT = 0x00000020;
        const int WS_EX_TOOLWINDOW = 0x00000080;
        const int WS_EX_LAYERED = 0x00080000;
        const int WS_EX_NOACTIVATE = 0x08000000;

        static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        const uint SWP_NOMOVE = 0x0002;
        const uint SWP_NOSIZE = 0x0001;
        const uint SWP_NOACTIVATE = 0x0010;
        const uint SWP_SHOWWINDOW = 0x0040;
        const uint SWP_FRAMECHANGED = 0x0020;

        static OverlayForm _form;
        static Thread _thread;
        static volatile bool _running;

        public static bool IsRunning => _running;

        public static void Start()
        {
            if (_running) return;
            _running = true;

            _thread = new Thread(ThreadProc) { IsBackground = true };
            _thread.SetApartmentState(ApartmentState.STA);
            _thread.Priority = ThreadPriority.AboveNormal;
            _thread.Start();
        }

        public static void Stop()
        {
            if (!_running) return;
            _running = false;
            try { _form?.BeginInvoke(new Action(() => _form.Close())); } catch { }
        }

        static void ThreadProc()
        {
            _form = new OverlayForm();

            _form.Show();

            AttachToRoblox(_form);

            var projector = new Thread(ProjectorLoop) { IsBackground = true };
            projector.Priority = ThreadPriority.AboveNormal;
            projector.Start();

            var paintTimer = new System.Windows.Forms.Timer { Interval = 4 };
            paintTimer.Tick += (s, e) =>
            {
                if (!_running) { paintTimer.Stop(); _form.Close(); return; }
                FitToRoblox(_form);
                _form.Invalidate();
            };
            paintTimer.Start();

            Application.Run(_form);
            _running = false;
        }

        static void AttachToRoblox(OverlayForm form)
        {
            if (form == null || Mem.RobloxHwnd == IntPtr.Zero) return;

            IntPtr hwnd = form.Handle;
            IntPtr parent = Mem.RobloxHwnd;

            IntPtr style = (IntPtr)(WS_CHILD | WS_VISIBLE | WS_CLIPSIBLINGS);
            SetWindowLongPtr(hwnd, GWL_STYLE, style);

            IntPtr exStyle = (IntPtr)(WS_EX_TOPMOST | WS_EX_TRANSPARENT
                                     | WS_EX_TOOLWINDOW | WS_EX_LAYERED
                                     | WS_EX_NOACTIVATE);
            SetWindowLongPtr(hwnd, GWL_EXSTYLE, exStyle);

            SetParent(hwnd, parent);

            FitToRoblox(form);

            SetWindowPos(hwnd, IntPtr.Zero, 0, 0, 0, 0,
                SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE
                | SWP_FRAMECHANGED | SWP_SHOWWINDOW);

            SetWindowPos(hwnd, HWND_TOPMOST, 0, 0, 0, 0,
                SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE | SWP_SHOWWINDOW);
        }

        static void FitToRoblox(OverlayForm form)
        {
            if (form == null || Mem.RobloxHwnd == IntPtr.Zero) return;

            if (GetClientRect(Mem.RobloxHwnd, out RECT rc))
            {
                int w = rc.Right - rc.Left;
                int h = rc.Bottom - rc.Top;
                if (w <= 0 || h <= 0) return;

                if (form.Left != 0 || form.Top != 0 ||
                    form.Width != w || form.Height != h)
                {
                    form.SetBounds(0, 0, w, h);
                }
            }
        }

        static void ProjectorLoop()
        {
            while (_running)
            {
                try
                {
                    var items = BuildItems();
                    var form = _form;
                    if (form != null) form.Items = items;
                }
                catch { }
                Thread.Sleep(2);
            }
        }

        static List<DrawItem> BuildItems()
        {
            Rectangle viewport = GetRobloxViewport();
            return EspBuilder.BuildEspItems(viewport);
        }

        public static Rectangle GetRobloxViewport()
        {
            IntPtr hwnd = Mem.RobloxHwnd;
            if (hwnd != IntPtr.Zero)
            {
                if (GetClientRect(hwnd, out RECT rc))
                {
                    var tl = new POINT { X = rc.Left, Y = rc.Top };
                    var br = new POINT { X = rc.Right, Y = rc.Bottom };
                    ClientToScreen(hwnd, ref tl);
                    ClientToScreen(hwnd, ref br);
                    int w = br.X - tl.X;
                    int h = br.Y - tl.Y;
                    if (w > 0 && h > 0)
                        return Rectangle.FromLTRB(tl.X, tl.Y, br.X, br.Y);
                }
            }
            return Screen.PrimaryScreen.Bounds;
        }
    }

    class OverlayForm : Form
    {
        public List<DrawItem> Items = new List<DrawItem>();

        Pen _outerPen;
        Pen _boxPen;
        SolidBrush _shadowBrush;
        SolidBrush _textBrush;
        Font _font;
        int _lastThickness = -1;
        int _lastR = -1, _lastG = -1, _lastB = -1;

        public OverlayForm()
        {
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.Manual;
            Bounds = new Rectangle(0, 0, 100, 100);
            TopMost = true;
            ShowInTaskbar = false;
            BackColor = Color.Black;
            TransparencyKey = Color.Black;
            DoubleBuffered = true;

            SetStyle(ControlStyles.OptimizedDoubleBuffer
                   | ControlStyles.AllPaintingInWmPaint
                   | ControlStyles.UserPaint, true);
        }

        protected override bool ShowWithoutActivation => true;

        protected override CreateParams CreateParams
        {
            get
            {
                var cp = base.CreateParams;
                cp.ExStyle |= WS_EX_TRANSPARENT
                            | WS_EX_TOOLWINDOW
                            | WS_EX_LAYERED
                            | WS_EX_NOACTIVATE;
                return cp;
            }
        }

        const int WS_EX_TRANSPARENT = 0x00000020;
        const int WS_EX_TOOLWINDOW = 0x00000080;
        const int WS_EX_LAYERED = 0x00080000;
        const int WS_EX_NOACTIVATE = 0x08000000;

        void EnsureResources(int thickness, Color color)
        {
            if (_outerPen == null || _lastThickness != thickness)
            {
                _outerPen?.Dispose();
                _boxPen?.Dispose();
                _outerPen = new Pen(Color.FromArgb(255, 0, 0, 0), thickness + 3f);
                _boxPen = new Pen(color, thickness);
                _lastThickness = thickness;
            }

            if (_lastR != color.R || _lastG != color.G || _lastB != color.B)
            {
                _boxPen?.Dispose();
                _textBrush?.Dispose();
                _boxPen = new Pen(color, thickness);
                _textBrush = new SolidBrush(color);
                _lastR = color.R;
                _lastG = color.G;
                _lastB = color.B;
            }

            if (_shadowBrush == null)
                _shadowBrush = new SolidBrush(Color.FromArgb(180, 0, 0, 0));

            if (_font == null)
                _font = new Font("Consolas", 9f, FontStyle.Bold);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = SmoothingMode.None;
            g.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

            var esp = FeatureManager.Get<EspFeature>();
            if (esp == null) return;

            int thickness = Math.Max(1, Math.Min(5, esp.BoxThickness));
            Color boxColor = Color.FromArgb(255, esp.ColorR, esp.ColorG, esp.ColorB);

            EnsureResources(thickness, boxColor);

            var items = Items;

            bool showBox = esp.ShowBox;
            bool showName = esp.ShowName;
            bool showHealth = esp.ShowHealth;
            bool showDistance = esp.ShowDistance;

            for (int i = 0; i < items.Count; i++)
            {
                var it = items[i];

                try
                {
                    if (showBox)
                    {
                        g.DrawRectangle(_outerPen, it.X - 1, it.Y - 1, it.W + 2, it.H + 2);
                        g.DrawRectangle(_boxPen, it.X, it.Y, it.W, it.H);
                    }

                    string label = null;
                    if (showName && !string.IsNullOrEmpty(it.Name)) label = it.Name;
                    if (showHealth && it.MaxHp > 0)
                        label = (label ?? "") + "  " + it.Hp.ToString("F0") + "/" + it.MaxHp.ToString("F0");
                    if (showDistance && it.Distance > 0)
                        label = (label ?? "") + "  [" + it.Distance.ToString("F0") + "m]";

                    if (!string.IsNullOrEmpty(label))
                    {
                        var sz = g.MeasureString(label, _font);
                        float lx = it.X + (it.W - sz.Width) / 2f;
                        float ly = it.Y - sz.Height - 1;
                        g.DrawString(label, _font, _shadowBrush, lx + 1, ly + 1);
                        g.DrawString(label, _font, _textBrush, lx, ly);
                    }
                }
                catch { }
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _outerPen?.Dispose();
                _boxPen?.Dispose();
                _shadowBrush?.Dispose();
                _textBrush?.Dispose();
                _font?.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    public struct DrawItem
    {
        public int X, Y, W, H;
        public string Name;
        public float Hp, MaxHp, Distance;
    }
}