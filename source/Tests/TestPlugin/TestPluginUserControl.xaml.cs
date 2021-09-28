using Playnite.SDK.Controls;
using Playnite.SDK.Models;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TestPlugin
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class TestPluginUserControl : PluginUserControl
    {
        public TestPluginSettingsViewModel SettingsModel { get; set;}

        public TestPluginUserControl(TestPluginSettingsViewModel settings)
        {
            InitializeComponent();
            DataContext = this;
            SettingsModel = settings;
        }

        public override void GameContextChanged(Game oldContext, Game newContext)
        {
            Console.WriteLine($"---- TestPluginUserControl ---- {newContext?.ToString()}");
        }
    }
}
