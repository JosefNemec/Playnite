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

namespace Playnite.DesktopApp.Windows
{
    public class PluginSettingsWindowFactory : WindowFactory
    {
        public override WindowBase CreateNewWindowInstance()
        {
            return new PluginSettingsWindow();
        }
    }

    /// <summary>
    /// Interaction logic for PluginSettingsWindow.xaml
    /// </summary>
    public partial class PluginSettingsWindow : WindowBase
    {
        public PluginSettingsWindow()
        {
            InitializeComponent();
        }
    }
}
