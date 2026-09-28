using System;
using System.Collections.Generic;

namespace NullEx
{
    public static class Roblox
    {
        public static IntPtr DataModel { get; private set; } = IntPtr.Zero;
        public static IntPtr Players { get; private set; } = IntPtr.Zero;
        public static IntPtr Workspace { get; private set; } = IntPtr.Zero;
        public static IntPtr LocalPlayer { get; private set; } = IntPtr.Zero;
        public static string LocalName { get; private set; } = "";

        static readonly byte[] _playerBuf = new byte[0x400];
        static readonly byte[] _cameraBuf = new byte[0x100];

        public static bool Resolve()
        {
            DataModel = Players = Workspace = LocalPlayer = IntPtr.Zero;
            LocalName = "";

            DataModel = FindDataModel();
            if (!Mem.IsHeap(DataModel)) return false;

            foreach (IntPtr child in Mem.GetChildren(DataModel))
            {
                string name = Mem.ReadInstanceName(child);
                if (Players == IntPtr.Zero && name == "Players") Players = child;
                if (Workspace == IntPtr.Zero && name == "Workspace") Workspace = child;
                if (Players != IntPtr.Zero && Workspace != IntPtr.Zero) break;
            }

            if (!Mem.IsHeap(Players)) Players = FindPlayersHeuristic();
            if (!Mem.IsHeap(Workspace)) Workspace = FindWorkspaceHeuristic();
            if (!Mem.IsHeap(Players)) return false;

            LocalPlayer = Mem.ReadPtr(Mem.Add(Players, Offsets.Player.LocalPlayer));
            if (!Mem.IsHeap(LocalPlayer)) LocalPlayer = FindLocalPlayerHeuristic();
            if (!Mem.IsHeap(LocalPlayer)) return false;

            LocalName = Mem.ReadInstanceName(LocalPlayer);
            EspBuilder.InvalidateCaches();
            return true;
        }

        static IntPtr FindDataModel()
        {
            IntPtr fake = Mem.ReadPtr(Mem.Add(Mem.BaseAddr, Offsets.FakeDataModel.Pointer));
            if (Mem.IsHeap(fake))
            {
                IntPtr dm = Mem.ReadPtr(Mem.Add(fake, Offsets.FakeDataModel.RealDataModel));
                if (Mem.IsHeap(dm) && LooksLikeDataModel(dm)) return dm;

                foreach (long off in new long[] { 0x1f0, 0x200, 0x1e8, 0x1e0 })
                {
                    IntPtr alt = Mem.ReadPtr(Mem.Add(fake, off));
                    if (Mem.IsHeap(alt) && LooksLikeDataModel(alt)) return alt;
                }
            }

            IntPtr ve = Mem.ReadPtr(Mem.Add(Mem.BaseAddr, Offsets.VisualEngine.Pointer));
            if (Mem.IsHeap(ve))
            {
                IntPtr vf = Mem.ReadPtr(Mem.Add(ve, Offsets.VisualEngine.FakeDataModel));
                if (Mem.IsHeap(vf))
                {
                    IntPtr dm = Mem.ReadPtr(Mem.Add(vf, Offsets.FakeDataModel.RealDataModel));
                    if (Mem.IsHeap(dm) && LooksLikeDataModel(dm)) return dm;

                    foreach (long off in new long[] { 0x1f0, 0x200, 0x1e8, 0x1e0 })
                    {
                        IntPtr alt = Mem.ReadPtr(Mem.Add(vf, off));
                        if (Mem.IsHeap(alt) && LooksLikeDataModel(alt)) return alt;
                    }
                }
            }

            for (long off = 0x8000000; off < 0x9500000; off += 8)
            {
                IntPtr cand = Mem.ReadPtr(Mem.Add(Mem.BaseAddr, off));
                if (Mem.IsHeap(cand) && LooksLikeDataModel(cand)) return cand;
            }

            return IntPtr.Zero;
        }

        static bool LooksLikeDataModel(IntPtr p)
        {
            if (!Mem.IsHeap(p)) return false;
            var children = Mem.GetChildren(p);
            if (children.Count < 5) return false;

            int hits = 0;
            foreach (IntPtr c in children)
            {
                string n = Mem.ReadInstanceName(c);
                if (n == "Workspace" || n == "Players" || n == "Lighting" ||
                    n == "ReplicatedStorage" || n == "ReplicatedFirst")
                    hits++;
                if (hits >= 2) return true;
            }
            return children.Count >= 20;
        }

        static IntPtr FindPlayersHeuristic()
        {
            IntPtr best = IntPtr.Zero;
            int bestCount = 0;

            foreach (IntPtr c in Mem.GetChildren(DataModel))
            {
                var subs = Mem.GetChildren(c);
                if (subs.Count == 0) continue;

                int playerCount = 0;
                foreach (IntPtr s in subs)
                {
                    long uid = Mem.ReadI64(Mem.Add(s, Offsets.Player.UserId));
                    if (uid > 0 && uid < 1_000_000_000_000L) playerCount++;
                }
                if (playerCount > bestCount) { bestCount = playerCount; best = c; }
            }
            return bestCount > 0 ? best : IntPtr.Zero;
        }

        static IntPtr FindWorkspaceHeuristic()
        {
            foreach (IntPtr c in Mem.GetChildren(DataModel))
            {
                IntPtr cam = Mem.ReadPtr(Mem.Add(c, Offsets.Workspace.CurrentCamera));
                if (!Mem.IsHeap(cam)) continue;

                Vector3 pos = Mem.ReadVec3(Mem.Add(cam, Offsets.Camera.Position));
                if (pos.X != 0 || pos.Y != 0 || pos.Z != 0) return c;
            }
            return IntPtr.Zero;
        }

        static IntPtr FindLocalPlayerHeuristic()
        {
            IntPtr candidate = IntPtr.Zero;
            int withChar = 0;
            int total = 0;

            foreach (IntPtr p in Mem.GetChildren(Players))
            {
                total++;
                IntPtr model = Mem.ReadPtr(Mem.Add(p, Offsets.Player.ModelInstance));
                if (Mem.IsHeap(model)) { candidate = p; withChar++; }
            }

            if (withChar == 1) return candidate;
            if (total == 1)
                foreach (IntPtr p in Mem.GetChildren(Players)) return p;

            return IntPtr.Zero;
        }

        public struct PlayerInfo
        {
            public long UserId;
            public IntPtr ModelInstance;
            public IntPtr Team;
        }

        public static bool ReadPlayerInfo(IntPtr player, out PlayerInfo info)
        {
            info = new PlayerInfo();
            if (!Mem.IsHeap(player)) return false;
            if (!Mem.ReadBlock(player, _playerBuf, _playerBuf.Length)) return false;

            int uidOff = (int)Offsets.Player.UserId;
            int modelOff = (int)Offsets.Player.ModelInstance;
            int teamOff = (int)Offsets.Player.Team;

            info.UserId = BitConverter.ToInt64(_playerBuf, uidOff);
            info.ModelInstance = new IntPtr(BitConverter.ToInt64(_playerBuf, modelOff));
            info.Team = new IntPtr(BitConverter.ToInt64(_playerBuf, teamOff));
            return true;
        }

        public static IntPtr Character(IntPtr player)
        {
            string name = Mem.ReadInstanceName(player);
            if (string.IsNullOrEmpty(name)) return IntPtr.Zero;

            IntPtr model = Mem.ReadPtr(Mem.Add(player, Offsets.Player.ModelInstance));
            if (Mem.IsHeap(model) && Mem.ReadInstanceName(model) == name) return model;

            if (Mem.IsHeap(Workspace))
            {
                foreach (IntPtr c in Mem.GetChildren(Workspace))
                {
                    if (Mem.ReadInstanceName(c) == name) return c;
                    foreach (IntPtr gc in Mem.GetChildren(c))
                        if (Mem.ReadInstanceName(gc) == name) return gc;
                }
            }
            return Mem.IsHeap(model) ? model : IntPtr.Zero;
        }

        public static IntPtr LocalCharacter() => Character(LocalPlayer);

        public static IntPtr FindChildByName(IntPtr parent, string name)
        {
            foreach (IntPtr c in Mem.GetChildren(parent))
                if (Mem.ReadInstanceName(c) == name) return c;
            return IntPtr.Zero;
        }

        public static IntPtr Humanoid(IntPtr character) => FindChildByName(character, "Humanoid");

        public static IntPtr RootPart(IntPtr humanoid)
        {
            IntPtr root = Mem.ReadPtr(Mem.Add(humanoid, Offsets.Humanoid.HumanoidRootPart));
            if (Mem.IsHeap(root)) return root;

            foreach (long off in new long[] { 0x120, 0x118, 0x128, 0x100, 0x108, 0x110, 0x458, 0x450, 0x460 })
            {
                IntPtr alt = Mem.ReadPtr(Mem.Add(humanoid, off));
                if (Mem.IsHeap(alt) && Mem.ReadInstanceName(alt) == "HumanoidRootPart") return alt;
            }
            return IntPtr.Zero;
        }

        public static IntPtr Primitive(IntPtr part)
            => Mem.ReadPtr(Mem.Add(part, Offsets.BasePart.Primitive));

        public static Vector3 PartPosition(IntPtr part)
        {
            IntPtr prim = Primitive(part);
            if (!Mem.IsHeap(prim)) return new Vector3();

            if (Mem.ReadPrimitive(prim, out Vector3 pos, out _, out _))
            {
                if (pos.X == 0 && pos.Y == 0 && pos.Z == 0)
                {
                    Vector3 alt = Mem.ReadVec3(Mem.Add(prim, 0xE4));
                    if (alt.X != 0 || alt.Y != 0 || alt.Z != 0) return alt;
                }
                return pos;
            }

            return new Vector3();
        }

        public static void GetBoundingBox(IntPtr character, out Vector3 top, out Vector3 bottom)
        {
            top = new Vector3();
            bottom = new Vector3();

            IntPtr head = FindChildByName(character, "Head");
            if (Mem.IsHeap(head))
            {
                Vector3 hp = PartPosition(head);
                if (hp.X != 0 || hp.Y != 0 || hp.Z != 0) top = hp;
            }

            IntPtr hum = Humanoid(character);
            IntPtr root = Mem.IsHeap(hum) ? RootPart(hum) : IntPtr.Zero;
            if (Mem.IsHeap(root))
            {
                Vector3 rp = PartPosition(root);
                if (rp.X != 0 || rp.Y != 0 || rp.Z != 0) bottom = rp;
            }

            if (top.Y == 0 || bottom.Y == 0)
            {
                float hi = float.MinValue, lo = float.MaxValue;
                Vector3 hiPos = new Vector3(), loPos = new Vector3();
                bool found = false;

                foreach (IntPtr c in Mem.GetChildren(character))
                {
                    if (!Mem.IsHeap(Primitive(c))) continue;
                    Vector3 p = PartPosition(c);
                    if (p.X == 0 && p.Y == 0 && p.Z == 0) continue;
                    found = true;
                    if (p.Y > hi) { hi = p.Y; hiPos = p; }
                    if (p.Y < lo) { lo = p.Y; loPos = p; }
                }

                if (found)
                {
                    if (top.Y == 0) top = hiPos;
                    if (bottom.Y == 0) bottom = loPos;
                }
            }

            if (top.Y - bottom.Y < 2.0f)
                top = new Vector3(bottom.X, bottom.Y + 3.0f, bottom.Z);
        }

        public static Vector3 CharacterPosition(IntPtr character)
        {
            IntPtr head = FindChildByName(character, "Head");
            if (Mem.IsHeap(head))
            {
                Vector3 p = PartPosition(head);
                if (p.X != 0 || p.Y != 0 || p.Z != 0) return p;
            }

            IntPtr hum = Humanoid(character);
            if (Mem.IsHeap(hum))
            {
                IntPtr root = RootPart(hum);
                if (Mem.IsHeap(root))
                {
                    Vector3 p = PartPosition(root);
                    if (p.X != 0 || p.Y != 0 || p.Z != 0) return p;
                }
            }

            foreach (IntPtr c in Mem.GetChildren(character))
            {
                if (!Mem.IsHeap(Primitive(c))) continue;
                Vector3 p = PartPosition(c);
                if (p.X != 0 || p.Y != 0 || p.Z != 0) return p;
            }
            return new Vector3();
        }

        public static IntPtr Camera()
            => Mem.ReadPtr(Mem.Add(Workspace, Offsets.Workspace.CurrentCamera));

        public static bool ReadCameraFull(IntPtr camera,
                                          out Vector3 position,
                                          out Vector3 right,
                                          out Vector3 up,
                                          out Vector3 forward,
                                          out float fov)
        {
            position = right = up = forward = new Vector3();
            fov = 70f * (float)Math.PI / 180f;

            if (!Mem.IsHeap(camera)) return false;

            const int bufStart = 0xb0;
            if (!Mem.ReadBlock(Mem.Add(camera, bufStart), _cameraBuf, _cameraBuf.Length))
                return false;

            int rotOff = (int)Offsets.Camera.Rotation - bufStart;
            int posOff = (int)Offsets.Camera.Position - bufStart;
            int fovOff = (int)Offsets.Camera.FieldOfView - bufStart;

            right = new Vector3(
                BitConverter.ToSingle(_cameraBuf, rotOff + 0),
                BitConverter.ToSingle(_cameraBuf, rotOff + 12),
                BitConverter.ToSingle(_cameraBuf, rotOff + 24));
            up = new Vector3(
                BitConverter.ToSingle(_cameraBuf, rotOff + 4),
                BitConverter.ToSingle(_cameraBuf, rotOff + 16),
                BitConverter.ToSingle(_cameraBuf, rotOff + 28));
            Vector3 back = new Vector3(
                BitConverter.ToSingle(_cameraBuf, rotOff + 8),
                BitConverter.ToSingle(_cameraBuf, rotOff + 20),
                BitConverter.ToSingle(_cameraBuf, rotOff + 32));
            forward = -back;

            position = new Vector3(
                BitConverter.ToSingle(_cameraBuf, posOff),
                BitConverter.ToSingle(_cameraBuf, posOff + 4),
                BitConverter.ToSingle(_cameraBuf, posOff + 8));

            fov = BitConverter.ToSingle(_cameraBuf, fovOff);
            return true;
        }

        public static void CameraBasis(IntPtr camera, out Vector3 right, out Vector3 up, out Vector3 forward)
        {
            right = up = forward = new Vector3();
            ReadCameraFull(camera, out _, out right, out up, out forward, out _);
        }

        public static float[] BuildCameraMatrix(Vector3 right, Vector3 up, Vector3 back)
        {
            return new float[9] {
                right.X, up.X, back.X,
                right.Y, up.Y, back.Y,
                right.Z, up.Z, back.Z,
            };
        }
    }
}