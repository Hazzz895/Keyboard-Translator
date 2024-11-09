using Keyboard_Translator.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Shapes;

namespace Keyboard_Translator
{
    public class JsonSettings
    {
        [JsonPropertyName("patterns")]
        public List<Pattern> Patterns { get; set; }

        [JsonPropertyName("selectedIndex")]
        public int SelectedIndex { get; set; }

        [JsonPropertyName("hotkey")]
        public Keys Hotkey { get; set; }

        [JsonPropertyName("modifers")]
        public ModifersInfo Modifers { get; set; }

        [JsonPropertyName("mode")]
        public Mode SelectedMode { get; set; }

        private static readonly JsonSettings defaultSettings = new JsonSettings()
        {
            Patterns = [new() {
                 FirstPattern = "`1234567890-=qwertyuiop[]\\asdfghjkl;'zxcvbnm,./~!@#$%^&*()_+QWERTYUIOP{}|ASDFGHJKL:\"ZXCVBNM<>?",
                 SecondPattern ="ё1234567890-=йцукенгшщзхъ\\фывапролджэячсмитьбю.Ё!\"№;%:?*()_+ЙЦУКЕНГШЩЗХЪ/ФЫВАПРОЛДЖЭЯЧСМИТЬБЮ,",
                 Name = "EN - RU"
            }],
            Hotkey = Keys.F4,
            SelectedIndex = 0,
            Modifers = new()
            {
                AltEnabled = false,
                ShiftEnabled = true,
                WinEnabled = false,
                CtrlEnabled = false
            },
            SelectedMode = Mode.Auto
        };

        public static JsonSettings ReadOrCreateSettingsFile(string path = "JsonSettings.json")
        {
            FileInfo fileInfo = new FileInfo(path);

            if (!fileInfo.Exists)
            {
                return UpdateToDefaultConfig();
            }
            else
            {
                using var stream = new FileStream(path, FileMode.Open);

                JsonSettings settings;
                try
                {
                    settings = JsonSerializer.Deserialize<JsonSettings>(stream);
                }
                catch { Console.WriteLine("aokoioij\n\n\n\n\n"); return null;}
                if (settings.CheckForNull()) return null;

                return settings;
            }

        }

        public static void UpdateSettingsFile(JsonSettings newSettings, string path = "JsonSettings.json")
        {
            using var stream = new FileStream(path, FileMode.OpenOrCreate);
            using var writer = new StreamWriter(stream);

            string content = JsonSerializer.Serialize(newSettings);
            writer.Write(content);
        }

        public static JsonSettings UpdateToDefaultConfig(string path = "JsonSettings.json")
        {
            UpdateSettingsFile(defaultSettings);
            return defaultSettings;
        }

        private bool CheckForNull()
        {
            if (Patterns == null) return true;
            if (Modifers == null) return true;
            if (Hotkey == null) return true;
            if (SelectedMode == null) return true;

            foreach (var pattern in Patterns)
                if (pattern.FirstPattern == null || pattern.SecondPattern == null) return true;
            return false;
        }

        public class Pattern
        {
            [JsonPropertyName("pattern1")]
            public string FirstPattern { get; set; }

            [JsonPropertyName("pattern2")]
            public string SecondPattern { get; set; }

            [JsonPropertyName("name")]
            public string Name { get; set; }

            public override string ToString()
            {
                return Name == null ? string.Empty : Name;
            }
        }
        
        public class ModifersInfo
        {
            [JsonPropertyName("shiftEnabled")]
            public bool ShiftEnabled { get; set; }

            [JsonPropertyName("ctrlEnabled")]
            public bool CtrlEnabled { get; set; }

            [JsonPropertyName("winEnabled")]
            public bool WinEnabled { get; set; }
            
            [JsonPropertyName("altEnabled")]
            public bool AltEnabled { get; set; }
        }

        public enum Mode
        {
            None =-1,
            Auto,
            FromFirstToSecond,
            FromSecondToFirst
        }
    }
}
