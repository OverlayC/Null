using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using System.Windows.Forms;

using Timer = System.Windows.Forms.Timer;

namespace NullEx
{
    class MainForm : Form
    {
        public static MainForm Instance;
        public static TextBox LogBox;

        Panel header;
        Label lblTitle, lblSubtitle;
        Button btnMinimize, btnClose;
        Panel tabBar;
        Button tabSettings, tabInfo;
        Panel bodyHost;
        Panel pageSettings, pageInfo, activePage;

        ScrollContainer leftColViewport;
        Panel leftColContent;
        Panel rightCol;

        readonly Dictionary<Feature, SubToggle> _featureToggles = new Dictionary<Feature, SubToggle>();
        readonly Dictionary<Feature, Dictionary<string, Control>> _featureSettingControls
            = new Dictionary<Feature, Dictionary<string, Control>>();

        Label lblAttachStatus;
        Button btnAttach;

        TextBox txtPosX, txtPosY;
        ComboBox cboTheme;

        bool g_Animating = false;
        bool g_AnimOpening = false;
        int g_AnimStep = 0;
        const int AnimSteps = 10;
        const int AnimIntervalMs = 14;
        Point g_AnimTargetPos;
        Point g_AnimStartPos;

        bool isDragging = false;
        Point dragStart;
        const int CornerRadius = 6;

        [DllImport("user32.dll")]
        static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);
        [DllImport("user32.dll")]
        static extern IntPtr GetForegroundWindow();
        [StructLayout(LayoutKind.Sequential)]
        public struct RECT { public int Left, Top, Right, Bottom; }
        class ScrollIndicator : Panel
        {
            public int ScrollPos;
            public int ContentHeight;
            public int ViewHeight;

            public ScrollIndicator()
            {
                Width = 4;
                BackColor = Color.Transparent;
                DoubleBuffered = true;
                SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);
                Visible = false;
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                if (ContentHeight <= ViewHeight) return;

                float frac = ViewHeight / (float)ContentHeight;
                int thumbH = Math.Max(20, (int)(Height * frac));

                float scrollFrac = ScrollPos / (float)Math.Max(1, ContentHeight - ViewHeight);
                int thumbY = (int)(scrollFrac * (Height - thumbH));

                using (var brush = new SolidBrush(Color.FromArgb(160, Theme.Accent)))
                using (var path = RoundedRect(new Rectangle(0, thumbY, Width, thumbH), Width / 2))
                    e.Graphics.FillPath(brush, path);
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
        class ScrollContainer : Panel
        {
            public Panel Content;
            public ScrollIndicator Indicator;

            int _scrollPos;
            int _fadeTimer;
            Timer _hideTimer;

            public ScrollContainer()
            {
                DoubleBuffered = true;
                BackColor = Color.Transparent;
                SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);

                Content = new Panel
                {
                    Left = 0,
                    Top = 0,
                    BackColor = Color.Transparent
                };
                Controls.Add(Content);

                Indicator = new ScrollIndicator();
                Controls.Add(Indicator);

                MouseWheel += (s, e) => Scroll(-e.Delta / 120 * 40);
                Content.MouseWheel += (s, e) => Scroll(-e.Delta / 120 * 40);

                Content.ControlAdded += (s, e) => HookWheel(e.Control);

                _hideTimer = new Timer { Interval = 50 };
                _hideTimer.Tick += (s, e) =>
                {
                    if (!Indicator.Visible) { _hideTimer.Stop(); return; }
                    _fadeTimer++;
                    if (_fadeTimer > 30)
                    {
                        Indicator.Visible = false;
                        _fadeTimer = 0;
                        _hideTimer.Stop();
                    }
                };
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                using (var brush = new SolidBrush(Theme.BgPanel))
                    e.Graphics.FillRectangle(brush, ClientRectangle);
                base.OnPaint(e);
            }

            protected override void OnControlAdded(ControlEventArgs e)
            {
                base.OnControlAdded(e);
                HookWheel(e.Control);
            }

            void HookWheel(Control c)
            {
                if (c is ComboBox)
                {
                    c.MouseWheel += (s, e) => { ((HandledMouseEventArgs)e).Handled = true; };
                }
                else
                {
                    c.MouseWheel += (s, e) => Scroll(-e.Delta / 120 * 40);
                }

                foreach (Control child in c.Controls)
                    HookWheel(child);
            }

            public void RecalculateScrollRange()
            {
                Content.Height = Math.Max(Content.Height, GetMaxChildBottom() + 20);

                Indicator.Left = ClientSize.Width - 6;
                Indicator.Top = 0;
                Indicator.Height = ClientSize.Height;
                Indicator.ContentHeight = Content.Height;
                Indicator.ViewHeight = ClientSize.Height;

                int maxScroll = Math.Max(0, Content.Height - ClientSize.Height);
                if (_scrollPos > maxScroll)
                {
                    _scrollPos = maxScroll;
                    Content.Top = -_scrollPos;
                }

                Indicator.Invalidate();
            }

            int GetMaxChildBottom()
            {
                int maxBottom = 0;
                foreach (Control c in Content.Controls)
                {
                    if (!c.Visible) continue;
                    int bottom = c.Top + c.Height;
                    if (bottom > maxBottom) maxBottom = bottom;
                }
                return maxBottom;
            }

            public void Scroll(int delta)
            {
                int maxScroll = Math.Max(0, Content.Height - ClientSize.Height);
                int newPos = Math.Max(0, Math.Min(maxScroll, _scrollPos + delta));
                if (newPos == _scrollPos) return;
                _scrollPos = newPos;

                Content.Top = -_scrollPos;

                Indicator.ScrollPos = _scrollPos;
                Indicator.ContentHeight = Content.Height;
                Indicator.ViewHeight = ClientSize.Height;
                Indicator.Left = ClientSize.Width - 6;
                Indicator.Top = 0;
                Indicator.Height = ClientSize.Height;

                if (maxScroll > 0)
                {
                    Indicator.Visible = true;
                    _fadeTimer = 0;
                    _hideTimer.Stop();
                    _hideTimer.Start();
                }

                Indicator.Invalidate();
            }

            protected override void OnResize(EventArgs eventargs)
            {
                base.OnResize(eventargs);
                Indicator.Left = ClientSize.Width - 6;
                Indicator.Height = ClientSize.Height;
                int maxScroll = Math.Max(0, Content.Height - ClientSize.Height);
                if (_scrollPos > maxScroll) Scroll(maxScroll - _scrollPos);
            }
        }

        class SubToggle : Panel
        {
            public bool Value;
            public Action<bool> OnChanged;
            public string Label;

            public SubToggle(string label, bool initial, Action<bool> onChange)
            {
                Label = label;
                Value = initial;
                OnChanged = onChange;
                Height = 22;
                BackColor = Color.Transparent;
                DoubleBuffered = true;
                Cursor = Cursors.Hand;
                Click += (s, e) => { Value = !Value; OnChanged?.Invoke(Value); Invalidate(); };
                MouseEnter += (s, e) => Invalidate();
                MouseLeave += (s, e) => Invalidate();
            }

            public void SetValueSilent(bool v) { Value = v; Invalidate(); }

            protected override void OnPaint(PaintEventArgs e)
            {
                var g = e.Graphics;
                bool hover = ClientRectangle.Contains(PointToClient(Cursor.Position));

                using (var bg = new SolidBrush(hover ? Theme.BgRowHv : Theme.BgRow))
                    g.FillRectangle(bg, ClientRectangle);
                using (var pen = new Pen(Theme.BorderCol))
                    g.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);

                if (Value)
                {
                    using (var brush = new SolidBrush(Theme.Accent))
                        g.FillRectangle(brush, 0, 0, 3, Height);
                }

                using (var font = new Font("Segoe UI", 8.5f))
                using (var brush = new SolidBrush(Value ? Theme.Accent : Theme.TextDim))
                    g.DrawString(Label, font, brush, 10, 3);

                using (var font = new Font("Segoe UI Semibold", 8f, FontStyle.Bold))
                using (var brush = new SolidBrush(Value ? Theme.Accent : Theme.TextMuted))
                {
                    var s = Value ? "ON" : "OFF";
                    var sz = g.MeasureString(s, font);
                    g.DrawString(s, font, brush, Width - sz.Width - 10, 4);
                }
            }
        }

        class SubSlider : Panel
        {
            public int Value, Min, Max;
            public Action<int> OnChanged;
            public string Label;

            public SubSlider(string label, int min, int max, int initial, Action<int> onChange)
            {
                Label = label;
                Min = min; Max = max;
                Value = Math.Max(min, Math.Min(max, initial));
                OnChanged = onChange;
                Height = 22;
                BackColor = Color.Transparent;
                DoubleBuffered = true;
                Cursor = Cursors.Hand;

                MouseDown += (s, e) => Update(e.X);
                MouseMove += (s, e) => { if (e.Button == MouseButtons.Left) Update(e.X); };
                MouseEnter += (s, e) => Invalidate();
                MouseLeave += (s, e) => Invalidate();
            }

            void Update(int mx)
            {
                int trackX = Width / 2 + 14;
                int trackW = Width - trackX - 10;
                float pct = Math.Max(0f, Math.Min(1f, (mx - trackX) / (float)trackW));
                int nv = Min + (int)Math.Round(pct * (Max - Min));
                if (nv != Value) { Value = nv; OnChanged?.Invoke(Value); Invalidate(); }
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                var g = e.Graphics;
                g.SmoothingMode = SmoothingMode.AntiAlias;

                bool hover = ClientRectangle.Contains(PointToClient(Cursor.Position));

                using (var bg = new SolidBrush(hover ? Theme.BgRowHv : Theme.BgRow))
                    g.FillRectangle(bg, ClientRectangle);
                using (var pen = new Pen(Theme.BorderCol))
                    g.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);

                using (var font = new Font("Segoe UI", 8.5f))
                using (var brush = new SolidBrush(Theme.TextDim))
                    g.DrawString(Label, font, brush, 10, 3);

                using (var font = new Font("Consolas", 8.5f, FontStyle.Bold))
                using (var brush = new SolidBrush(Theme.Accent))
                {
                    string vs = Value.ToString();
                    var sz = g.MeasureString(vs, font);
                    g.DrawString(vs, font, brush, Width / 2 + 8 - sz.Width, 3);
                }

                int trackX = Width / 2 + 14;
                int trackW = Width - trackX - 10;
                int trackY = Height / 2;

                using (var brush = new SolidBrush(Theme.BorderCol))
                    g.FillRectangle(brush, trackX, trackY, trackW, 2);

                float pct = (Value - Min) / (float)(Max - Min);
                int fillW = (int)(trackW * pct);

                using (var brush = new SolidBrush(Theme.Accent))
                    g.FillRectangle(brush, trackX, trackY, fillW, 2);

                using (var brush = new SolidBrush(Color.White))
                    g.FillEllipse(brush, trackX + fillW - 3, trackY - 2, 6, 6);
            }
        }

        class RowColor : Panel
        {
            public Color Value;
            public Action<Color> OnChanged;

            public RowColor(Color initial, Action<Color> onChange)
            {
                Value = initial;
                OnChanged = onChange;
                Height = 22;
                BackColor = Color.Transparent;
                DoubleBuffered = true;
                Cursor = Cursors.Hand;
                Click += (s, e) =>
                {
                    using (var cd = new ColorDialog())
                    {
                        cd.Color = Value;
                        if (cd.ShowDialog() == DialogResult.OK)
                        {
                            Value = cd.Color;
                            OnChanged?.Invoke(Value);
                            Invalidate();
                        }
                    }
                };
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                using (var bg = new SolidBrush(Theme.BgRow))
                    e.Graphics.FillRectangle(bg, ClientRectangle);
                using (var pen = new Pen(Theme.BorderCol))
                    e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);

                using (var font = new Font("Segoe UI", 8.5f))
                using (var brush = new SolidBrush(Theme.TextDim))
                    e.Graphics.DrawString("Color", font, brush, 10, 3);

                var sw = new Rectangle(Width - 46, 4, 34, 14);
                using (var brush = new SolidBrush(Value))
                    e.Graphics.FillRectangle(brush, sw);
                using (var pen = new Pen(Theme.BorderBright))
                    e.Graphics.DrawRectangle(pen, sw);
            }
        }

        public MainForm()
        {
            Instance = this;

            Theme.Apply(Theme.Current);

            Text = "NullEx";
            ClientSize = new Size(880, 520);
            FormBorderStyle = FormBorderStyle.None;
            MaximizeBox = false;
            StartPosition = FormStartPosition.Manual;
            BackColor = Theme.BgOuter;
            ForeColor = Theme.TextMain;
            Font = new Font("Segoe UI", 8.5f);
            DoubleBuffered = true;
            SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint | ControlStyles.UserPaint, true);

            ApplyRoundedRegion();
            Resize += (s, e) => ApplyRoundedRegion();

            Theme.PaletteChanged += OnPaletteChanged;

            header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 30,
                BackColor = Theme.BgPanel,
                Tag = "BgPanel"
            };
            header.MouseDown += Header_MouseDown;
            header.MouseMove += Header_MouseMove;
            header.MouseUp += Header_MouseUp;

            lblTitle = new Label
            {
                Text = "NullEx.wtf",
                ForeColor = Theme.Accent,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI Semibold", 10f, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(10, 7)
            };
            lblTitle.MouseDown += Header_MouseDown;
            lblTitle.MouseMove += Header_MouseMove;
            lblTitle.MouseUp += Header_MouseUp;
            header.Controls.Add(lblTitle);

            lblSubtitle = new Label
            {
                Text = "v1.0",
                ForeColor = Theme.TextMuted,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 8f),
                AutoSize = true,
                Location = new Point(86, 8)
            };
            lblSubtitle.MouseDown += Header_MouseDown;
            lblSubtitle.MouseMove += Header_MouseMove;
            lblSubtitle.MouseUp += Header_MouseUp;
            header.Controls.Add(lblSubtitle);

            btnMinimize = CreateTitleButton("−", ClientSize.Width - 50, 6, 20, 20);
            btnMinimize.Click += (s, e) => WindowState = FormWindowState.Minimized;
            header.Controls.Add(btnMinimize);

            btnClose = CreateTitleButton("x", ClientSize.Width - 26, 6, 20, 20);
            btnClose.Click += (s, e) => Close();
            header.Controls.Add(btnClose);

            Controls.Add(header);


            tabBar = new Panel
            {
                Left = 8,
                Top = header.Height + 8,
                Width = ClientSize.Width - 16,
                Height = 26,
                BackColor = Color.Transparent
            };
            Controls.Add(tabBar);

            tabSettings = CreateTabButton("Tab", 0, 70);
            tabSettings.Click += (s, e) => SelectTab(0);
            tabInfo = CreateTabButton("Info", 74, 70);
            tabInfo.Click += (s, e) => SelectTab(1);
            tabBar.Controls.Add(tabSettings);
            tabBar.Controls.Add(tabInfo);

            bodyHost = new Panel
            {
                Left = 8,
                Top = tabBar.Top + tabBar.Height + 6,
                Width = ClientSize.Width - 16,
                Height = ClientSize.Height - (tabBar.Top + tabBar.Height + 6) - 8,
                BackColor = Color.Transparent,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };
            Controls.Add(bodyHost);

            BuildSettingsPage();

            SelectTab(0);

            foreach (var f in FeatureManager.Features)
            {
                f.StateChanged += _ =>
                {
                    try { Config.Save(); } catch { }
                };
            }

            FormClosing += (s, e) =>
            {
                Config.Save();
            };

            var syncTimer = new Timer { Interval = 120 };
            syncTimer.Tick += (s, e) => SyncUiToFeatures();
            syncTimer.Start();

            var animPump = new Timer { Interval = AnimIntervalMs };
            animPump.Tick += (s, e) => AnimTick();
            animPump.Start();

            CenterOnRobloxMonitor();

            Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = SmoothingMode.None;
                using (var pen = new Pen(Theme.BorderBright))
                    e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
            };
        }

        void OnPaletteChanged()
        {
            Theme.Reskin(this, true);
            InvalidateRecursive(this);
            Update();
        }

        static void InvalidateRecursive(Control c)
        {
            c.Invalidate();
            foreach (Control child in c.Controls)
                InvalidateRecursive(child);
        }

        void OnThemeChanged()
        {
            if (cboTheme == null) return;
            string name = cboTheme.SelectedItem as string ?? "Blue";
            Theme.Apply(name);
            Log($"Theme: {name}");
        }


        void BuildSettingsPage()
        {
            pageSettings = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Theme.BgPanel,
                Tag = "BgPanel"
            };
            bodyHost.Controls.Add(pageSettings);
            pageSettings.Visible = false;

            int bodyW = pageSettings.Width;
            int leftW = (int)(bodyW * 0.65);
            int rightW = bodyW - leftW - 8;
            int viewH = pageSettings.Height;

            var scroll = new ScrollContainer
            {
                Left = 0,
                Top = 0,
                Width = leftW,
                Height = viewH,
                BackColor = Color.Transparent,
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Bottom
            };
            pageSettings.Controls.Add(scroll);
            scroll.Paint += (s, e) =>
            {
                using (var pen = new Pen(Theme.BorderCol))
                    e.Graphics.DrawRectangle(pen, 0, 0, scroll.Width - 1, scroll.Height - 1);
            };

            leftColViewport = scroll;
            leftColContent = scroll.Content;

            leftColContent.Width = scroll.ClientSize.Width;
            leftColContent.Height = 2000;

            rightCol = new Panel
            {
                Left = leftW + 8,
                Top = 0,
                Width = rightW,
                Height = viewH,
                BackColor = Theme.BgPanel,
                Tag = "BgPanel",
                Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom
            };
            pageSettings.Controls.Add(rightCol);
            rightCol.Paint += (s, e) =>
            {
                using (var pen = new Pen(Theme.BorderCol))
                    e.Graphics.DrawRectangle(pen, 0, 0, rightCol.Width - 1, rightCol.Height - 1);
            };

            BuildFeatureColumns();
            BuildRightColumn();
            BuildInfoPage();
        }

        void BuildFeatureColumns()
        {
            int pad = 10;
            int colGap = 10;
            int totalWidth = leftColViewport.ClientSize.Width;
            int usable = totalWidth - pad * 2 - 8;
            int colW = (usable - colGap) / 2;
            int colAx = pad;
            int colBx = pad + colW + colGap;

            int ay = 10;
            int by = 10;
            bool useA = true;

            foreach (var f in FeatureManager.Features)
            {
                int x = useA ? colAx : colBx;
                ref int y = ref (useA ? ref ay : ref by);
                BuildFeatureBlock(f, x, ref y, colW);
                y += 8;
                useA = !useA;
            }

            int contentBottom = Math.Max(ay, by) + 20;
            leftColContent.Height = contentBottom;

            if (leftColViewport != null)
            {
                leftColViewport.Content.Width = leftColViewport.ClientSize.Width;
                leftColViewport.Content.Height = contentBottom;
                leftColViewport.Content.PerformLayout();
                leftColViewport.RecalculateScrollRange();
            }
        }

        void BuildFeatureBlock(Feature f, int x, ref int y, int width)
        {
            AddGroupHeader(leftColContent, f.Name, x, y);
            y += 20;

            var toggle = new SubToggle("Enabled", f.IsEnabled, v => f.SetEnabled(v));
            toggle.Left = x; toggle.Top = y; toggle.Width = width;
            leftColContent.Controls.Add(toggle);
            _featureToggles[f] = toggle;
            y += 24;

            var keyRow = new KeyBindRow("Keybind", f.ToggleKey, vk =>
            {
                f.ToggleKey = vk;
                Config.Save();
            });
            keyRow.Left = x; keyRow.Top = y; keyRow.Width = width;
            leftColContent.Controls.Add(keyRow);
            y += 24;

            f.StateChanged += feat =>
            {
                try
                {
                    toggle.BeginInvoke(new Action(() =>
                    {
                        toggle.SetValueSilent(feat.IsEnabled);
                    }));
                }
                catch { }
            };

            var map = new Dictionary<string, Control>();

            foreach (var kv in f.Settings)
            {
                string key = kv.Key;

                bool isKeySetting =
                    key.EndsWith("Key", StringComparison.OrdinalIgnoreCase) ||
                    key.EndsWith("KeyVK", StringComparison.OrdinalIgnoreCase) ||
                    key.EndsWith("Bind", StringComparison.OrdinalIgnoreCase);

                if (isKeySetting && kv.Value is int keyCode)
                {
                    var kr = new KeyBindRow(key, keyCode, v => f.Settings[key] = v);
                    kr.Left = x; kr.Top = y; kr.Width = width;
                    leftColContent.Controls.Add(kr);
                    map[key] = kr;
                    y += 24;
                    continue;
                }

                if (kv.Value is bool b)
                {
                    var t = new SubToggle(key, b, v => f.Settings[key] = v);
                    t.Left = x; t.Top = y; t.Width = width;
                    leftColContent.Controls.Add(t);
                    map[key] = t;
                    y += 24;
                }
                else if (kv.Value is int i)
                {
                    int max = GuessMaxFor(key);
                    var s = new SubSlider(key, 0, max, i, v => f.Settings[key] = v);
                    s.Left = x; s.Top = y; s.Width = width;
                    leftColContent.Controls.Add(s);
                    map[key] = s;
                    y += 24;
                }
                else if (kv.Value is float fl)
                {
                    int scaled = (int)Math.Round(fl * 100f);
                    int max = GuessMaxFor(key);
                    var s = new SubSlider(key + " (x100)", 0, max, scaled, v => f.Settings[key] = v / 100f);
                    s.Left = x; s.Top = y; s.Width = width;
                    leftColContent.Controls.Add(s);
                    map[key] = s;
                    y += 24;
                }
            }

            _featureSettingControls[f] = map;
        }

        static int GuessMaxFor(string key)
        {
            switch (key)
            {
                case "Fov": return 360;
                case "BoxThickness": return 5;
                case "WidthRatio": return 90;
                case "RadiusPx": return 20;
                case "DelayMs": return 500;
                case "MaxDistance": return 5000;
                case "MaxTargetDistance": return 5000;
                case "TickMs": return 100;
                case "Speed": return 500;
                case "Smoothing": return 100;
                case "NoclipRadius": return 200;
                case "VisCheckSamples": return 64;
                default: return 100;
            }
        }


        void BuildRightColumn()
        {
            int y = 10;
            int x = 10;


            AddGroupHeader(rightCol, "Attach", x, y); y += 20;

            bool attached = Mem.IsAttached && Mem.IsHeap(Roblox.DataModel);

            lblAttachStatus = new Label
            {
                Text = attached ? $"Attached — {Roblox.LocalName}" : "Not attached",
                ForeColor = attached ? Theme.Accent : Theme.CloseRed,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 8.5f),
                AutoSize = false,
                Left = x,
                Top = y,
                Width = rightCol.Width - 20,
                Height = 16
            };
            rightCol.Controls.Add(lblAttachStatus);
            y += 20;

            btnAttach = MakeFlatButton(rightCol, "Attach", x, y, rightCol.Width - 20, 22);
            btnAttach.Click += (s, e) =>
            {
                lblAttachStatus.Text = "Attaching...";
                lblAttachStatus.ForeColor = Theme.TextDim;
                btnAttach.Enabled = false;

                System.Threading.ThreadPool.QueueUserWorkItem(_ =>
                {
                    bool ok = Program.TryAttach();
                    try
                    {
                        BeginInvoke(new Action(() =>
                        {
                            btnAttach.Enabled = true;
                            if (ok)
                            {
                                lblAttachStatus.Text = $"Attached — {Roblox.LocalName}";
                                lblAttachStatus.ForeColor = Theme.Accent;

                                Config.Load();
                                FeatureManager.StartHotkeys();
                                MenuKeys.Install();

                                Log("Attached successfully.");
                            }
                            else
                            {
                                lblAttachStatus.Text = "Not attached — is Roblox running?";
                                lblAttachStatus.ForeColor = Theme.CloseRed;
                                Log("Attach failed.");
                            }
                        }));
                    }
                    catch { }
                });
            };
            rightCol.Controls.Add(btnAttach);
            y += 32;



            AddGroupHeader(rightCol, "Window", x, y); y += 20;

            var menuKeyRow = new KeyBindRow("Open / Close", MenuKeys.ToggleKey, vk =>
            {
                MenuKeys.ToggleKey = vk;
                Config.Save();
            });
            menuKeyRow.Left = x;
            menuKeyRow.Top = y - 2;
            menuKeyRow.Width = rightCol.Width - 20;
            rightCol.Controls.Add(menuKeyRow);
            y += 26;

            AddRowLabel(rightCol, "Position X", x, y);
            txtPosX = MakeInsetInput(rightCol, "0.5", rightCol.Width - 90, y - 2, 80, 20);
            y += 26;

            AddRowLabel(rightCol, "Position Y", x, y);
            txtPosY = MakeInsetInput(rightCol, "35", rightCol.Width - 90, y - 2, 80, 20);
            y += 30;


            AddGroupHeader(rightCol, "Theme", x, y); y += 20;

            AddRowLabel(rightCol, "Presets", x, y);
            cboTheme = new ComboBox
            {
                Left = rightCol.Width - 110,
                Top = y - 2,
                Width = 100,
                Height = 22,
                DropDownStyle = ComboBoxStyle.DropDownList,
                FlatStyle = FlatStyle.Flat,
                BackColor = Theme.BgInset,
                ForeColor = Theme.TextMain,
                Font = new Font("Segoe UI", 8.5f)
            };
            cboTheme.Items.AddRange(Theme.Presets);
            cboTheme.SelectedItem = Theme.Current;
            cboTheme.SelectedIndexChanged += (s, e) => OnThemeChanged();
            rightCol.Controls.Add(cboTheme);
            y += 30;


            AddGroupHeader(rightCol, "Config", x, y); y += 20;

            var btnSaveCfg = MakeFlatButton(rightCol, "Save Config", x, y, 100, 22);
            btnSaveCfg.Click += (s, e) => { Config.Save(); Log("Config saved."); };

            var btnLoadCfg = MakeFlatButton(rightCol, "Load Config", x + 106, y, 100, 22);
            btnLoadCfg.Click += (s, e) =>
            {
                Config.Load();
                SyncUiToFeatures();
                Log("Config loaded.");
            };
            y += 28;

            var btnOpenFolder = MakeFlatButton(rightCol, "Open Config Folder", x, y, rightCol.Width - 20, 22);
            btnOpenFolder.Click += (s, e) =>
            {
                try
                {
                    System.Diagnostics.Process.Start("explorer.exe",
                        "/select,\"" + Config.FilePath + "\"");
                }
                catch (Exception ex) { Log("Open folder failed: " + ex.Message); }
            };
            y += 28;

            var btnResetCfg = MakeFlatButton(rightCol, "Reset to Defaults", x, y, rightCol.Width - 20, 22);
            btnResetCfg.Click += (s, e) =>
            {
                var result = MessageBox.Show(
                    "Delete the saved config and use defaults on next launch?",
                    "NullEx", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.Yes)
                {
                    Config.Delete();
                    Log("Config reset. Restart to apply defaults.");
                }
            };
            y += 32;

            AddGroupHeader(rightCol, "Utilities", x, y); y += 20;

            var btnListPlayers = MakeFlatButton(rightCol, "List Players", x, y, 100, 22);
            btnListPlayers.Click += (s, e) => Program.CmdPlayers();
            var btnMyPos = MakeFlatButton(rightCol, "My Position", x + 106, y, 100, 22);
            btnMyPos.Click += (s, e) => Program.CmdPos();
            y += 28;

            var btnRefresh = MakeFlatButton(rightCol, "Refresh Pointers", x, y, rightCol.Width - 20, 22);
            btnRefresh.Click += (s, e) =>
            {
                if (Program.ResolveCore()) Log("Refreshed.");
                else Log("Refresh failed.");
            };
            y += 32;


            AddGroupHeader(rightCol, "Credits", x, y); y += 20;

            var ownersBar = new Panel
            {
                Left = x,
                Top = y,
                Width = rightCol.Width - 20,
                Height = 22,
                BackColor = Theme.AccentSoft,
                Tag = "AccentSoft"
            };
            rightCol.Controls.Add(ownersBar);
            ownersBar.Paint += (s, e) =>
            {
                using (var pen = new Pen(Theme.BorderBright))
                    e.Graphics.DrawRectangle(pen, 0, 0, ownersBar.Width - 1, ownersBar.Height - 1);
                using (var font = new Font("Segoe UI Semibold", 8f, FontStyle.Bold))
                using (var brush = new SolidBrush(Theme.TextMain))
                    e.Graphics.DrawString("Owners / Developers", font, brush, 8, 4);
            };
            y += 28;

            AddRowLabel(rightCol, "Overlay", x + 8, y); y += 20;
        }

        void BuildInfoPage()
        {
            pageInfo = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.Transparent
            };
            bodyHost.Controls.Add(pageInfo);
            pageInfo.Visible = false;

            var lbl = new Label
            {
                Left = 24,
                Top = 24,
                Width = pageInfo.Width - 48,
                Height = pageInfo.Height - 48,
                ForeColor = Theme.TextDim,
                BackColor = Color.Transparent,
                Font = new Font("Consolas", 9.5f),
                Text = "NullEx v1.0\r\n\r\n" +
                       "Each feature has a clickable Keybind row — click it and\r\n" +
                       "press any key or mouse button to rebind. Esc cancels.\r\n" +
                       "Right-click a keybind row to clear it.\r\n\r\n" +
                       "Fly controls:\r\n" +
                       "  WASD       move   Ctrl: down   Shift: boost"
            };
            pageInfo.Controls.Add(lbl);
        }

        void SyncUiToFeatures()
        {
            foreach (var kv in _featureToggles)
            {
                var f = kv.Key;
                var t = kv.Value;
                if (t.Value != f.IsEnabled)
                    t.SetValueSilent(f.IsEnabled);
            }
            if (lblAttachStatus != null && !lblAttachStatus.IsDisposed)
            {
                bool attached = Mem.IsAttached && Mem.IsHeap(Roblox.DataModel);
                string want = attached ? $"Attached — {Roblox.LocalName}" : "Not attached";
                if (lblAttachStatus.Text != want)
                {
                    lblAttachStatus.Text = want;
                    lblAttachStatus.ForeColor = attached ? Theme.Accent : Theme.CloseRed;
                }
            }
        }

        void AddGroupHeader(Control parent, string text, int x, int y)
        {
            parent.Controls.Add(new Label
            {
                Text = text,
                ForeColor = Theme.Accent,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI Semibold", 9f, FontStyle.Bold),
                AutoSize = true,
                Location = new Point(x, y)
            });
        }

        void AddRowLabel(Control parent, string text, int x, int y)
        {
            parent.Controls.Add(new Label
            {
                Text = text,
                ForeColor = Theme.TextDim,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 8.5f),
                AutoSize = true,
                Location = new Point(x, y)
            });
        }

        TextBox MakeInsetInput(Control parent, string text, int x, int y, int w, int h)
        {
            var tb = new TextBox
            {
                Left = x,
                Top = y,
                Width = w,
                Height = h,
                Text = text,
                BackColor = Theme.BgInset,
                ForeColor = Theme.TextMain,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Consolas", 8.5f)
            };
            parent.Controls.Add(tb);
            return tb;
        }

        Button MakeFlatButton(Control parent, string text, int x, int y, int w, int h)
        {
            var b = new Button
            {
                Text = text,
                Left = x,
                Top = y,
                Width = w,
                Height = h,
                FlatStyle = FlatStyle.Flat,
                BackColor = Theme.BgRow,
                ForeColor = Theme.TextMain,
                Font = new Font("Segoe UI", 8f),
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };
            b.FlatAppearance.BorderColor = Theme.BorderCol;
            b.FlatAppearance.BorderSize = 1;
            b.FlatAppearance.MouseOverBackColor = Theme.BgRowHv;
            b.FlatAppearance.MouseDownBackColor = Theme.AccentDim;
            parent.Controls.Add(b);
            return b;
        }

        Button CreateTabButton(string text, int x, int w)
        {
            var b = new Button
            {
                Text = text,
                Left = x,
                Top = 0,
                Width = w,
                Height = 26,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                ForeColor = Theme.TextDim,
                Font = new Font("Segoe UI", 9f),
                Cursor = Cursors.Hand,
                UseVisualStyleBackColor = false
            };
            b.FlatAppearance.BorderSize = 0;
            b.FlatAppearance.MouseOverBackColor = Color.Transparent;

            b.Paint += (s, e) =>
            {
                bool active = (b == tabSettings && activePage == pageSettings) ||
                              (b == tabInfo && activePage == pageInfo);

                using (var bg = new SolidBrush(active ? Theme.BgInset : Theme.BgPanel))
                    e.Graphics.FillRectangle(bg, b.ClientRectangle);

                using (var pen = new Pen(Theme.BorderCol))
                    e.Graphics.DrawLine(pen, 0, b.Height - 1, b.Width, b.Height - 1);

                if (active)
                {
                    using (var pen = new Pen(Theme.Accent, 2))
                        e.Graphics.DrawLine(pen, 0, b.Height - 2, b.Width, b.Height - 2);
                }

                Color fg = active ? Theme.Accent : Theme.TextDim;
                TextRenderer.DrawText(e.Graphics, b.Text, b.Font, b.ClientRectangle, fg,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            };
            return b;
        }

        void SelectTab(int index)
        {
            pageSettings.Visible = index == 0;
            pageInfo.Visible = index == 1;
            activePage = index == 0 ? pageSettings : pageInfo;
            tabSettings.Invalidate();
            tabInfo.Invalidate();
        }

        Button CreateTitleButton(string text, int x, int y, int w, int h)
        {
            var b = new Button
            {
                Text = text,
                Left = x,
                Top = y,
                Width = w,
                Height = h,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.Transparent,
                Font = new Font("Segoe UI", 9f),
                Cursor = Cursors.Hand
            };
            b.FlatAppearance.BorderSize = 0;
            b.FlatAppearance.MouseOverBackColor = Color.Transparent;
            b.FlatAppearance.MouseDownBackColor = Color.Transparent;

            bool hovered = false;
            b.MouseEnter += (s, e) => { hovered = true; b.Invalidate(); };
            b.MouseLeave += (s, e) => { hovered = false; b.Invalidate(); };

            b.Paint += (s, e) =>
            {
                Color bg = Color.Transparent;
                Color fg = Theme.TextMuted;

                if (hovered)
                {
                    if (text == "x") { bg = Theme.CloseRed; fg = Color.White; }
                    else { bg = Theme.BgRowHv; fg = Theme.TextMain; }
                }

                if (bg != Color.Transparent)
                    using (var brush = new SolidBrush(bg))
                        e.Graphics.FillRectangle(brush, b.ClientRectangle);

                TextRenderer.DrawText(e.Graphics, text, b.Font, b.ClientRectangle, fg,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            };
            return b;
        }

        public void BeginShowExternal() => BeginShow();
        public void BeginHideExternal() => BeginHide();

        void BeginShow()
        {
            CenterOnRobloxMonitor();
            g_AnimTargetPos = Location;
            g_AnimStartPos = new Point(Location.X, Location.Y + 20);
            Location = g_AnimStartPos;
            ShowInTaskbar = true;
            Show();
            BringToFront();
            StartAnim(true);
        }

        void BeginHide()
        {
            g_AnimStartPos = Location;
            g_AnimTargetPos = new Point(Location.X, Location.Y + 20);
            StartAnim(false);
        }

        void StartAnim(bool opening)
        {
            g_Animating = true;
            g_AnimOpening = opening;
            g_AnimStep = 0;
        }

        void AnimTick()
        {
            if (!g_Animating) return;

            g_AnimStep++;
            float t = g_AnimStep / (float)AnimSteps;
            if (t > 1f) t = 1f;
            float eased = 1f - (float)Math.Pow(1f - t, 3);
            int y = g_AnimStartPos.Y + (int)((g_AnimTargetPos.Y - g_AnimStartPos.Y) * eased);
            Location = new Point(g_AnimStartPos.X, y);

            if (g_AnimStep >= AnimSteps)
            {
                g_Animating = false;
                if (g_AnimOpening) { Location = g_AnimTargetPos; Activate(); }
                else { Location = g_AnimTargetPos; Hide(); ShowInTaskbar = false; }
            }
        }

        void CenterOnRobloxMonitor()
        {
            try
            {
                Screen target = GetRobloxScreen();
                var area = target.WorkingArea;
                Location = new Point(
                    area.X + (area.Width - Width) / 2,
                    area.Y + (area.Height - Height) / 2);
            }
            catch
            {
                var area = Screen.PrimaryScreen.WorkingArea;
                Location = new Point(
                    area.X + (area.Width - Width) / 2,
                    area.Y + (area.Height - Height) / 2);
            }
        }

        Screen GetRobloxScreen()
        {
            IntPtr hwnd = Program.GetRobloxHwnd();
            if (hwnd == IntPtr.Zero) hwnd = GetForegroundWindow();
            if (hwnd != IntPtr.Zero)
            {
                RECT r;
                if (GetWindowRect(hwnd, out r))
                {
                    var center = new Point((r.Left + r.Right) / 2, (r.Top + r.Bottom) / 2);
                    foreach (var s in Screen.AllScreens)
                        if (s.Bounds.Contains(center)) return s;
                }
            }
            return Screen.PrimaryScreen;
        }

        void ApplyRoundedRegion()
        {
            using (var path = RoundedRect(new Rectangle(0, 0, Width, Height), CornerRadius))
                Region = new Region(path);
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

        void Header_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left) { isDragging = true; dragStart = e.Location; }
        }

        void Header_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragging)
            {
                Point p = PointToScreen(e.Location);
                Location = new Point(p.X - dragStart.X, p.Y - dragStart.Y);
            }
        }

        void Header_MouseUp(object sender, MouseEventArgs e) => isDragging = false;

        public static void Log(string msg)
        {
            try
            {
                string dir = System.IO.Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "NullEx");
                System.IO.Directory.CreateDirectory(dir);

                string file = System.IO.Path.Combine(dir, "log.txt");
                System.IO.File.AppendAllText(file,
                    $"[{DateTime.Now:HH:mm:ss}] {msg}\r\n");
            }
            catch { }

            System.Diagnostics.Debug.WriteLine($"[NullEx] {msg}");
        }
    }
}