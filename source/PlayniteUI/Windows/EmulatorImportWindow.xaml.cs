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
using System.Windows.Shapes;
using Microsoft.Win32;
using Playnite.Models;
using System.Diagnostics;
using Playnite.Providers.Steam;
using PlayniteUI.Controls;
using System.IO;
using Playnite;
using Playnite.Emulators;
using System.Threading;
using Playnite.Database;
using static PlayniteUI.PlatformsWindow;

namespace PlayniteUI
{
    public class EmulatorImportWindowFactory : WindowFactory
    {
        public static EmulatorImportWindowFactory Instance
        {
            get => new EmulatorImportWindowFactory();
        }

        public override WindowBase CreateNewWindowInstance()
        {
            return new EmulatorImportWindow();
        }
    }

    /// <summary>
    /// Interaction logic for EmulatorImportWindow.xaml
    /// </summary>
    public partial class EmulatorImportWindow : WindowBase
    {
        public EmulatorImportWindow()
        {
            InitializeComponent();
        }
    }
}
