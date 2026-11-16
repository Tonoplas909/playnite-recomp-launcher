using System.Windows;
using System.Windows.Controls;
using Playnite.SDK;

namespace RecompLauncherLibrary
{
    public partial class SettingsView : UserControl
    {
        private readonly IPlayniteAPI api;

        public SettingsView(IPlayniteAPI api)
        {
            this.api = api;
            InitializeComponent();
        }

        private void Browse_Click(object sender, RoutedEventArgs e)
        {
            var path = api.Dialogs.SelectFile("recomplauncher.exe|*.exe");
            if (!string.IsNullOrEmpty(path))
                ((RecompLauncherSettingsViewModel)DataContext).Settings.ExePath = path;
        }
    }
}
