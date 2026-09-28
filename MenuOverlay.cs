using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace NullEx
{
    public static class MenuOverlay
    {
        static MenuDimForm _form;
        static Thread _thread;
        static volatile bool _running;

        public static void Start()
        {
            if (_running) return;
            _running = true;

            _thread = new Thread(() =>
            {
                _form = new MenuDimForm();
                _form.Show();
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

        class MenuDimForm : Form
        {
            public MenuDimForm()
            {
                FormBorderStyle = FormBorderStyle.None;
                StartPosition = FormStartPosition.Manual;
                Bounds = SystemInformation.VirtualScreen;
                BackColor = Color.Black;
                Opacity = 0.35;
                ShowInTaskbar = false;
                TopMost = false;
                Visible = false;

                SetStyle(ControlStyles.OptimizedDoubleBuffer
                       | ControlStyles.AllPaintingInWmPaint
                       | ControlStyles.UserPaint, true);

                Click += (s, e) => SetVisible(false);
                MouseClick += (s, e) => SetVisible(false);
            }
        }
    }
}