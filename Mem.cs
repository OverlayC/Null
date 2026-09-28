using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace NullEx
{
    public static class Mem
    {
        [DllImport("kernel32.dll")]
        static extern IntPtr OpenProcess(int access, bool inherit, int pid);

        [DllImport("kernel32.dll")]
        static extern bool ReadProcessMemory(IntPtr h, IntPtr addr, byte[] buf, int size, out int read);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool WriteProcessMemory(IntPtr h, IntPtr addr, byte[] buf, int size, out int written);

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool VirtualProtectEx(IntPtr h, IntPtr addr, int size, uint newProtect, out uint old);

        [DllImport("kernel32.dll")]
        static extern bool CloseHandle(IntPtr h);

        [DllImport("user32.dll")]
        static extern bool EnumWindows(EnumWindowsProc cb, IntPtr lParam);
        delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
        [DllImport("user32.dll")]
        static extern uint GetWindowThreadProcessId(IntPtr hWnd, out int pid);
        [DllImport("user32.dll")]
        static extern bool IsWindowVisible(IntPtr hWnd);

        const int PROCESS_VM_READ = 0x0010;
        const int PROCESS_VM_WRITE = 0x0020;
        const int PROCESS_VM_OPERATION = 0x0008;
        const int PROCESS_QUERY_INFORMATION = 0x0400;
        const uint PAGE_READWRITE = 0x04;

        public static IntPtr Handle { get; private set; } = IntPtr.Zero;
        public static IntPtr BaseAddr { get; private set; } = IntPtr.Zero;
        public static int ProcessId { get; private set; } = 0;
        public static IntPtr RobloxHwnd { get; private set; } = IntPtr.Zero;

        public static bool IsAttached => Handle != IntPtr.Zero;

        public static bool Attach()
        {
            Process p = FindRoblox();
            if (p == null) return false;

            Handle = OpenProcess(
                PROCESS_VM_READ | PROCESS_VM_WRITE | PROCESS_VM_OPERATION | PROCESS_QUERY_INFORMATION,
                false, p.Id);
            if (Handle == IntPtr.Zero) return false;

            ProcessId = p.Id;
            BaseAddr = GetModuleBase(p);
            RobloxHwnd = FindWindowForProcess(p.Id);

            return BaseAddr != IntPtr.Zero;
        }

        public static void Detach()
        {
            if (Handle != IntPtr.Zero)
            {
                try { CloseHandle(Handle); } catch { }
                Handle = IntPtr.Zero;
            }
        }


        public static IntPtr ReadPtr(IntPtr addr)
        {
            byte[] buf = new byte[8];
            if (ReadProcessMemory(Handle, addr, buf, 8, out int n) && n == 8)
                return new IntPtr(BitConverter.ToInt64(buf, 0));
            return IntPtr.Zero;
        }

        public static long ReadI64(IntPtr addr)
        {
            byte[] buf = new byte[8];
            if (ReadProcessMemory(Handle, addr, buf, 8, out int n) && n == 8)
                return BitConverter.ToInt64(buf, 0);
            return 0;
        }

        public static int ReadI32(IntPtr addr)
        {
            byte[] buf = new byte[4];
            if (ReadProcessMemory(Handle, addr, buf, 4, out int n) && n == 4)
                return BitConverter.ToInt32(buf, 0);
            return 0;
        }

        public static float ReadF32(IntPtr addr)
        {
            byte[] buf = new byte[4];
            if (ReadProcessMemory(Handle, addr, buf, 4, out int n) && n == 4)
                return BitConverter.ToSingle(buf, 0);
            return 0f;
        }

        public static bool ReadBytes(IntPtr addr, byte[] buf, int size)
            => ReadProcessMemory(Handle, addr, buf, size, out int n) && n == size;

        public static Vector3 ReadVec3(IntPtr addr)
        {
            byte[] buf = new byte[12];
            if (ReadProcessMemory(Handle, addr, buf, 12, out int n) && n == 12)
                return new Vector3(
                    BitConverter.ToSingle(buf, 0),
                    BitConverter.ToSingle(buf, 4),
                    BitConverter.ToSingle(buf, 8));
            return new Vector3();
        }

        public static byte ReadU8(IntPtr addr)
        {
            byte[] buf = new byte[1];
            if (ReadProcessMemory(Handle, addr, buf, 1, out int n) && n == 1)
                return buf[0];
            return 0;
        }

        public static bool WriteU8(IntPtr addr, byte value)
        {
            byte[] buf = { value };
            return WriteBytes(addr, buf);
        }


        public static bool ReadBlock(IntPtr addr, byte[] buffer, int size)
        {
            if (buffer == null || buffer.Length < size) return false;
            return ReadProcessMemory(Handle, addr, buffer, size, out int n) && n == size;
        }

        public static bool ReadPrimitive(IntPtr primitivePtr,
                                         out Vector3 position,
                                         out Vector3 size,
                                         out Vector3 velocity)
        {
            position = new Vector3();
            size = new Vector3();
            velocity = new Vector3();

            if (!IsHeap(primitivePtr)) return false;

            const int offsetStart = 0xb0;
            const int offsetEnd = 0x1c8;
            byte[] buf = new byte[offsetEnd - offsetStart];

            if (!ReadBlock(Add(primitivePtr, offsetStart), buf, buf.Length)) return false;

            int posOff = (int)Offsets.Primitive.Position - offsetStart;
            int sizeOff = (int)Offsets.Primitive.Size - offsetStart;
            int velOff = (int)Offsets.Primitive.AssemblyLinearVelocity - offsetStart;

            position = new Vector3(
                BitConverter.ToSingle(buf, posOff),
                BitConverter.ToSingle(buf, posOff + 4),
                BitConverter.ToSingle(buf, posOff + 8));

            size = new Vector3(
                BitConverter.ToSingle(buf, sizeOff),
                BitConverter.ToSingle(buf, sizeOff + 4),
                BitConverter.ToSingle(buf, sizeOff + 8));

            velocity = new Vector3(
                BitConverter.ToSingle(buf, velOff),
                BitConverter.ToSingle(buf, velOff + 4),
                BitConverter.ToSingle(buf, velOff + 8));

            return true;
        }


        public static bool WriteBytes(IntPtr addr, byte[] data)
        {
            VirtualProtectEx(Handle, addr, data.Length, PAGE_READWRITE, out uint old);
            bool ok = WriteProcessMemory(Handle, addr, data, data.Length, out int written) && written == data.Length;
            VirtualProtectEx(Handle, addr, data.Length, old, out _);
            return ok;
        }

        public static bool WriteF32(IntPtr addr, float v)
            => WriteBytes(addr, BitConverter.GetBytes(v));

        public static bool WriteVec3(IntPtr addr, Vector3 v)
        {
            byte[] buf = new byte[12];
            BitConverter.GetBytes(v.X).CopyTo(buf, 0);
            BitConverter.GetBytes(v.Y).CopyTo(buf, 4);
            BitConverter.GetBytes(v.Z).CopyTo(buf, 8);
            return WriteBytes(addr, buf);
        }

        public static bool WriteFloats(IntPtr addr, float[] values)
        {
            byte[] buf = new byte[values.Length * 4];
            for (int i = 0; i < values.Length; i++)
                BitConverter.GetBytes(values[i]).CopyTo(buf, i * 4);
            return WriteBytes(addr, buf);
        }


        public static IntPtr Add(IntPtr p, long offset) => new IntPtr(p.ToInt64() + offset);

        public static bool IsHeap(IntPtr p)
        {
            long v = p.ToInt64();
            return v > 0x10000000000L && v < 0x7F0000000000L;
        }


        public static List<IntPtr> GetChildren(IntPtr parent)
        {
            var result = new List<IntPtr>();
            if (!IsHeap(parent)) return result;

            IntPtr head = ReadPtr(Add(parent, Offsets.Instance.ChildrenStart));
            if (!IsHeap(head)) return result;

            IntPtr begin = ReadPtr(head);
            IntPtr end = ReadPtr(Add(head, 0x08));
            if (!IsHeap(begin) || !IsHeap(end)) return result;

            long span = end.ToInt64() - begin.ToInt64();
            if (span <= 0 || span > 0x100000) return result;

            int count = (int)(span / 0x10);
            for (int i = 0; i < count; i++)
            {
                IntPtr child = ReadPtr(Add(begin, i * 0x10));
                if (IsHeap(child)) result.Add(child);
            }
            return result;
        }


        public static string ReadString(IntPtr addr, int maxLen = 256)
        {
            int size = ReadI32(Add(addr, Offsets.Misc.StringLength));
            if (size > 0 && size <= maxLen)
            {
                IntPtr ptr = ReadPtr(addr);
                if (IsHeap(ptr))
                {
                    byte[] buf = new byte[size];
                    if (ReadProcessMemory(Handle, ptr, buf, size, out int n) && n == size)
                    {
                        string s = Encoding.UTF8.GetString(buf);
                        if (IsPrintable(s)) return s;
                    }
                }
            }

            byte[] inline = new byte[16];
            if (ReadProcessMemory(Handle, addr, inline, 16, out int got) && got > 0)
            {
                int end = Array.IndexOf(inline, (byte)0);
                if (end > 0)
                {
                    string s = Encoding.UTF8.GetString(inline, 0, end);
                    if (IsPrintable(s)) return s;
                }
            }
            return string.Empty;
        }

        public static string ReadInstanceName(IntPtr instance)
        {
            if (!IsHeap(instance)) return string.Empty;

            IntPtr nc = ReadPtr(Add(instance, Offsets.Instance.NameContainer));
            if (IsHeap(nc))
            {
                string s = ReadString(nc);
                if (IsPrintable(s) && s.Length < 64) return s;
                s = ReadString(Add(nc, 0x08));
                if (IsPrintable(s) && s.Length < 64) return s;
                s = ReadString(Add(nc, 0x10));
                if (IsPrintable(s) && s.Length < 64) return s;
            }

            string d = ReadString(Add(instance, Offsets.Instance.NameContainer));
            if (IsPrintable(d) && d.Length < 64) return d;

            d = ReadString(Add(instance, Offsets.Instance.Name));
            if (IsPrintable(d) && d.Length < 64) return d;

            return string.Empty;
        }

        public static bool IsPrintable(string s)
        {
            if (string.IsNullOrEmpty(s)) return false;
            foreach (char c in s)
                if (c < 0x20 || c > 0x7E) return false;
            return true;
        }


        static Process FindRoblox()
        {
            foreach (string name in new[] { "RobloxPlayerBeta", "RobloxPlayer", "Roblox" })
            {
                var procs = Process.GetProcessesByName(name);
                if (procs.Length > 0) return procs[0];
            }
            return null;
        }

        static IntPtr GetModuleBase(Process p)
        {
            try
            {
                foreach (ProcessModule m in p.Modules)
                    if (m.ModuleName.StartsWith("RobloxPlayer", StringComparison.OrdinalIgnoreCase))
                        return m.BaseAddress;
            }
            catch { }
            return IntPtr.Zero;
        }

        static IntPtr FindWindowForProcess(int pid)
        {
            IntPtr result = IntPtr.Zero;
            EnumWindows((hwnd, _) =>
            {
                GetWindowThreadProcessId(hwnd, out int wpid);
                if (wpid == pid && IsWindowVisible(hwnd))
                {
                    result = hwnd;
                    return false;
                }
                return true;
            }, IntPtr.Zero);
            return result;
        }
    }

    public struct Vector3
    {
        public float X, Y, Z;
        public Vector3(float x, float y, float z) { X = x; Y = y; Z = z; }

        public static Vector3 operator +(Vector3 a, Vector3 b) => new Vector3(a.X + b.X, a.Y + b.Y, a.Z + b.Z);
        public static Vector3 operator -(Vector3 a, Vector3 b) => new Vector3(a.X - b.X, a.Y - b.Y, a.Z - b.Z);
        public static Vector3 operator -(Vector3 a) => new Vector3(-a.X, -a.Y, -a.Z);
        public static Vector3 operator *(Vector3 a, float s) => new Vector3(a.X * s, a.Y * s, a.Z * s);

        public float Length() => (float)Math.Sqrt(X * X + Y * Y + Z * Z);

        public Vector3 Normalized()
        {
            float len = Length();
            if (len < 1e-6f) return new Vector3(0, 0, -1);
            return new Vector3(X / len, Y / len, Z / len);
        }

        public static float Dot(Vector3 a, Vector3 b) => a.X * b.X + a.Y * b.Y + a.Z * b.Z;

        public static Vector3 Cross(Vector3 a, Vector3 b) => new Vector3(
            a.Y * b.Z - a.Z * b.Y,
            a.Z * b.X - a.X * b.Z,
            a.X * b.Y - a.Y * b.X);

        public override string ToString() => $"({X:F2}, {Y:F2}, {Z:F2})";
    }
}