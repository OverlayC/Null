using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading;
using System.Windows.Forms;

namespace NullEx
{
    public static class LoadingScreen
    {
        static LoadingForm _form;
        static Thread _thread;
        static volatile bool _running;

        public static void Start()
        {
            if (_running) return;
            _running = true;

            _thread = new Thread(() =>
            {
                _form = new LoadingForm();
                _form.FormClosed += (s, e) => { _running = false; };
                Application.Run(_form);
            })
            { IsBackground = true };
            _thread.SetApartmentState(ApartmentState.STA);
            _thread.Start();

            for (int i = 0; i < 50 && _form == null; i++)
                Thread.Sleep(10);
        }

        public static void Stop()
        {
            if (!_running) return;
            _running = false;
            try { _form?.BeginInvoke(new Action(() => _form.Close())); } catch { }
        }

        public static void SetStatus(string text, int percent)
        {
            try
            {
                _form?.BeginInvoke(new Action(() => _form?.SetStatus(text, percent)));
            }
            catch { }
        }

        class LoadingForm : Form
        {
            Label _status;
            ProgressBar _progress;
            System.Windows.Forms.Timer _spin;
            int _spinAngle;
            int _targetProgress;
            int _currentProgress;
            string _statusText = "Initializing...";

            public LoadingForm()
            {
                FormBorderStyle = FormBorderStyle.None;
                StartPosition = FormStartPosition.CenterScreen;
                ClientSize = new Size(420, 180);
                BackColor = Theme.BgOuter;
                DoubleBuffered = true;
                ShowInTaskbar = false;
                TopMost = true;

                SetStyle(ControlStyles.OptimizedDoubleBuffer
                       | ControlStyles.AllPaintingInWmPaint
                       | ControlStyles.UserPaint, true);

                _status = new Label
                {
                    Text = "Initializing...",
                    ForeColor = Theme.TextDim,
                    BackColor = Color.Transparent,
                    Font = new Font("Segoe UI", 9f),
                    AutoSize = false,
                    TextAlign = ContentAlignment.MiddleCenter,
                    Left = 20,
                    Top = 110,
                    Width = 380,
                    Height = 20
                };
                Controls.Add(_status);

                _progress = new ProgressBar
                {
                    Style = ProgressBarStyle.Continuous,
                    Minimum = 0,
                    Maximum = 100,
                    Value = 0,
                    Left = 40,
                    Top = 140,
                    Width = 340,
                    Height = 8,
                    ForeColor = Theme.Accent
                };
                Controls.Add(_progress);

                _spin = new System.Windows.Forms.Timer { Interval = 16 };
                _spin.Tick += (s, e) =>
                {
                    _spinAngle = (_spinAngle + 6) % 360;
                    if (_currentProgress < _targetProgress)
                        _currentProgress = Math.Min(_targetProgress, _currentProgress + 2);
                    if (_progress.Value != _currentProgress)
                        _progress.Value = _currentProgress;
                    if (_status.Text != _statusText)
                        _status.Text = _statusText;
                    Invalidate();
                };
                _spin.Start();
            }

            public void SetStatus(string text, int percent)
            {
                _statusText = text ?? "";
                _targetProgress = Math.Max(0, Math.Min(100, percent));
            }

            protected override void OnFormClosing(FormClosingEventArgs e)
            {
                try { _spin?.Stop(); _spin?.Dispose(); } catch { }
                base.OnFormClosing(e);
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                using (var pen = new Pen(Theme.BorderBright, 1))
                using (var path = RoundedRect(new Rectangle(0, 0, Width - 1, Height - 1), 8))
                    g.DrawPath(pen, path);

                using (var font = new Font("Segoe UI Semibold", 14f, FontStyle.Bold))
                using (var brush = new SolidBrush(Theme.Accent))
                {
                    var sz = g.MeasureString("NullEx", font);
                    g.DrawString("NullEx", font, brush, (Width - sz.Width) / 2, 30);
                }

                using (var font = new Font("Segoe UI", 8.5f))
                using (var brush = new SolidBrush(Theme.TextMuted))
                {
                    string sub = "v1.0  —  starting up";
                    var sz = g.MeasureString(sub, font);
                    g.DrawString(sub, font, brush, (Width - sz.Width) / 2, 62);
                }

                int cx = Width / 2;
                int cy = 88;
                using (var pen = new Pen(Theme.Accent, 2))
                {
                    pen.StartCap = LineCap.Round;
                    pen.EndCap = LineCap.Round;
                    g.DrawArc(pen, cx - 10, cy - 10, 20, 20, _spinAngle, 270);
                }
            }

            static GraphicsPath RoundedRect(Rectangle bounds, int radius)
            {
                int d = radius * 2;
                var path = new GraphicsPath();
                if (d <= 0) { path.AddRectangle(bounds); return path; }
                path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
                path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
                path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
                path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
                path.CloseFigure();
                return path;
            }
        }
    }
}