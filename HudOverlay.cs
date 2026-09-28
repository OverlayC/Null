using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace NullEx
{
    public static class HudOverlay
    {
        [DllImport("user32.dll")]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr after, int x, int y, int cx, int cy, uint flags);
        [DllImport("user32.dll", SetLastError = true)]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, IntPtr val);

        static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        const uint SWP_NOMOVE = 0x0002, SWP_NOSIZE = 0x0001, SWP_NOACTIVATE = 0x0010, SWP_SHOWWINDOW = 0x0040;

        static HudForm _form;
        static Thread _thread;
        static volatile bool _running;

        public static bool IsRunning => _running;

        public static void Start()
        {
            if (_running) return;
            _running = true;

            _thread = new Thread(() =>
            {
                _form = new HudForm();
                _form.Show();

                SetWindowLong(_form.Handle, -8, IntPtr.Zero);
                SetWindowPos(_form.Handle, HWND_TOPMOST, 0, 0, 0, 0,
                    SWP_NOMOVE | SWP_NOSIZE | SWP_NOACTIVATE | SWP_SHOWWINDOW);

                _form.FormClosed += (s, e) => { _running = false; };

                Application.Run(_form);
            })
            { IsBackground = true };
            _thread.SetApartmentState(ApartmentState.STA);
            _thread.Start();
        }

        public static void Stop()
        {
            if (!_running) return;
            _running = false;
            try { _form?.BeginInvoke(new Action(() => _form.Close())); } catch { }
        }

        public static void SetVisible(bool on)
        {
            try { _form?.BeginInvoke(new Action(() => { if (_form != null) _form.Visible = on; })); } catch { }
        }

        class HudForm : Form
        {
            Label _lblFps, _lblTime, _lblData;
            Panel _closeBtn;
            System.Windows.Forms.Timer _timer;

            long _lastTicks;
            int _frames;
            int _fps;

            [DllImport("user32.dll")]
            static extern bool ReleaseCapture();
            [DllImport("user32.dll")]
            static extern IntPtr SendMessage(IntPtr hWnd, int msg, int wParam, int lParam);

            public HudForm()
            {
                FormBorderStyle = FormBorderStyle.None;
                StartPosition = FormStartPosition.Manual;
                ClientSize = new Size(240, 28);
                BackColor = Theme.BgPanel;
                DoubleBuffered = true;
                ShowInTaskbar = false;
                TopMost = true;

                SetStyle(ControlStyles.OptimizedDoubleBuffer
                       | ControlStyles.AllPaintingInWmPaint
                       | ControlStyles.UserPaint, true);

                var wa = Screen.PrimaryScreen.WorkingArea;
                Location = new Point(wa.Right - Width - 16, wa.Top + 12);

                MouseDown += (s, e) =>
                {
                    if (e.Button == MouseButtons.Left)
                    {
                        ReleaseCapture();
                        SendMessage(Handle, 0xA1, 0x2, 0);
                    }
                };

                var font = new Font("Consolas", 8.5f, FontStyle.Bold);

                _lblFps = new Label
                {
                    Text = "FPS --",
                    ForeColor = Theme.TextMain,
                    BackColor = Color.Transparent,
                    Font = font,
                    AutoSize = false,
                    Left = 8,
                    Top = 6,
                    Width = 60,
                    Height = 18,
                    TextAlign = ContentAlignment.MiddleLeft
                };
                Controls.Add(_lblFps);

                _lblTime = new Label
                {
                    Text = "--:--:--",
                    ForeColor = Theme.TextDim,
                    BackColor = Color.Transparent,
                    Font = font,
                    AutoSize = false,
                    Left = 70,
                    Top = 6,
                    Width = 68,
                    Height = 18,
                    TextAlign = ContentAlignment.MiddleLeft
                };
                Controls.Add(_lblTime);

                _lblData = new Label
                {
                    Text = "n/a",
                    ForeColor = Theme.Accent,
                    BackColor = Color.Transparent,
                    Font = font,
                    AutoSize = false,
                    Left = 140,
                    Top = 6,
                    Width = 60,
                    Height = 18,
                    TextAlign = ContentAlignment.MiddleLeft
                };
                Controls.Add(_lblData);

                _closeBtn = new Panel
                {
                    Left = ClientSize.Width - 24,
                    Top = 4,
                    Width = 20,
                    Height = 20,
                    BackColor = Color.Transparent,
                    Cursor = Cursors.Hand
                };
                _closeBtn.Paint += (s, e) =>
                {
                    using (var fontX = new Font("Segoe UI", 9f))
                    using (var brush = new SolidBrush(Theme.TextMuted))
                        e.Graphics.DrawString("x", fontX, brush, 5, 1);
                };
                _closeBtn.MouseEnter += (s, e) => _closeBtn.Invalidate();
                _closeBtn.MouseLeave += (s, e) => _closeBtn.Invalidate();
                _closeBtn.Click += (s, e) =>
                {
                    if (MessageBox.Show("Close NullEx?", "NullEx",
                        MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
                    {
                        Application.Exit();
                    }
                };
                Controls.Add(_closeBtn);

                _lastTicks = Environment.TickCount64;

                _timer = new System.Windows.Forms.Timer { Interval = 500 };
                _timer.Tick += (s, e) => UpdateHud();
                _timer.Start();

                Paint += (s, e) =>
                {
                    using (var pen = new Pen(Theme.BorderCol))
                        e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);

                    using (var brush = new SolidBrush(Theme.Accent))
                        e.Graphics.FillRectangle(brush, 0, 0, 2, Height);
                };
            }

            void UpdateHud()
            {
                long now = Environment.TickCount64;
                long dt = now - _lastTicks;
                _lastTicks = now;

                _frames++;
                if (_frames % 2 == 0)
                {
                    _fps = (int)Math.Round(2000.0 / Math.Max(1, dt));
                    if (_fps > 240) _fps = 240;
                }

                _lblFps.Text = $"FPS {_fps:000}";
                _lblTime.Text = DateTime.Now.ToString("HH:mm:ss");

                try
                {
                    string name = Roblox.LocalName ?? "?";
                    if (name.Length > 8) name = name.Substring(0, 8);
                    long place = Mem.IsHeap(Roblox.DataModel)
                        ? Mem.ReadI64(Mem.Add(Roblox.DataModel, Offsets.DataModel.PlaceId))
                        : 0;
                    _lblData.Text = place > 0 ? $"#{place}" : name;
                }
                catch { _lblData.Text = "n/a"; }
            }

            protected override void OnFormClosing(FormClosingEventArgs e)
            {
                try { _timer?.Stop(); _timer?.Dispose(); } catch { }
                base.OnFormClosing(e);
            }
        }
    }
}