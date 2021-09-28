using Playnite.Common;
using Playnite.Converters;
using Playnite.Extensions.Markup;
using Playnite.SDK;
using Playnite.Settings;
using Playnite.ViewModels;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Playnite.DesktopApp.Controls
{
    public class SortSettingsMenu : ContextMenu
    {
        private readonly PlayniteSettings settings;

        static SortSettingsMenu()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SortSettingsMenu), new FrameworkPropertyMetadata(typeof(SortSettingsMenu)));
        }

        public SortSettingsMenu() : this(PlayniteApplication.Current?.AppSettings)
        {
        }

        public SortSettingsMenu(PlayniteSettings settings)
        {
            this.settings = settings;
            InitializeItems();
            Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            StaysOpen = false;
        }

        public void InitializeItems()
        {
            if (settings == null)
            {
                return;
            }

            ViewSettingsMenu.GenerateSortMenu(Items, settings);
        }
    }
}
