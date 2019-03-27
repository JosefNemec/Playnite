using Playnite.DesktopApp.ViewModels;
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
    public class MainPanel : Control
    {
        static MainPanel()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MainPanel), new FrameworkPropertyMetadata(typeof(MainPanel)));
        }

        public MainPanel() : this(DesktopApplication.Current?.MainModel)
        {
        }

        public MainPanel(DesktopAppViewModel model)
        {
            if (DesignerProperties.GetIsInDesignMode(this))
            {
                DataContext = new DesignMainViewModel();
            }
            else if (model != null)
            {
                DataContext = model;
            }
        }
    }
}
