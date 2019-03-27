using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Playnite.DesktopApp.Controls.Views
{
    public class LibraryListView : Control
    {
        static LibraryListView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(LibraryListView), new FrameworkPropertyMetadata(typeof(LibraryListView)));
        }
    }
}
