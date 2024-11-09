using System;
using System.IO;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using SendKeys = System.Windows.Forms.SendKeys;
using System.Windows.Interop;
using Keys = System.Windows.Forms.Keys;
using System.Threading.Tasks;

namespace Keyboard_Translator
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly IntPtr hWnd;
        public static JsonSettings Settings;
        private PInvokeHelper.KeyboardHooker _hooker;

        private bool IsSelectingNewHotkey = false;
        public MainWindow()
        {
            InitializeComponent();

            hWnd = new WindowInteropHelper(this).EnsureHandle();
            PInvokeHelper.ChangeApplicationTheme(hWnd, true);

            Settings = JsonSettings.ReadOrCreateSettingsFile();

            if (Settings == null)
            {
                if (MessageBox.Show("Во время считывания файла настроек произошла ошибка. Нажмите \"ОК\", чтобы вернуть настройки по умолчанию.", "Неожиданная шибка чтения файла", MessageBoxButton.OKCancel, MessageBoxImage.Error, MessageBoxResult.Cancel) == MessageBoxResult.OK)
                {
                    Settings = JsonSettings.UpdateToDefaultConfig();
                }
                else
                {
                    Application.Current.Shutdown();
                    return;
                }
            }

            PatternList.ItemsSource = Settings.Patterns;
            PatternList.SelectedIndex = Settings.SelectedIndex;

            WinCheckBox.IsChecked = Settings.Modifers.WinEnabled;
            ShiftCheckBox.IsChecked = Settings.Modifers.ShiftEnabled;
            CtrlCheckBox.IsChecked = Settings.Modifers.CtrlEnabled;
            AltCheckBox.IsChecked = Settings.Modifers.AltEnabled;

            ModeList.SelectedIndex = (int)Settings.SelectedMode;

            _hooker = new();
            _hooker.KeyPressed += KeyPressed;
            _hooker.Hook();

            ShiftCheckBox.IsChecked = Settings.Modifers.ShiftEnabled;
            CtrlCheckBox.IsChecked = Settings.Modifers.CtrlEnabled;
            WinCheckBox.IsChecked = Settings.Modifers.WinEnabled;
            AltCheckBox.IsChecked = Settings.Modifers.AltEnabled;

            HotkeyButton.Content = Settings.Hotkey;
            ModeList.SelectionChanged += ModeList_SelectionChanged;
        }

        private void KeyPressed(object s, PInvokeHelper.KeyboardHooker.KeyPressedArgs args) 
        {
            if (args.State != PInvoke.KeyboardState.KeyUp) return;

            if (IsSelectingNewHotkey) 
            {
                if (args.Key is Keys.LShiftKey or Keys.RShiftKey or Keys.Shift or Keys.ShiftKey) 
                {
                    ShiftCheckBox.IsChecked = !ShiftCheckBox.IsChecked;
                    return;
                }
                if (args.Key is Keys.LControlKey or Keys.RControlKey or Keys.Control or Keys.ControlKey) 
                {
                    CtrlCheckBox.IsChecked = !CtrlCheckBox.IsChecked;
                    return;
                }
                if (args.Key is Keys.LWin or Keys.RWin) 
                {
                    WinCheckBox.IsChecked = !WinCheckBox.IsChecked;
                    return;
                } 
                if (args.Key is Keys.LMenu or Keys.RMenu or Keys.Menu) 
                {
                    AltCheckBox.IsChecked = !AltCheckBox.IsChecked;
                    return;
                }

                HotkeyButton.Content = args.Key;

                Settings.Hotkey = args.Key;
                JsonSettings.UpdateSettingsFile(Settings);

                IsSelectingNewHotkey = false;
            }
            else
            {
                if (Settings.Hotkey == args.Key) DoTrasnalate();
            }
        }

        private void DoTrasnalate()
        {
            SendKeys.SendWait("^C");
            string text = Clipboard.GetText();
            string updText = Translate(text, Settings.Patterns[Settings.SelectedIndex], (JsonSettings.Mode)Settings.SelectedMode);
            ReplaceText(updText);
        }

        private string Translate(string text, JsonSettings.Pattern patterns, JsonSettings.Mode mode)
        {
            if (mode == JsonSettings.Mode.FromFirstToSecond) return ReplaceWithPattern(text, patterns.FirstPattern, patterns.SecondPattern);
            else if (mode == JsonSettings.Mode.FromSecondToFirst) return ReplaceWithPattern(text, patterns.SecondPattern, patterns.FirstPattern);
            else if (mode == JsonSettings.Mode.Auto)
            {
                var definedMode = DefineMode(text, patterns);
                return Translate(text, patterns, definedMode);
            }
            else return text;
        }

        private JsonSettings.Mode DefineMode(string text, JsonSettings.Pattern patterns)
        {
            foreach (var ch in text) 
            {
                bool fContains = patterns.FirstPattern.Contains(ch.ToString());
                bool sContains = patterns.SecondPattern.Contains(ch.ToString());

                if (fContains == sContains) continue;

                if (fContains) return JsonSettings.Mode.FromFirstToSecond;
                else if (sContains) return JsonSettings.Mode.FromSecondToFirst;
                else return JsonSettings.Mode.None;
            }
            return JsonSettings.Mode.None;
        }

        private string ReplaceWithPattern(string text, string firstPattern, string secondPattern)
        {
            string output = "";
            foreach (var ch in text)
            {
                if (firstPattern.Contains(ch.ToString())) output += secondPattern[firstPattern.IndexOf(ch)];
                else output += ch;
            }
            return output;
        }

        private void ReplaceText(string text)
        {
            Clipboard.SetText(text);
            SendKeys.SendWait("^V");
            Clipboard.Clear();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            if (_hooker != null && _hooker.Hooked == true)  _hooker.UnHook();
            Application.Current.Shutdown();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            new EditWindow(Settings, PatternList.SelectedIndex).Show();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var s = sender as Button;
            if (!IsSelectingNewHotkey)
            {
                s.Content = "Press new key...";
                IsSelectingNewHotkey = true;
            }
            else
            {
                s.Content = Settings.Hotkey;
                IsSelectingNewHotkey = false;
            }
        }

        private void HotkeyButton_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (IsSelectingNewHotkey)
            {
                HotkeyButton.Content = "Cancel";
            }
        }

        private void HotkeyButton_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (IsSelectingNewHotkey)
            {
                HotkeyButton.Content = "Press new key...";
            }
        }

        private void ModeList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!IsInitialized) return;
            var s = sender as ComboBox;
            Settings.SelectedMode = (JsonSettings.Mode)s.SelectedIndex;
            JsonSettings.UpdateSettingsFile(Settings);
        }

        private void ShiftCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (!IsInitialized) return;
            var s = sender as CheckBox;
            Settings.Modifers.ShiftEnabled = s.IsChecked ?? false;
            JsonSettings.UpdateSettingsFile(Settings);
        } 

        private void AltCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (!IsInitialized) return;
            var s = sender as CheckBox;
            Settings.Modifers.AltEnabled = s.IsChecked ?? false;
            JsonSettings.UpdateSettingsFile(Settings);
        } 
        
        private void CtrlCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (!IsInitialized) return;
            var s = sender as CheckBox;
            Settings.Modifers.CtrlEnabled = s.IsChecked ?? false;
            JsonSettings.UpdateSettingsFile(Settings);
        }
        
        private void WinCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            if (!IsInitialized) return;
            var s = sender as CheckBox;
            Settings.Modifers.WinEnabled = s.IsChecked ?? false;
            JsonSettings.UpdateSettingsFile(Settings);
        }
    }
}
