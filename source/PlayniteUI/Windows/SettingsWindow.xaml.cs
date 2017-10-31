using PlayniteUI.Controls;
using PlayniteUI.ViewModels;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

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

        private void CombSkinColor_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            var settings = DataContext as SettingsViewModel;
            Skins.ApplySkin(settings.Settings.Skin, settings.Settings.SkinColor);
        }

        private void ComboSkins_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (CombSkinColor.SelectedValue == null && ComboSkins.SelectedItem != null)
            {
                CombSkinColor.SelectedValue = (ComboSkins.SelectedItem as Skin).Profiles.First();
            }

            var settings = DataContext as SettingsViewModel;
            Skins.ApplySkin(settings.Settings.Skin, settings.Settings.SkinColor);
        }
    }
}
