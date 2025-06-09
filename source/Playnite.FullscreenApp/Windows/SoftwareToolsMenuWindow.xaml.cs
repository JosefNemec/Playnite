using Playnite.Controls;
using Playnite.Windows;
using System;
using System.Collections.Generic;
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
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Playnite.FullscreenApp.Windows
{
    public class SoftwareToolsMenuWindowFactory : WindowFactory
    {
        public override WindowBase CreateNewWindowInstance()
        {
            return new SoftwareToolsMenuWindow();
        }
    }

    /// <summary>
    /// Interaction logic for UpdateWindow.xaml
    /// </summary>
    public partial class SoftwareToolsMenuWindow : WindowBase
    {
        public SoftwareToolsMenuWindow() : base()
        {
            InitializeComponent();
            WindowTools.ConfigureChildWindow(this);
        }
    }
}
