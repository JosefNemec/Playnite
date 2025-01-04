using Playnite.Controls;
using Playnite.ViewModels;
using Playnite.Windows;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Controls;

namespace Playnite.DesktopApp.Windows
{
    public class SettingsWindowFactory : WindowFactory
    {
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
        private WindowPositionHandler positionManager;

        public SettingsWindow() : base(nameof(SettingsWindow), true)
        {
            InitializeComponent();
            if (PlayniteApplication.Current.AppSettings != null)
            {
                positionManager = new WindowPositionHandler(this, "SettingsWindow", PlayniteApplication.Current.AppSettings.WindowPositions);
            }
        }
    }
}
