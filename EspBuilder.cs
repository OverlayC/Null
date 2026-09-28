using System;
using System.Collections.Generic;
using System.Drawing;

namespace NullEx
{
    public static class EspBuilder
    {
        static string _cachedMyName;
        static int _frame;
        static readonly Dictionary<long, PlayerCacheEntry> _charCache = new Dictionary<long, PlayerCacheEntry>();
        static readonly Dictionary<long, BoxCacheEntry> _boxCache = new Dictionary<long, BoxCacheEntry>();

        class PlayerCacheEntry
        {
            public IntPtr Character;
            public int LastSeenFrame;
        }

        class BoxCacheEntry
        {
            public Vector3 Top;
            public Vector3 Bottom;
            public int LastFrame;
        }

        public static void InvalidateCaches()
        {
            _cachedMyName = null;
            _charCache.Clear();
            _boxCache.Clear();
        }

        public static List<DrawItem> BuildEspItems(Rectangle viewport)
        {
            _frame++;

            var list = new List<DrawItem>();
            var esp = FeatureManager.Get<EspFeature>();
            if (esp == null) return list;

            IntPtr camera = Roblox.Camera();
            if (!Mem.IsHeap(camera)) return list;

            if (!Roblox.ReadCameraFull(camera, out Vector3 camPos, out Vector3 camRight, out Vector3 camUp,
                                        out Vector3 camFwd, out float fov))
                return list;

            if (fov > 3.5f && fov < 200f) fov = fov * (float)Math.PI / 180f;
            if (fov <= 0.05f || fov > 3.2f) fov = 70f * (float)Math.PI / 180f;

            double tanY = Math.Tan(fov / 2.0);
            double aspect = (double)viewport.Width / Math.Max(1, viewport.Height);
            double tanX = tanY * aspect;

            if (string.IsNullOrEmpty(_cachedMyName))
                _cachedMyName = Roblox.LocalName;
            string myName = _cachedMyName;

            foreach (IntPtr p in Mem.GetChildren(Roblox.Players))
            {
                string nm = Mem.ReadInstanceName(p);
                if (string.IsNullOrEmpty(nm) || nm == myName) continue;

                long pKey = p.ToInt64();

                IntPtr ch;
                if (_charCache.TryGetValue(pKey, out var pc) &&
                    Mem.IsHeap(pc.Character) &&
                    _frame - pc.LastSeenFrame < 600)
                {
                    ch = pc.Character;
                    pc.LastSeenFrame = _frame;
                }
                else
                {
                    ch = Roblox.Character(p);
                    _charCache[pKey] = new PlayerCacheEntry { Character = ch, LastSeenFrame = _frame };
                }

                if (!Mem.IsHeap(ch)) continue;

                long chKey = ch.ToInt64();
                Vector3 top, bottom;

                if (_boxCache.TryGetValue(chKey, out var bc) && _frame - bc.LastFrame < 3)
                {
                    top = bc.Top;
                    bottom = bc.Bottom;
                }
                else
                {
                    Roblox.GetBoundingBox(ch, out top, out bottom);
                    _boxCache[chKey] = new BoxCacheEntry { Top = top, Bottom = bottom, LastFrame = _frame };
                }

                if ((top.X == 0 && top.Y == 0 && top.Z == 0) &&
                    (bottom.X == 0 && bottom.Y == 0 && bottom.Z == 0)) continue;

                if (bottom.Y == 0) bottom = new Vector3(top.X, top.Y - 5f, top.Z);
                Vector3 feet = new Vector3(bottom.X, bottom.Y - 0.5f, bottom.Z);

                if (!Project(top, camPos, camRight, camUp, camFwd, viewport, tanX, tanY, out int topX, out int topY, out _)) continue;
                if (!Project(feet, camPos, camRight, camUp, camFwd, viewport, tanX, tanY, out _, out int botY, out _)) continue;

                int h = Math.Abs(botY - topY);
                if (h < 4) h = 4;
                int w = (int)(h * (esp.WidthRatio / 100f));
                if (w < 4) w = 4;

                int bx = topX - w / 2 - viewport.X;
                int by = Math.Min(topY, botY) - viewport.Y;

                float hp = 0, maxHp = 0;
                IntPtr hum = Roblox.Humanoid(ch);
                if (Mem.IsHeap(hum))
                {
                    hp = Mem.ReadF32(Mem.Add(hum, Offsets.Humanoid.Health));
                    maxHp = Mem.ReadF32(Mem.Add(hum, Offsets.Humanoid.MaxHealth));
                }

                float camDist = (top - camPos).Length();
                if (esp.MaxDistance > 0 && camDist > esp.MaxDistance) continue;

                list.Add(new DrawItem
                {
                    X = bx,
                    Y = by,
                    W = w,
                    H = h,
                    Name = nm,
                    Hp = hp,
                    MaxHp = maxHp,
                    Distance = camDist
                });
            }

            if (_frame % 300 == 0)
            {
                var toRemove = new List<long>();
                foreach (var kv in _charCache)
                    if (_frame - kv.Value.LastSeenFrame > 900) toRemove.Add(kv.Key);
                foreach (var k in toRemove) _charCache.Remove(k);

                var toRemove2 = new List<long>();
                foreach (var kv in _boxCache)
                    if (_frame - kv.Value.LastFrame > 900) toRemove2.Add(kv.Key);
                foreach (var k in toRemove2) _boxCache.Remove(k);
            }

            return list;
        }

        static bool Project(Vector3 world, Vector3 camPos, Vector3 right, Vector3 up, Vector3 fwd,
                            Rectangle viewport, double tanX, double tanY,
                            out int sx, out int sy, out float z)
        {
            sx = sy = 0;
            z = 0;

            Vector3 rel = world - camPos;
            float x = Vector3.Dot(rel, right);
            float y = Vector3.Dot(rel, up);
            z = Vector3.Dot(rel, fwd);
            if (z <= 0.1f) return false;

            double px = (x / (z * tanX)) * (viewport.Width / 2.0);
            double py = -(y / (z * tanY)) * (viewport.Height / 2.0);

            sx = (int)(viewport.X + viewport.Width / 2.0 + px);
            sy = (int)(viewport.Y + viewport.Height / 2.0 + py);
            return true;
        }
    }
}