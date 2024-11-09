using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using static Keyboard_Translator.PInvoke;

namespace Keyboard_Translator
{
    internal static class PInvokeHelper
    {
        public static void ChangeApplicationTheme(IntPtr hWnd, bool darkTheme)
        {
            int value = darkTheme ? True : False;
            DwmSetWindowAttribute(hWnd, DwmWindowAttribute.DWMWA_USE_IMMERSIVE_DARK_MODE, ref value, sizeof(int));
        }

        public class KeyboardHooker
        {
            public KeyboardHooker() { }

            private IntPtr hookHandle = IntPtr.Zero;

            public bool Hooked = false;
            public LowLevelKeyboardProc _proc;
            public EventHandler<KeyPressedArgs> KeyPressed;

            private IntPtr OnKeyPressed(int nCode, IntPtr wParam, IntPtr lParam)
            {
                bool handled = false;

                KeyPressedArgs eventArguments = new()
                {
                        KeyData = Marshal.PtrToStructure<LowLevelKeyboardInputEvent>(lParam),
                        State = (KeyboardState)wParam
                };
                if (Enum.IsDefined(typeof(Keys), eventArguments.KeyData.VirtualCode)) eventArguments.Key = (Keys)eventArguments.KeyData.VirtualCode;

                KeyPressed.Invoke(this, eventArguments);
                handled = eventArguments.Handled;
                return (IntPtr)(handled ? 1 : 0);
            }

            public void Hook()
            {
                _proc = OnKeyPressed;
                Hooked = true;
                hookHandle = CreateHook(_proc);
            }

            public void UnHook()
            {
                _proc = null;
                hookHandle = IntPtr.Zero;
                Hooked = false;
                UnhookWindowsHookEx(hookHandle);
            }

            private IntPtr CreateHook(LowLevelKeyboardProc proc)
            {
                using var procces = Process.GetCurrentProcess();
                using var module = procces.MainModule;

                var moduleHandle = GetModuleHandle(module.ModuleName);

                IntPtr hook = SetWindowsHookEx(WhHookId.WH_KEYBOARD_LL, proc, moduleHandle, 0);
                return hook;
            }

            public class KeyPressedArgs : HandledEventArgs
            {
                public KeyPressedArgs() { }

                public Keys Key { get; internal set; }
                public LowLevelKeyboardInputEvent KeyData { get; internal set; }
                public KeyboardState State { get; internal set; }
            }
        }
    }
}
