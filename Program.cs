using System;
using System.Threading;
using System.Windows.Forms;

using Application = System.Windows.Forms.Application;

namespace NullEx
{
    class Program
    {
        public static class Settings
        {
            public static volatile int EspWidthRatio = 50;
        }

        public static bool ResolveCore() => Roblox.Resolve();

        public static void CmdPlayers()
        {
            MainForm.Log("=== All Players ===");
            foreach (IntPtr p in Mem.GetChildren(Roblox.Players))
            {
                string nm = Mem.ReadInstanceName(p);
                if (string.IsNullOrEmpty(nm)) continue;

                long uid = Mem.ReadI64(Mem.Add(p, Offsets.Player.UserId));
                IntPtr ch = Roblox.Character(p);

                float hp = -1, maxHp = -1;
                if (Mem.IsHeap(ch))
                {
                    IntPtr hum = Roblox.Humanoid(ch);
                    if (Mem.IsHeap(hum))
                    {
                        hp = Mem.ReadF32(Mem.Add(hum, Offsets.Humanoid.Health));
                        maxHp = Mem.ReadF32(Mem.Add(hum, Offsets.Humanoid.MaxHealth));
                    }
                }

                Vector3 pos = Mem.IsHeap(ch) ? Roblox.CharacterPosition(ch) : new Vector3();
                MainForm.Log($"  [{uid}] {nm}  HP={hp:F1}/{maxHp:F1}  pos={pos}");
            }
        }

        public static void CmdPos()
        {
            IntPtr ch = Roblox.LocalCharacter();
            if (!Mem.IsHeap(ch)) { MainForm.Log("Local character not found."); return; }
            MainForm.Log($"Position: {Roblox.CharacterPosition(ch)}");
        }

        public static IntPtr GetRobloxHwnd() => Mem.RobloxHwnd;

        public static bool TryAttach()
        {
            if (Mem.IsAttached)
            {
                LoadingScreen.SetStatus("Re-resolving pointers...", 40);
                bool ok = Roblox.Resolve();
                LoadingScreen.SetStatus(ok ? "Ready" : "Resolve failed", ok ? 100 : 40);
                return ok;
            }

            LoadingScreen.SetStatus("Finding Roblox process...", 15);
            if (!Mem.Attach())
            {
                LoadingScreen.SetStatus("Roblox not found", 0);
                return false;
            }

            LoadingScreen.SetStatus("Resolving pointers...", 40);
            if (!Roblox.Resolve())
            {
                LoadingScreen.SetStatus("Resolve failed", 40);
                return false;
            }

            for (int i = 0; i < 40; i++)
            {
                bool ready =
                    Mem.IsHeap(Roblox.DataModel) &&
                    Mem.IsHeap(Roblox.Workspace) &&
                    Mem.IsHeap(Roblox.Players) &&
                    Mem.IsHeap(Roblox.LocalCharacter());

                if (ready)
                {
                    LoadingScreen.SetStatus("Ready", 100);
                    return true;
                }

                LoadingScreen.SetStatus("Waiting for game to load...", 50 + Math.Min(40, i));
                Thread.Sleep(250);
                Roblox.Resolve();
            }

            bool usable = Mem.IsHeap(Roblox.DataModel) && Mem.IsHeap(Roblox.Workspace);
            LoadingScreen.SetStatus(usable ? "Ready (no character yet)" : "Not ready", usable ? 100 : 60);
            return usable;
        }

        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            LoadingScreen.Start();
            LoadingScreen.SetStatus("Registering features...", 5);

            FeatureManager.Register(new EspFeature());
            FeatureManager.Register(new AimFeature());
            FeatureManager.Register(new FlyFeature());
            FeatureManager.Register(new TriggerbotFeature());

            bool attached = TryAttach();

            if (attached)
            {
                LoadingScreen.SetStatus("Loading config...", 90);
                Config.Load();

                LoadingScreen.SetStatus("Starting hotkeys...", 95);
                FeatureManager.StartHotkeys();
                MenuKeys.Install();

                MainForm.Log($"Attached: PID {Mem.ProcessId}");
                MainForm.Log($"Local:    {Roblox.LocalName}");
                MainForm.Log($"Config:   {Config.FilePath}");
            }
            else
            {
                MainForm.Log("Roblox not found — open the game and click Attach.");
            }

            LoadingScreen.SetStatus("Opening menu...", 100);
            Thread.Sleep(200);
            LoadingScreen.Stop();
            Thread.Sleep(120);


            Application.Run(new MainForm());

            MenuKeys.Uninstall();
            Config.Save();
            FeatureManager.StopHotkeys();
            FeatureManager.DisableAll();
            Mem.Detach();
        }
    }
}