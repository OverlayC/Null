using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace NullEx
{
    public sealed class TriggerbotFeature : Feature
    {
        public override string Name => "Triggerbot";
        public override string Description => "Auto-fire on a visible target";
        protected override int UpdateIntervalMs => 2;

        public TriggerbotFeature()
        {
            ToggleKey = (int)VirtualKeys.T;
            Set("Armed", false);
            Set("RequireKey", false);
            Set("ActivationKey", (int)VirtualKeys.None);
            Set("RadiusPx", 6);
            Set("DelayMs", 80);
            Set("VisibleOnly", true);
            Set("VisCheckSamples", 24);
            Set("MaxTargetDistance", 500f);
        }

        public bool Armed { get => Get<bool>("Armed"); set => Set("Armed", value); }
        public bool RequireKey { get => Get<bool>("RequireKey"); set => Set("RequireKey", value); }
        public int ActivationKey { get => Get<int>("ActivationKey"); set => Set("ActivationKey", value); }
        public int RadiusPx { get => Get<int>("RadiusPx"); set => Set("RadiusPx", value); }
        public int DelayMs { get => Get<int>("DelayMs"); set => Set("DelayMs", value); }
        public bool VisibleOnly { get => Get<bool>("VisibleOnly"); set => Set("VisibleOnly", value); }
        public int VisCheckSamples { get => Get<int>("VisCheckSamples"); set => Set("VisCheckSamples", value); }
        public float MaxTargetDistance { get => Get<float>("MaxTargetDistance"); set => Set("MaxTargetDistance", value); }

        private long _lastShotMs;

        protected override void OnUpdate()
        {
            if (!Armed) return;

            long now = Environment.TickCount64;
            if (now - _lastShotMs < DelayMs) return;

            if (RequireKey)
            {
                int vk = ActivationKey;
                if (vk != (int)VirtualKeys.None && !KeyDown(vk)) return;
            }

            IntPtr camera = Roblox.Camera();
            if (!Mem.IsHeap(camera)) return;

            if (!Roblox.ReadCameraFull(camera, out Vector3 camPos,
                                                out Vector3 camRight,
                                                out Vector3 camUp,
                                                out Vector3 camFwd,
                                                out float fov))
                return;

            NormalizeFov(ref fov);
            Rectangle vp = GetRobloxViewport();

            double tanY = Math.Tan(fov / 2.0);
            double aspect = (double)vp.Width / Math.Max(1, vp.Height);
            if (aspect < 0.1 || aspect > 10.0) aspect = 16.0 / 9.0;
            double tanX = tanY * aspect;

            int centerX = vp.X + vp.Width / 2;
            int centerY = vp.Y + vp.Height / 2;
            int radius = Math.Max(1, (int)(RadiusPx * (vp.Height / 1080.0)));
            int radiusSq = radius * radius;

            IntPtr localChar = Roblox.LocalCharacter();

            if (!PickTarget(camPos, camRight, camUp, camFwd, tanX, tanY,
                            vp, centerX, centerY, radiusSq,
                            out Vector3 targetHead, out IntPtr targetHeadPart))
                return;

            if (VisibleOnly && !IsVisible(camPos, targetHead, targetHeadPart, localChar))
                return;

            SendMouseClick(ActivationKey);
            _lastShotMs = now;
        }

        bool PickTarget(Vector3 camPos, Vector3 camRight, Vector3 camUp, Vector3 camFwd,
                        double tanX, double tanY, Rectangle vp,
                        int centerX, int centerY, int radiusSq,
                        out Vector3 bestHead, out IntPtr bestHeadPart)
        {
            bestHead = new Vector3();
            bestHeadPart = IntPtr.Zero;
            float bestDistSq = float.MaxValue;

            string myName = Roblox.LocalName;

            foreach (IntPtr player in Mem.GetChildren(Roblox.Players))
            {
                string nm = Mem.ReadInstanceName(player);
                if (string.IsNullOrEmpty(nm) || nm == myName) continue;

                IntPtr ch = Roblox.Character(player);
                if (!Mem.IsHeap(ch)) continue;

                IntPtr hum = Roblox.Humanoid(ch);
                if (!Mem.IsHeap(hum)) continue;

                float hp = Mem.ReadF32(Mem.Add(hum, Offsets.Humanoid.Health));
                if (hp <= 0f) continue;

                IntPtr head = Roblox.FindChildByName(ch, "Head");
                Vector3 targetPos;
                if (Mem.IsHeap(head))
                {
                    targetPos = Roblox.PartPosition(head);
                }
                else
                {
                    IntPtr hrp = Roblox.RootPart(hum);
                    if (!Mem.IsHeap(hrp)) continue;
                    targetPos = Roblox.PartPosition(hrp);
                    head = hrp;
                }

                if (targetPos.X == 0 && targetPos.Y == 0 && targetPos.Z == 0) continue;

                Vector3 rel = targetPos - camPos;
                float dist = rel.Length();
                if (dist > MaxTargetDistance) continue;

                float z = Vector3.Dot(rel, camFwd);
                if (z <= 0.1f) continue;

                float x = Vector3.Dot(rel, camRight);
                float y = Vector3.Dot(rel, camUp);

                double ndcX = x / (z * tanX);
                double ndcY = y / (z * tanY);

                int sx = (int)(centerX + ndcX * (vp.Width / 2.0));
                int sy = (int)(centerY - ndcY * (vp.Height / 2.0));

                int dx = sx - centerX;
                int dy = sy - centerY;
                float dSq = dx * dx + dy * dy;

                if (dSq < bestDistSq)
                {
                    bestDistSq = dSq;
                    bestHead = targetPos;
                    bestHeadPart = head;
                }
            }

            return bestHeadPart != IntPtr.Zero && bestDistSq <= radiusSq;
        }

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vk);

        static bool KeyDown(int vk) => (GetAsyncKeyState(vk) & 0x8000) != 0;

        bool IsVisible(Vector3 from, Vector3 to, IntPtr targetHeadPart, IntPtr localChar)
        {
            const int BIT_CANQUERY = 0x20;

            Vector3 delta = to - from;
            float totalLen = delta.Length();
            if (totalLen < 0.5f) return true;

            var candidates = new List<IntPtr>();
            CollectCandidates(Roblox.Workspace, from, totalLen,
                              candidates, 0, 3,
                              targetHeadPart, localChar);

            if (candidates.Count == 0) return true;

            int samples = Math.Max(4, Math.Min(64, VisCheckSamples));
            const float startT = 0.08f;
            const float endT = 0.94f;

            for (int s = 1; s <= samples; s++)
            {
                float t = startT + (endT - startT) * (s / (float)samples);
                Vector3 point = from + delta * t;

                for (int i = 0; i < candidates.Count; i++)
                {
                    IntPtr part = candidates[i];
                    if (!Mem.IsHeap(part)) continue;

                    IntPtr prim = Roblox.Primitive(part);
                    if (!Mem.IsHeap(prim)) continue;

                    byte flags = Mem.ReadU8(Mem.Add(prim, Offsets.Primitive.Flags));
                    if ((flags & BIT_CANQUERY) == 0) continue;

                    if (!Mem.ReadPrimitive(prim, out Vector3 pos, out Vector3 size, out _))
                        continue;
                    if (size.X <= 0 || size.Y <= 0 || size.Z <= 0) continue;

                    if (PointInsideOrientedBox(point, pos, size, prim))
                        return false;
                }
            }

            return true;
        }

        static bool PointInsideOrientedBox(Vector3 point, Vector3 center,
                                           Vector3 size, IntPtr prim)
        {
            byte[] mat = new byte[36];
            if (!Mem.ReadBytes(Mem.Add(prim, Offsets.Primitive.Rotation), mat, 36))
                return false;

            float m00 = BitConverter.ToSingle(mat, 0);
            float m10 = BitConverter.ToSingle(mat, 4);
            float m20 = BitConverter.ToSingle(mat, 8);
            float m01 = BitConverter.ToSingle(mat, 12);
            float m11 = BitConverter.ToSingle(mat, 16);
            float m21 = BitConverter.ToSingle(mat, 20);
            float m02 = BitConverter.ToSingle(mat, 24);
            float m12 = BitConverter.ToSingle(mat, 28);
            float m22 = BitConverter.ToSingle(mat, 32);

            float rx = point.X - center.X;
            float ry = point.Y - center.Y;
            float rz = point.Z - center.Z;

            float lx = rx * m00 + ry * m10 + rz * m20;
            float ly = rx * m01 + ry * m11 + rz * m21;
            float lz = rx * m02 + ry * m12 + rz * m22;

            float hx = size.X * 0.5f;
            float hy = size.Y * 0.5f;
            float hz = size.Z * 0.5f;

            const float EPS = 0.02f;
            return Math.Abs(lx) <= hx + EPS
                && Math.Abs(ly) <= hy + EPS
                && Math.Abs(lz) <= hz + EPS;
        }

        void CollectCandidates(IntPtr root, Vector3 origin, float maxDist,
                               List<IntPtr> outList, int depth, int maxDepth,
                               IntPtr targetHeadPart, IntPtr localChar)
        {
            if (depth > maxDepth) return;
            if (!Mem.IsHeap(root)) return;

            if (root == targetHeadPart) return;
            if (root == localChar) return;

            foreach (IntPtr child in Mem.GetChildren(root))
            {
                if (!Mem.IsHeap(child)) continue;

                if (child == localChar) continue;
                if (IsUnder(child, localChar)) continue;
                if (IsUnder(child, targetHeadPart)) continue;

                IntPtr prim = Roblox.Primitive(child);
                if (Mem.IsHeap(prim))
                {
                    Vector3 ppos = Mem.ReadVec3(Mem.Add(prim, Offsets.Primitive.Position));
                    if (ppos.X != 0 || ppos.Y != 0 || ppos.Z != 0)
                    {
                        Vector3 d = ppos - origin;
                        if (d.Length() <= maxDist + 20f)
                            outList.Add(child);
                    }
                }

                CollectCandidates(child, origin, maxDist, outList,
                                  depth + 1, maxDepth,
                                  targetHeadPart, localChar);
            }
        }

        static bool IsUnder(IntPtr descendant, IntPtr ancestor)
        {
            if (!Mem.IsHeap(descendant) || !Mem.IsHeap(ancestor)) return false;

            IntPtr current = descendant;
            for (int i = 0; i < 5; i++)
            {
                if (current == ancestor) return true;
                IntPtr parent = Mem.ReadPtr(Mem.Add(current, Offsets.Instance.Parent));
                if (!Mem.IsHeap(parent)) return false;
                current = parent;
            }
            return false;
        }

        private static void NormalizeFov(ref float fov)
        {
            if (fov >= 3.5f && fov <= 200f) fov = fov * (float)Math.PI / 180f;
            else if (fov <= 0.05f || fov > 3.2f) fov = 70f * (float)Math.PI / 180f;
        }

        private static Rectangle GetRobloxViewport()
        {
            IntPtr hwnd = Mem.RobloxHwnd;
            if (hwnd != IntPtr.Zero && GetClientRect(hwnd, out RECT rc))
            {
                var tl = new POINT { X = rc.Left, Y = rc.Top };
                var br = new POINT { X = rc.Right, Y = rc.Bottom };
                ClientToScreen(hwnd, ref tl);
                ClientToScreen(hwnd, ref br);
                if (br.X - tl.X > 0 && br.Y - tl.Y > 0)
                    return Rectangle.FromLTRB(tl.X, tl.Y, br.X, br.Y);
            }
            return Screen.PrimaryScreen.Bounds;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public IntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct INPUT
        {
            public uint type;
            public MOUSEINPUT mi;
        }

        [DllImport("user32.dll")]
        private static extern uint SendInput(uint n, INPUT[] p, int cb);

        [DllImport("user32.dll")]
        private static extern bool GetClientRect(IntPtr h, out RECT r);

        [DllImport("user32.dll")]
        private static extern bool ClientToScreen(IntPtr h, ref POINT p);

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT { public int Left, Top, Right, Bottom; }

        [StructLayout(LayoutKind.Sequential)]
        private struct POINT { public int X, Y; }

        private const uint INPUT_MOUSE = 0;
        private const uint LEFTDOWN = 0x0002, LEFTUP = 0x0004;
        private const uint RIGHTDOWN = 0x0008, RIGHTUP = 0x0010;
        private const uint MIDDLEDOWN = 0x0020, MIDDLEUP = 0x0040;
        private const uint XDOWN = 0x0080, XUP = 0x0100;

        private static void SendMouseClick(int key)
        {
            uint downFlag, upFlag;
            uint xButtonData = 0;

            switch (key)
            {
                case (int)VirtualKeys.RightMouse:
                    downFlag = RIGHTDOWN; upFlag = RIGHTUP; break;
                case (int)VirtualKeys.MiddleMouse:
                    downFlag = MIDDLEDOWN; upFlag = MIDDLEUP; break;
                case (int)VirtualKeys.Mouse4:
                    downFlag = XDOWN; upFlag = XUP; xButtonData = 1; break;
                case (int)VirtualKeys.Mouse5:
                    downFlag = XDOWN; upFlag = XUP; xButtonData = 2; break;
                case (int)VirtualKeys.LeftMouse:
                default:
                    downFlag = LEFTDOWN; upFlag = LEFTUP; break;
            }

            var down = new INPUT[1];
            down[0].type = INPUT_MOUSE;
            down[0].mi.dwFlags = downFlag;
            down[0].mi.mouseData = xButtonData;
            SendInput(1, down, Marshal.SizeOf(typeof(INPUT)));

            System.Threading.Thread.Sleep(8);

            var up = new INPUT[1];
            up[0].type = INPUT_MOUSE;
            up[0].mi.dwFlags = upFlag;
            up[0].mi.mouseData = xButtonData;
            SendInput(1, up, Marshal.SizeOf(typeof(INPUT)));
        }
    }
}