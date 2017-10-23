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
using Playnite.Database;
using NLog;
using PlayniteUI.Controls;
using Playnite;
using Playnite.Models;
using System.Collections.ObjectModel;
using System.IO;
using System.ComponentModel;
using System.Windows.Threading;
using Newtonsoft.Json;
using PlayniteUI.Windows;
using Playnite.Emulators;
using PlayniteUI.ViewModels;

namespace PlayniteUI
{
    public class PlatformsWindowFactory : WindowFactory
    {
        public static PlatformsWindowFactory Instance
        {
            get => new PlatformsWindowFactory();
        }

        public override WindowBase CreateNewWindowInstance()
        {
            return new PlatformsWindow();
        }
    }

    /// <summary>
    /// Interaction logic for Configuration.xaml
    /// </summary>
    public partial class PlatformsWindow : WindowBase
    {
        public PlatformsWindow()
        {
            InitializeComponent();
        }
    }
}