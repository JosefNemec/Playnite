using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
using Playnite;
using Playnite.Database;
using Playnite.Models;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.Threading;
using NLog;
using PlayniteUI.Controls;

namespace PlayniteUI
{
    public class InstalledGamesWindowFactory : WindowFactory
    {
        public static InstalledGamesWindowFactory Instance
        {
            get => new InstalledGamesWindowFactory();
        }

        public override WindowBase CreateNewWindowInstance()
        {
            return new InstalledGamesWindow();
        }
    }

    /// <summary>
    /// Interaction logic for InstalledGamesWindow.xaml
    /// </summary>
    public partial class InstalledGamesWindow : WindowBase
    {
        public InstalledGamesWindow()
        {
            InitializeComponent();
        }
    }
}
