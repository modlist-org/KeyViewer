using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace KeyViewer.Core
{
    public static class Constants
    {
        public static string SettingsPath => Path.Combine(Main.Mod.Path, "Settings.json");

        public const string Version = "4.9.7";
        public const float Rad2Deg100 = .5729f;

        public static readonly Color TooEarlyColor = new Color(1.000f, 0.000f, 0.000f, 1.000f);
        public static readonly Color VeryEarlyColor = new Color(1.000f, 0.436f, 0.306f, 1.000f);
        public static readonly Color EarlyPerfectColor = new Color(0.627f, 1.000f, 0.306f, 1.000f);
        public static readonly Color PerfectColor = new Color(0.376f, 1.000f, 0.307f, 1.000f);
        public static readonly Color LatePerfectColor = new Color(0.627f, 1.000f, 0.306f, 1.000f);
        public static readonly Color VeryLateColor = new Color(1.000f, 0.435f, 0.306f, 1.000f);
        public static readonly Color TooLateColor = new Color(1.000f, 0.000f, 0.000f, 1.000f);
        public static readonly Color MultipressColor = new Color(0.000f, 1.000f, 0.930f, 1.000f);
        public static readonly Color FailMissColor = new Color(0.851f, 0.346f, 1.000f, 1.000f);
        public static readonly Color FailOverloadColor = new Color(0.851f, 0.346f, 1.000f, 1.000f);
        public static readonly Dictionary<KeyCode, string> KeyString =
            new Dictionary<KeyCode, string>()
            {
                { KeyCode.Alpha0, "0" },
                { KeyCode.Alpha1, "1" },
                { KeyCode.Alpha2, "2" },
                { KeyCode.Alpha3, "3" },
                { KeyCode.Alpha4, "4" },
                { KeyCode.Alpha5, "5" },
                { KeyCode.Alpha6, "6" },
                { KeyCode.Alpha7, "7" },
                { KeyCode.Alpha8, "8" },
                { KeyCode.Alpha9, "9" },
                { KeyCode.Keypad0, "0" },
                { KeyCode.Keypad1, "1" },
                { KeyCode.Keypad2, "2" },
                { KeyCode.Keypad3, "3" },
                { KeyCode.Keypad4, "4" },
                { KeyCode.Keypad5, "5" },
                { KeyCode.Keypad6, "6" },
                { KeyCode.Keypad7, "7" },
                { KeyCode.Keypad8, "8" },
                { KeyCode.Keypad9, "9" },
                { KeyCode.KeypadPlus, "+" },
                { KeyCode.KeypadMinus, "-" },
                { KeyCode.KeypadMultiply, "*" },
                { KeyCode.KeypadDivide, "/" },
                { KeyCode.KeypadEnter, "↵" },
                { KeyCode.KeypadEquals, "=" },
                { KeyCode.KeypadPeriod, "." },
                { KeyCode.Return, "↵" },
                { KeyCode.Tab, "⇥" },
                { KeyCode.Backslash, "\\\\" },
                { KeyCode.Slash, "/" },
                { KeyCode.Minus, "-" },
                { KeyCode.Equals, "=" },
                { KeyCode.LeftBracket, "[" },
                { KeyCode.RightBracket, "]" },
                { KeyCode.Semicolon, ";" },
                { KeyCode.Comma, "," },
                { KeyCode.Period, "." },
                { KeyCode.Quote, "'" },
                { KeyCode.UpArrow, "↑" },
                { KeyCode.DownArrow, "↓" },
                { KeyCode.LeftArrow, "←" },
                { KeyCode.RightArrow, "→" },
                { KeyCode.Space, "␣" },
                { KeyCode.BackQuote, "`" },
                { KeyCode.LeftShift, "L⇧" },
                { KeyCode.RightShift, "R⇧" },
                { KeyCode.LeftControl, "LCtrl" },
                { KeyCode.RightControl, "RCtrl" },
                { KeyCode.LeftAlt, "LAlt" },
                { KeyCode.RightAlt, "RAlt" },
                { KeyCode.Delete, "Del" },
                { KeyCode.PageDown, "Pg↓" },
                { KeyCode.PageUp, "Pg↑" },
                { KeyCode.CapsLock, "⇪" },
                { KeyCode.Insert, "Ins" },
                { KeyCode.Mouse0, "M0" },
                { KeyCode.Mouse1, "M1" },
                { KeyCode.Mouse2, "M2" },
                { KeyCode.Mouse3, "M3" },
                { KeyCode.Mouse4, "M4" },
                { KeyCode.Mouse5, "M5" },
                { KeyCode.Mouse6, "M6" },
            };
    }
}
