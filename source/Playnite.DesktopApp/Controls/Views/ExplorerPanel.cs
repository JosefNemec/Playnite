using Playnite.Commands;
using Playnite.DesktopApp.ViewModels;
using Playnite.Extensions;
using Playnite.ViewModels;
using Playnite.ViewModels.Desktop.DesignData;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Playnite.DesktopApp.Controls.Views
{
    public class ExplorerPanel : Control
    {
        private readonly DesktopAppViewModel mainModel;

        static ExplorerPanel()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ExplorerPanel), new FrameworkPropertyMetadata(typeof(ExplorerPanel)));
        }

        public ExplorerPanel() : this(DesktopApplication.Current?.MainModel)
        {
        }

        public ExplorerPanel(DesktopAppViewModel model)
        {
            mainModel = model;
            if (DesignerProperties.GetIsInDesignMode(this))
            {
                DataContext = new DesignMainViewModel();
            }
            else if (mainModel != null)
            {
                DataContext = mainModel.DatabaseExplorer;
            }
        }
    }
}
