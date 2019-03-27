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
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Playnite.DesktopApp.Controls.Views
{ 
    public class MainWindow : Control
    {
        static MainWindow()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MainWindow), new FrameworkPropertyMetadata(typeof(MainWindow)));
        }

        public MainWindow() : this(DesktopApplication.Current?.MainModel)
        {
        }

        public MainWindow(DesktopAppViewModel mainModel)
        {
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
