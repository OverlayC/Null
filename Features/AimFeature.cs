using System;

namespace NullEx
{
    public sealed class AimFeature : Feature
    {
        public override string Name => "Aim";
        public override string Description => "Lock camera onto closest player";
        protected override int UpdateIntervalMs => 8;

        public string TargetName { get; set; } = "";

        public AimFeature()
        {
            ToggleKey = 0x52;
            Set("Fov", 180);
            Set("Smoothing", 0f);
            Set("MaxDistance", 0f);
            Set("ClosestToCrosshair", false);
        }

        public int Fov { get => Get<int>("Fov"); set => Set("Fov", value); }
        public float Smoothing { get => Get<float>("Smoothing"); set => Set("Smoothing", value); }
        public float MaxDistance { get => Get<float>("MaxDistance"); set => Set("MaxDistance", value); }
        public bool ClosestToCrosshair { get => Get<bool>("ClosestToCrosshair"); set => Set("ClosestToCrosshair", value); }

        protected override void OnEnable() => Log("aim on");
        protected override void OnDisable() => Log("aim off");

        protected override void OnUpdate()
        {
            IntPtr camera = Roblox.Camera();
            if (!Mem.IsHeap(camera)) return;

            IntPtr character = Roblox.LocalCharacter();
            if (!Mem.IsHeap(character)) return;

            Vector3 eye = Roblox.CharacterPosition(character);
            if (eye.X == 0 && eye.Y == 0 && eye.Z == 0) return;

            if (!Roblox.ReadCameraFull(camera, out _, out _, out _, out Vector3 camFwd, out _))
                return;

            IntPtr target = PickTarget(eye, camFwd);
            if (!Mem.IsHeap(target)) return;

            Vector3 targetPos = Roblox.CharacterPosition(target);
            if (targetPos.X == 0 && targetPos.Y == 0 && targetPos.Z == 0) return;

            Vector3 fwd = (targetPos - eye).Normalized();

            if (Smoothing > 0.001f)
            {
                float t = Math.Max(0f, Math.Min(1f, Smoothing));
                fwd = new Vector3(
                    camFwd.X + (fwd.X - camFwd.X) * (1f - t),
                    camFwd.Y + (fwd.Y - camFwd.Y) * (1f - t),
                    camFwd.Z + (fwd.Z - camFwd.Z) * (1f - t)).Normalized();
            }

            Vector3 worldUp = new Vector3(0, 1, 0);
            Vector3 right = Vector3.Cross(fwd, worldUp).Normalized();
            Vector3 up = Vector3.Cross(right, fwd);
            Vector3 back = -fwd;

            float[] m = Roblox.BuildCameraMatrix(right, up, back);
            Mem.WriteVec3(Mem.Add(camera, Offsets.Camera.Position), eye);
            Mem.WriteFloats(Mem.Add(camera, Offsets.Camera.Rotation), m);
        }

        private IntPtr PickTarget(Vector3 eye, Vector3 camFwd)
        {
            string myName = Roblox.LocalName;
            IntPtr best = IntPtr.Zero;
            float bestScore = float.MaxValue;

            foreach (IntPtr p in Mem.GetChildren(Roblox.Players))
            {
                string nm = Mem.ReadInstanceName(p);
                if (string.IsNullOrEmpty(nm) || nm == myName) continue;
                if (!string.IsNullOrEmpty(TargetName) &&
                    !nm.Equals(TargetName, StringComparison.OrdinalIgnoreCase)) continue;

                IntPtr ch = Roblox.Character(p);
                if (!Mem.IsHeap(ch)) continue;

                Vector3 pos = Roblox.CharacterPosition(ch);
                if (pos.X == 0 && pos.Y == 0 && pos.Z == 0) continue;

                float dist = (pos - eye).Length();
                if (MaxDistance > 0f && dist > MaxDistance) continue;

                if (Fov > 0 && Fov < 360)
                {
                    float dot = Vector3.Dot(camFwd, (pos - eye).Normalized());
                    float angle = (float)(Math.Acos(Math.Max(-1f, Math.Min(1f, dot))) * 180.0 / Math.PI);
                    if (angle > Fov / 2f) continue;
                }

                float score = ClosestToCrosshair
                    ? (float)(Math.Acos(Math.Max(-1f, Math.Min(1f,
                        Vector3.Dot(camFwd, (pos - eye).Normalized())))) * 180.0 / Math.PI)
                    : dist;

                if (score < bestScore) { bestScore = score; best = ch; }
                if (!string.IsNullOrEmpty(TargetName)) break;
            }
            return best;
        }
    }
}