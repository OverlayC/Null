using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;

namespace NullEx
{
    public sealed class FlyFeature : Feature
    {
        public override string Name => "Fly";
        public override string Description => "Free movement + noclip";
        protected override int UpdateIntervalMs => 0;

        public FlyFeature()
        {
            ToggleKey = 0;
            Set("Speed", 50f);
            Set("TickMs", 15);
            Set("Noclip", false);
            Set("NoclipRadius", 40);
            Set("VerticalDown", true);
            Set("VerticalUp", false);
        }

        public float Speed { get => Get<float>("Speed"); set => Set("Speed", value); }
        public int TickMs { get => Get<int>("TickMs"); set => Set("TickMs", value); }
        public bool Noclip { get => Get<bool>("Noclip"); set => Set("Noclip", value); }
        public int NoclipRadius { get => Get<int>("NoclipRadius"); set => Set("NoclipRadius", value); }
        public bool VerticalDown { get => Get<bool>("VerticalDown"); set => Set("VerticalDown", value); }
        public bool VerticalUp { get => Get<bool>("VerticalUp"); set => Set("VerticalUp", value); }

        private IntPtr _gravAddr = IntPtr.Zero;
        private float _savedGravity = 196.2f;
        private bool _gravitySaved;

        class TouchedPrim
        {
            public IntPtr Prim;
            public byte OriginalFlags;
        }
        readonly Dictionary<IntPtr, TouchedPrim> _touched = new Dictionary<IntPtr, TouchedPrim>();

        const int BIT_CANCOLLIDE = 0x08;
        const int BIT_CANTOUCH = 0x10;
        const int BIT_CANQUERY = 0x20;
        const byte NOCLIP_MASK = unchecked((byte)~(BIT_CANCOLLIDE | BIT_CANTOUCH | BIT_CANQUERY));

        protected override void OnEnable()
        {
            IntPtr world = Mem.ReadPtr(Mem.Add(Roblox.Workspace, Offsets.Workspace.World));
            if (Mem.IsHeap(world))
            {
                _gravAddr = Mem.Add(world, Offsets.World.Gravity);
                if (!_gravitySaved)
                {
                    float g = Mem.ReadF32(_gravAddr);
                    if (g > 0.01f && g < 10000f) _savedGravity = g;
                    _gravitySaved = true;
                }
                Mem.WriteF32(_gravAddr, 0f);
            }
        }

        protected override void OnDisable()
        {
            if (Mem.IsHeap(_gravAddr) && _gravitySaved)
                Mem.WriteF32(_gravAddr, _savedGravity);

            RestoreCollision();
        }

        protected override void OnUpdate()
        {
            IntPtr camera = Roblox.Camera();
            if (!Mem.IsHeap(camera)) { Sleep(TickMs); return; }

            IntPtr character = Roblox.LocalCharacter();
            if (!Mem.IsHeap(character)) { Sleep(TickMs); return; }

            IntPtr hrp = Roblox.FindChildByName(character, "HumanoidRootPart");
            if (!Mem.IsHeap(hrp)) { Sleep(TickMs); return; }

            IntPtr prim = Roblox.Primitive(hrp);
            if (!Mem.IsHeap(prim)) { Sleep(TickMs); return; }

            if (Mem.IsHeap(_gravAddr)) Mem.WriteF32(_gravAddr, 0f);

            if (Noclip)
            {
                StripFlagsRecursive(character);

                StripNearbyWorldPrimitives(prim);
            }

            if (!Roblox.ReadCameraFull(camera, out _, out Vector3 camRight, out _, out Vector3 camFwd, out _))
            { Sleep(TickMs); return; }

            float speed = Speed;
            Vector3 dir = new Vector3();

            if (Key(VK.W)) dir = dir + camFwd;
            if (Key(VK.S)) dir = dir - camFwd;
            if (Key(VK.D)) dir = dir + camRight;
            if (Key(VK.A)) dir = dir - camRight;

            if (VerticalDown && Key(VK.CTRL)) dir.Y -= 1f;
            if (VerticalUp && Key(VK.SPACE)) dir.Y += 1f;

            if (Key(VK.SHIFT)) speed *= 2.5f;

            Vector3 finalVel = (dir.X == 0 && dir.Y == 0 && dir.Z == 0)
                ? new Vector3()
                : dir.Normalized() * speed;

            Mem.WriteVec3(Mem.Add(prim, Offsets.Primitive.AssemblyLinearVelocity), finalVel);

            Sleep(TickMs);
        }

        void StripFlagsRecursive(IntPtr instance)
        {
            if (!Mem.IsHeap(instance)) return;

            StripOnePrimitive(instance);

            foreach (IntPtr child in Mem.GetChildren(instance))
                StripFlagsRecursive(child);
        }

        void StripOnePrimitive(IntPtr instance)
        {
            IntPtr prim = Roblox.Primitive(instance);
            if (!Mem.IsHeap(prim)) return;

            IntPtr flagsAddr = Mem.Add(prim, Offsets.Primitive.Flags);
            byte flags = Mem.ReadU8(flagsAddr);
            byte stripped = (byte)(flags & NOCLIP_MASK);

            if (stripped == flags) return;

            if (!_touched.ContainsKey(prim))
                _touched[prim] = new TouchedPrim { Prim = prim, OriginalFlags = flags };

            Mem.WriteU8(flagsAddr, stripped);
        }

        void StripNearbyWorldPrimitives(IntPtr myPrim)
        {
            if (!Mem.IsHeap(Roblox.Workspace)) return;

            Vector3 myPos = Mem.ReadVec3(Mem.Add(myPrim, Offsets.Primitive.Position));
            if (myPos.X == 0 && myPos.Y == 0 && myPos.Z == 0) return;

            float r = NoclipRadius;
            float r2 = r * r;

            foreach (IntPtr topChild in Mem.GetChildren(Roblox.Workspace))
            {
                StripNearbyRecursive(topChild, myPos, r2, 0);
            }
        }

        void StripNearbyRecursive(IntPtr instance, Vector3 myPos, float r2, int depth)
        {
            if (depth > 3) return;
            if (!Mem.IsHeap(instance)) return;

            IntPtr prim = Roblox.Primitive(instance);
            if (Mem.IsHeap(prim))
            {
                Vector3 pos = Mem.ReadVec3(Mem.Add(prim, Offsets.Primitive.Position));
                if (pos.X != 0 || pos.Y != 0 || pos.Z != 0)
                {
                    float dx = pos.X - myPos.X;
                    float dy = pos.Y - myPos.Y;
                    float dz = pos.Z - myPos.Z;
                    float d2 = dx * dx + dy * dy + dz * dz;
                    if (d2 <= r2)
                    {
                        StripOnePrimitive(instance);
                    }
                }
            }

            var children = Mem.GetChildren(instance);
            if (children.Count == 0) return;

            foreach (IntPtr child in children)
                StripNearbyRecursive(child, myPos, r2, depth + 1);
        }

        void RestoreCollision()
        {
            foreach (var kv in _touched)
            {
                var t = kv.Value;
                if (!Mem.IsHeap(t.Prim)) continue;
                IntPtr flagsAddr = Mem.Add(t.Prim, Offsets.Primitive.Flags);
                byte current = Mem.ReadU8(flagsAddr);
                byte desired = (byte)(current | t.OriginalFlags);
                if (current != desired) Mem.WriteU8(flagsAddr, desired);
            }
            _touched.Clear();
        }

        private static void Sleep(int ms) => Thread.Sleep(Math.Max(1, ms));

        [DllImport("user32.dll")]
        private static extern short GetAsyncKeyState(int vk);
        private static bool Key(int vk) => (GetAsyncKeyState(vk) & 0x8000) != 0;

        private static class VK
        {
            public const int W = 0x57;
            public const int A = 0x41;
            public const int S = 0x53;
            public const int D = 0x44;
            public const int CTRL = 0x11;
            public const int SHIFT = 0x10;
            public const int SPACE = 0x20;
        }
    }
}