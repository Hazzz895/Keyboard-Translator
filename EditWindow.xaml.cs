using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Keyboard_Translator
{
    /// <summary>
    /// Логика взаимодействия для EditWindow.xaml
    /// </summary>
    public partial class EditWindow : Window
    {
        private IntPtr hWnd;
        private JsonSettings settings;
        public EditWindow(JsonSettings settings, int selectedIndex)
        {
            InitializeComponent();

            hWnd = new WindowInteropHelper(this).EnsureHandle();
            PInvokeHelper.ChangeApplicationTheme(hWnd, true);

            this.settings = settings;
            FirstPatternBox.Text = settings.Patterns[selectedIndex].FirstPattern;
            SecondPatternBox.Text = settings.Patterns[selectedIndex].SecondPattern;
        }
    }
}
