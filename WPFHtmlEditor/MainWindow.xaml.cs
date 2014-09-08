using System.Windows;
using System.Windows.Input;
using SoftFluent.Tools;

namespace WPFHtmlEditor
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            WpfExtensions.SetTracing();
            InitializeComponent();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            if (!e.Handled)
            {
                MainMenu.RaiseMenuItemClickOnKeyGesture(e);
            }
        }

        private void MenuExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
