using PlayniteUI.Controls;
using PlayniteUI.ViewModels;
using System.Drawing;

namespace PlayniteUI
{
    public class SettingsWindowFactory : WindowFactory
    {
        public static SettingsWindowFactory Instance
        {
            get => new SettingsWindowFactory();
        }

        public override WindowBase CreateNewWindowInstance()
        {
            return new SettingsWindow();
        }
    }

    /// <summary>
    /// Interaction logic for Configuration.xaml
    /// </summary>
    public partial class SettingsWindow : WindowBase
    {
        public SettingsWindow()
        {
            InitializeComponent();
        }

        private void ComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var settings = DataContext as SettingsViewModel;
            Skins.ApplySkin(settings.Settings.Skin, settings.Settings.SkinColor);
        }

        private void ComboSkins_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var settings = DataContext as SettingsViewModel;
            Skins.ApplySkin(settings.Settings.Skin, settings.Settings.SkinColor);
        }
    }
}
