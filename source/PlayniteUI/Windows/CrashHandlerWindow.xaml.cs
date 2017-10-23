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

namespace PlayniteUI.Windows
{
    public class CrashHandlerWindowFactory : WindowFactory
    {
        public static CrashHandlerWindowFactory Instance
        {
            get => new CrashHandlerWindowFactory();
        }

        public override WindowBase CreateNewWindowInstance()
        {
            return new CrashHandlerWindow();
        }
    }

    /// <summary>
    /// Interaction logic for CrashHandlerWindow.xaml
    /// </summary>
    public partial class CrashHandlerWindow : WindowBase
    {
        public CrashHandlerWindow()
        {
            InitializeComponent();
        }
    }
}
