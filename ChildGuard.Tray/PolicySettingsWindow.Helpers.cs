using System.Windows;

namespace ChildGuard.Tray
{
    public partial class PolicySettingsWindow : Window
    {
        private bool Confirm(string msg)
        {
            var r = System.Windows.MessageBox.Show(msg, "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question);
            return r == MessageBoxResult.Yes;
        }
    }
}

