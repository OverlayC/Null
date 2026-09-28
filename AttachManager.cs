using System;
using System.Threading;
using System.Windows.Forms;

namespace NullEx
{
    public static class AttachManager
    {
        public static bool IsAttached => Mem.IsAttached && Mem.IsHeap(Roblox.DataModel);

        public static bool Attach()
        {
            if (IsAttached) return true;

            if (!Mem.Attach())
            {
                MainForm.Log("Attach failed: could not open process");
                return false;
            }

            for (int i = 0; i < 40; i++)
            {
                if (Roblox.Resolve() &&
                    Mem.IsHeap(Roblox.DataModel) &&
                    Mem.IsHeap(Roblox.Workspace) &&
                    Mem.IsHeap(Roblox.Players))
                {
                    MainForm.Log($"Attached: PID {Mem.ProcessId}  Local={Roblox.LocalName}");
                    HudOverlay.Start();
                    return true;
                }
                Thread.Sleep(250);
            }

            MainForm.Log("Attach failed: Roblox pointers never resolved");
            Mem.Detach();
            return false;
        }

        public static void AttachAsync(Action<bool> onDone)
        {
            var t = new Thread(() =>
            {
                bool ok = Attach();
                try { onDone?.Invoke(ok); } catch { }
            })
            { IsBackground = true };
            t.Start();
        }

        public static void Detach()
        {
            if (!Mem.IsAttached) return;

            try
            {
                FeatureManager.DisableAll();
                FeatureManager.StopHotkeys();
                Overlay.Stop();
                HudOverlay.Stop();

                Thread.Sleep(150);
                Mem.Detach();

                MainForm.Log("Detached.");
            }
            catch (Exception ex)
            {
                MainForm.Log($"Detach error: {ex.Message}");
            }
        }

        public static void DetachAsync(Action onDone)
        {
            var t = new Thread(() =>
            {
                Detach();
                try { onDone?.Invoke(); } catch { }
            })
            { IsBackground = true };
            t.Start();
        }
    }
}