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
    public class LibraryGridView : Control
    {
        static LibraryGridView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(LibraryGridView), new FrameworkPropertyMetadata(typeof(LibraryGridView)));
        }

        public LibraryGridView() : this(DesktopApplication.Current?.MainModel)
        {
        }

        public LibraryGridView(DesktopAppViewModel mainModel)
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
