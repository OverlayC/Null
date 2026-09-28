using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

namespace NullEx
{
    public static class FeatureManager
    {
        private static readonly List<Feature> _features = new List<Feature>();
        private static Thread _keyThread;
        private static volatile bool _running;

        public static IReadOnlyList<Feature> Features => _features;

        public static T Register<T>(T feature) where T : Feature
        {
            _features.Add(feature);
            return feature;
        }

        public static Feature Get(string name)
            => _features.Find(f => f.Name.Equals(name, StringComparison.OrdinalIgnoreCase));

        public static T Get<T>() where T : Feature
            => (T)_features.Find(f => f is T);

        public static void Enable(string name) => Get(name)?.SetEnabled(true);
        public static void Disable(string name) => Get(name)?.SetEnabled(false);

        public static void Toggle(string name)
        {
            var f = Get(name);
            if (f == null) return;
            f.SetEnabled(!f.IsDesired);
        }

        public static bool IsEnabled(string name) => Get(name)?.IsEnabled ?? false;

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vk);

        public static void StartHotkeys()
        {
            if (_running)
            {
                MainForm.Log("StartHotkeys: already running, ignoring duplicate call");
                return;
            }
            _running = true;

            MainForm.Log($"StartHotkeys: starting pump for {_features.Count} features");
            foreach (var f in _features)
                MainForm.Log($"  feature '{f.Name}' ToggleKey={f.ToggleKey} (0x{f.ToggleKey:X})");

            _keyThread = new Thread(() =>
            {
                var down = new Dictionary<Feature, bool>();

                while (_running)
                {
                    foreach (var f in _features)
                    {
                        int vk = f.ToggleKey;
                        if (vk == 0) continue;

                        bool pressed = (GetAsyncKeyState(vk) & 0x8000) != 0;
                        down.TryGetValue(f, out bool wasDown);

                        if (pressed && !wasDown)
                        {
                            bool nextState = !f.IsDesired;
                            MainForm.Log($"Hotkey: '{f.Name}' key 0x{vk:X} pressed -> SetEnabled({nextState})");
                            f.SetEnabled(nextState);
                        }

                        down[f] = pressed;
                    }
                    Thread.Sleep(20);
                }

                MainForm.Log("Hotkey pump exiting");
            })
            { IsBackground = true, Name = "FeatureHotkeys" };

            _keyThread.Start();
        }

        public static void StopHotkeys()
        {
            _running = false;
        }

        public static void DisableAll()
        {
            foreach (var f in _features) f.SetEnabled(false);
        }
    }
}