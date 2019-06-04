using Playnite.API;
using Playnite.API.DesignData;
using Playnite.DesktopApp.ViewModels;
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
        private DesktopAppViewModel mainModel;

        static NotificationPanel()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(NotificationPanel), new FrameworkPropertyMetadata(typeof(NotificationPanel)));
        }

        public NotificationPanel() : this(DesktopApplication.Current?.MainModel)
        {
        }

        public NotificationPanel(DesktopAppViewModel mainModel)
        {
            this.mainModel = mainModel;
            if (DesignerProperties.GetIsInDesignMode(this))
            {
                DataContext = new DesignMainViewModel();
            }
            else
            {
                DataContext = mainModel;
            }
        }
    }
}
