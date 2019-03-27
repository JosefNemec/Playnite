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
    public class FilterPanel : Control
    {
        private readonly DesktopAppViewModel mainModel;

        static FilterPanel()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(FilterPanel), new FrameworkPropertyMetadata(typeof(FilterPanel)));
        }

        public FilterPanel() : this(DesktopApplication.Current?.MainModel)
        {
        }

        public FilterPanel(DesktopAppViewModel model)
        {
            mainModel = model;
            if (DesignerProperties.GetIsInDesignMode(this))
            {
                DataContext = new DesignMainViewModel();
            }
            else if (mainModel != null)
            {
                DataContext = mainModel;
            }
        }
    }
}
