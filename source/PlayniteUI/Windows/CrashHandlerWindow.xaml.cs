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

namespace PlayniteUI.Windows
{
    /// <summary>
    /// Interaction logic for CrashHandlerWindow.xaml
    /// </summary>
    public partial class CrashHandlerWindow : Window
    {
        private static Logger logger = LogManager.GetCurrentClassLogger();

        public CrashHandlerWindow()
        {
            InitializeComponent();
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ButtonSaveDiag_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new SaveFileDialog()
            {
                Filter = "ZIP Archive (*.zip)|*.zip"
            };

            if (dialog.ShowDialog(this) == true)
            {
                try
                {
                    Diagnostic.CreateDiagPackage(dialog.FileName);
                    MessageBox.Show("Diagnostics package created successfully.");
                }
                catch (Exception exc)
                {
                    logger.Error(exc, "Faild to created diagnostics package.");
                    MessageBox.Show("Failed to create diagnostics package.");
                }
            }
        }

        private void ButtonReportIssue_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(@"https://github.com/JosefNemec/Playnite/issues");
        }

        private void ButtonRestart_Click(object sender, RoutedEventArgs e)
        {
            Process.Start(Paths.ExecutablePath);
            Close();
        }
    }
}
