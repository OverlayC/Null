using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace NullEx
{
    public static class Config
    {
        static readonly string ConfigDir =
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "NullEx");

        static readonly string ConfigPath =
            Path.Combine(ConfigDir, "config.ini");

        public static string FilePath => ConfigPath;

        private static string _lastWritten;

        public static void Save()
        {
            try
            {
                Directory.CreateDirectory(ConfigDir);

                var sb = new StringBuilder();
                sb.AppendLine("; NullEx configuration");
                sb.AppendLine("; Auto-generated. Edit with care.");
                sb.AppendLine();

                sb.AppendLine("[Global]");
                sb.AppendLine($"Theme={Theme.Current}");
                sb.AppendLine($"MenuKey={MenuKeys.ToggleKey}");
                sb.AppendLine();

                foreach (var f in FeatureManager.Features)
                {
                    sb.AppendLine($"[{f.Name}]");
                    sb.AppendLine($"Enabled={(f.IsDesired ? 1 : 0)}");
                    sb.AppendLine($"ToggleKey={f.ToggleKey}");

                    foreach (string key in f.PersistentKeys)
                    {
                        if (!f.Settings.TryGetValue(key, out object val)) continue;
                        sb.AppendLine($"{key}={Format(val)}");
                    }
                    sb.AppendLine();
                }

                string content = sb.ToString();

                if (content == _lastWritten) return;

                File.WriteAllText(ConfigPath, content, Encoding.UTF8);
                _lastWritten = content;

                MainForm.Log($"Config saved: {ConfigPath}");
            }
            catch (Exception ex)
            {
                MainForm.Log($"Config save failed: {ex.Message}");
            }
        }

        public static void Load()
        {
            try
            {
                if (!File.Exists(ConfigPath))
                {
                    MainForm.Log("No config file found; using defaults.");
                    return;
                }

                var lines = File.ReadAllLines(ConfigPath, Encoding.UTF8);
                string section = "";
                var sections = new Dictionary<string, Dictionary<string, string>>(StringComparer.OrdinalIgnoreCase);

                foreach (string raw in lines)
                {
                    string line = raw.Trim();
                    if (line.Length == 0) continue;
                    if (line.StartsWith(";") || line.StartsWith("#")) continue;

                    if (line.StartsWith("[") && line.EndsWith("]"))
                    {
                        section = line.Substring(1, line.Length - 2).Trim();
                        if (!sections.ContainsKey(section))
                            sections[section] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                        continue;
                    }

                    int eq = line.IndexOf('=');
                    if (eq <= 0) continue;
                    string key = line.Substring(0, eq).Trim();
                    string value = line.Substring(eq + 1).Trim();

                    if (!sections.ContainsKey(section))
                        sections[section] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                    sections[section][key] = value;
                }

                if (sections.TryGetValue("Global", out var g))
                {
                    if (g.TryGetValue("Theme", out var themeName))
                        Theme.Apply(themeName);

                    if (g.TryGetValue("MenuKey", out var mkStr) &&
                        int.TryParse(mkStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out int mk))
                    {
                        MenuKeys.ToggleKey = mk;
                        MainForm.Log($"Config: menu key = {(VirtualKeys)mk}");
                    }
                }

                foreach (var f in FeatureManager.Features)
                {
                    if (!sections.TryGetValue(f.Name, out var s)) continue;

                    if (s.TryGetValue("ToggleKey", out var tkStr) &&
                        int.TryParse(tkStr, NumberStyles.Integer, CultureInfo.InvariantCulture, out int tk))
                    {
                        f.ToggleKey = tk;
                    }

                    foreach (string key in f.PersistentKeys)
                    {
                        if (!s.TryGetValue(key, out var valStr)) continue;
                        if (!f.Settings.TryGetValue(key, out object current)) continue;

                        object parsed = ParseAs(current, valStr);
                        if (parsed != null)
                            f.Settings[key] = parsed;
                    }

                    if (s.TryGetValue("Enabled", out var enStr) && enStr.Trim() == "1")
                    {
                        MainForm.Log($"Config: enabling feature '{f.Name}'");
                        f.SetEnabled(true);
                    }
                }

                MainForm.Log($"Config loaded: {ConfigPath}");
            }
            catch (Exception ex)
            {
                MainForm.Log($"Config load failed: {ex.Message}");
            }
        }

        public static void Delete()
        {
            try
            {
                if (File.Exists(ConfigPath)) File.Delete(ConfigPath);
                _lastWritten = null;
                MainForm.Log("Config deleted.");
            }
            catch (Exception ex)
            {
                MainForm.Log($"Config delete failed: {ex.Message}");
            }
        }

        static string Format(object val)
        {
            switch (val)
            {
                case bool b: return b ? "1" : "0";
                case float f: return f.ToString("R", CultureInfo.InvariantCulture);
                case double d: return d.ToString("R", CultureInfo.InvariantCulture);
                case int i: return i.ToString(CultureInfo.InvariantCulture);
                case long l: return l.ToString(CultureInfo.InvariantCulture);
                case string s: return s;
                default: return val?.ToString() ?? "";
            }
        }

        static object ParseAs(object template, string text)
        {
            try
            {
                switch (template)
                {
                    case bool _:
                        if (text == "1" || text.Equals("true", StringComparison.OrdinalIgnoreCase)) return true;
                        if (text == "0" || text.Equals("false", StringComparison.OrdinalIgnoreCase)) return false;
                        return null;

                    case float _:
                        return float.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out float f) ? (object)f : null;

                    case double _:
                        return double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out double d) ? (object)d : null;

                    case int _:
                        return int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int i) ? (object)i : null;

                    case long _:
                        return long.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out long l) ? (object)l : null;

                    case string _:
                        return text;

                    default:
                        return null;
                }
            }
            catch { return null; }
        }
    }
}