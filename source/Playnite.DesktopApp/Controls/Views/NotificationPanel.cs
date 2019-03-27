using Playnite.API;
using Playnite.API.DesignData;
using Playnite.SDK;
using Playnite.ViewModels;
using Playnite.ViewModels.Desktop.DesignData;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Playnite.DesktopApp.Controls.Views
{
    public class NotificationPanel : Control
    {
        private readonly IPlayniteAPI api;

        static NotificationPanel()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NotificationPanel), new FrameworkPropertyMetadata(typeof(NotificationPanel)));
        }

        public NotificationPanel() : this(PlayniteApplication.Current?.Api)
        {
        }

        public NotificationPanel(IPlayniteAPI api)
        {
            this.api = api;
            if (DesignerProperties.GetIsInDesignMode(this))
            {
                DataContext = new DesignPlayniteAPI();
            }
            else if (api != null)
            {
                DataContext = api;
            }
        }
    }
}
