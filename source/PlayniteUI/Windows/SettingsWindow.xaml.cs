using PlayniteUI.Controls;
using PlayniteUI.ViewModels;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Controls;

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
            if ((sender as ComboBox).SelectedValue == null)
            {
                return;
            }

            var settings = DataContext as SettingsViewModel;
            if (settings.Settings.Skin == Skins.CurrentSkin)
            {
                Skins.ApplySkin(settings.Settings.Skin, settings.Settings.SkinColor);
            }
        }

        private void ComboSkins_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if ((sender as ComboBox).SelectedValue == null)
            {
                return;
            }

            if (CombSkinColor.SelectedValue == null && ComboSkins.SelectedItem != null)
            {
                CombSkinColor.SelectedValue = (ComboSkins.SelectedItem as Skin).Profiles.First();
            }
        }

        private void ComboSkinsFullscreen_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //if (CombSkinColorFullscreen.SelectedValue == null && ComboSkinsFullscreen.SelectedItem != null)
            //{
            //    CombSkinColorFullscreen.SelectedValue = (ComboSkinsFullscreen.SelectedItem as Skin).Profiles.First();
            //}
        }
    }
}
