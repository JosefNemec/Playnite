using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using NLog;
using Playnite;
using PlayniteUI.Controls;

namespace PlayniteUI
{
    public class AboutWindowFactory : WindowFactory
    {
        public static AboutWindowFactory Instance
        {
            get => new AboutWindowFactory();
        }

        public override WindowBase CreateNewWindowInstance()
        {
            return new AboutWindow();
        }
    }

    /// <summary>
    /// Interaction logic for AboutWindow.xaml
    /// </summary>
    public partial class AboutWindow : WindowBase
    {
        public AboutWindow()
        {
            InitializeComponent();
        }
    }
}
