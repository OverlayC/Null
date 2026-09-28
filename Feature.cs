using System;
using System.Collections.Generic;
using System.Threading;

namespace NullEx
{
    public abstract class Feature
    {
        public abstract string Name { get; }
        public virtual string Description => "";

        public int ToggleKey { get; set; } = 0;

        private volatile bool _enabled;
        private volatile bool _desired;
        private Thread _thread;

        public bool IsEnabled => _enabled;
        public bool IsDesired => _desired;
        public bool IsRunning => _thread != null && _thread.IsAlive;

        public Dictionary<string, object> Settings { get; } = new Dictionary<string, object>();

        public HashSet<string> PersistentKeys { get; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        public event Action<Feature> StateChanged;

        protected T Get<T>(string key, T fallback = default)
        {
            if (Settings.TryGetValue(key, out var v) && v is T t) return t;
            return fallback;
        }

        protected void Set<T>(string key, T value) => Settings[key] = value;

        internal void SetEnabled(bool on)
        {
            Log($"SetEnabled({on})  desired_before={_desired}  enabled={_enabled}");
            if (_desired == on) { Log("no-op (already in that state)"); return; }
            _desired = on;
            RaiseChanged();
            if (on) StartThread();
            Log($"SetEnabled done  desired_after={_desired}");
        }

        private void StartThread()
        {
            if (_thread != null && _thread.IsAlive)
            {
                Log("StartThread: thread already alive, skipping");
                return;
            }

            Log("StartThread: creating thread");
            _thread = new Thread(() =>
            {
                Log("thread body entered");
                try { OnEnable(); }
                catch (Exception ex)
                {
                    Log($"OnEnable FAILED: {ex}");
                    _desired = false;
                    RaiseChanged();
                    return;
                }

                _enabled = true;
                Log("_enabled = true; entering update loop");

                try
                {
                    int ticks = 0;
                    while (_desired)
                    {
                        try { OnUpdate(); }
                        catch (Exception ex)
                        {
                            Log($"OnUpdate FAILED: {ex.Message}");
                            Thread.Sleep(50);
                        }

                        if (++ticks % 200 == 0)
                            Log($"alive, {ticks} ticks");
                    }
                }
                finally
                {
                    _enabled = false;
                    try { OnDisable(); }
                    catch (Exception ex) { Log($"OnDisable FAILED: {ex.Message}"); }
                    Log("thread exited");
                }
            })
            {
                IsBackground = true,
                Name = $"Feature:{Name}"
            };

            _thread.Start();
        }

        protected virtual int UpdateIntervalMs => 4;

        protected virtual void OnEnable() { }
        protected virtual void OnDisable() { }
        protected abstract void OnUpdate();

        protected void Log(string msg) => MainForm.Log($"[{Name}] {msg}");

        private void RaiseChanged()
        {
            try { StateChanged?.Invoke(this); } catch { }
        }
    }
}