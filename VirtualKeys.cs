using System;

namespace NullEx
{
    public enum VirtualKeys : int
    {
        None = 0x00,

        LeftMouse = 0x01,
        RightMouse = 0x02,
        MiddleMouse = 0x04,
        Mouse4 = 0x05,
        Mouse5 = 0x06,

        Backspace = 0x08,
        Tab = 0x09,
        Enter = 0x0D,
        Shift = 0x10,
        Ctrl = 0x11,
        Alt = 0x12,
        Pause = 0x13,
        CapsLock = 0x14,
        Esc = 0x1B,
        Space = 0x20,
        PageUp = 0x21,
        PageDown = 0x22,
        End = 0x23,
        Home = 0x24,
        Left = 0x25,
        Up = 0x26,
        Right = 0x27,
        Down = 0x28,
        PrintScreen = 0x2C,
        Insert = 0x2D,
        Delete = 0x2E,

        D0 = 0x30,
        D1 = 0x31,
        D2 = 0x32,
        D3 = 0x33,
        D4 = 0x34,
        D5 = 0x35,
        D6 = 0x36,
        D7 = 0x37,
        D8 = 0x38,
        D9 = 0x39,

        A = 0x41,
        B = 0x42,
        C = 0x43,
        D = 0x44,
        E = 0x45,
        F = 0x46,
        G = 0x47,
        H = 0x48,
        I = 0x49,
        J = 0x4A,
        K = 0x4B,
        L = 0x4C,
        M = 0x4D,
        N = 0x4E,
        O = 0x4F,
        P = 0x50,
        Q = 0x51,
        R = 0x52,
        S = 0x53,
        T = 0x54,
        U = 0x55,
        V = 0x56,
        W = 0x57,
        X = 0x58,
        Y = 0x59,
        Z = 0x5A,

        Num0 = 0x60,
        Num1 = 0x61,
        Num2 = 0x62,
        Num3 = 0x63,
        Num4 = 0x64,
        Num5 = 0x65,
        Num6 = 0x66,
        Num7 = 0x67,
        Num8 = 0x68,
        Num9 = 0x69,
        Multiply = 0x6A,
        Add = 0x6B,
        Subtract = 0x6D,
        Decimal = 0x6E,
        Divide = 0x6F,

        F1 = 0x70,
        F2 = 0x71,
        F3 = 0x72,
        F4 = 0x73,
        F5 = 0x74,
        F6 = 0x75,
        F7 = 0x76,
        F8 = 0x77,
        F9 = 0x78,
        F10 = 0x79,
        F11 = 0x7A,
        F12 = 0x7B,

        NumLock = 0x90,
        ScrollLock = 0x91,
        LeftShift = 0xA0,
        RightShift = 0xA1,
        LeftCtrl = 0xA2,
        RightCtrl = 0xA3,
        LeftAlt = 0xA4,
        RightAlt = 0xA5,
        Semicolon = 0xBA,
        Plus = 0xBB,
        Comma = 0xBC,
        Minus = 0xBD,
        Period = 0xBE,
        Slash = 0xBF,
        Tilde = 0xC0,
        OpenBracket = 0xDB,
        Backslash = 0xDC,
        CloseBracket = 0xDD,
        Quote = 0xDE,
    }

    public static class VirtualKeysExtensions
    {
        public static string ToDisplayString(this VirtualKeys vk)
        {
            if (vk == VirtualKeys.None) return "(none)";

            switch (vk)
            {
                case VirtualKeys.LeftMouse: return "Mouse 1";
                case VirtualKeys.RightMouse: return "Mouse 2";
                case VirtualKeys.MiddleMouse: return "Mouse 3";
                case VirtualKeys.Mouse4: return "Mouse 4";
                case VirtualKeys.Mouse5: return "Mouse 5";
                case VirtualKeys.Ctrl: return "Ctrl";
                case VirtualKeys.Shift: return "Shift";
                case VirtualKeys.Alt: return "Alt";
                case VirtualKeys.Esc: return "Esc";
                case VirtualKeys.Enter: return "Enter";
                case VirtualKeys.Space: return "Space";
                case VirtualKeys.Tab: return "Tab";
                case VirtualKeys.Backspace: return "Backspace";
                case VirtualKeys.Insert: return "Insert";
                case VirtualKeys.Delete: return "Delete";
                case VirtualKeys.Home: return "Home";
                case VirtualKeys.End: return "End";
                case VirtualKeys.PageUp: return "PgUp";
                case VirtualKeys.PageDown: return "PgDn";
                case VirtualKeys.Up: return "Up";
                case VirtualKeys.Down: return "Down";
                case VirtualKeys.Left: return "Left";
                case VirtualKeys.Right: return "Right";
                case VirtualKeys.CapsLock: return "CapsLock";
                case VirtualKeys.NumLock: return "NumLock";
                case VirtualKeys.ScrollLock: return "ScrollLock";
                case VirtualKeys.PrintScreen: return "PrtSc";
                case VirtualKeys.Pause: return "Pause";
                case VirtualKeys.Tilde: return "`";
                case VirtualKeys.Minus: return "-";
                case VirtualKeys.Plus: return "=";
                case VirtualKeys.OpenBracket: return "[";
                case VirtualKeys.CloseBracket: return "]";
                case VirtualKeys.Backslash: return "\\";
                case VirtualKeys.Semicolon: return ";";
                case VirtualKeys.Quote: return "'";
                case VirtualKeys.Comma: return ",";
                case VirtualKeys.Period: return ".";
                case VirtualKeys.Slash: return "/";
            }

            if (vk >= VirtualKeys.D0 && vk <= VirtualKeys.D9)
                return ((char)('0' + (vk - VirtualKeys.D0))).ToString();
            if (vk >= VirtualKeys.Num0 && vk <= VirtualKeys.Num9)
                return "Num " + (vk - VirtualKeys.Num0);

            string name = vk.ToString();
            return name;
        }
    }
}