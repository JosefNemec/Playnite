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
using Microsoft.Win32;
using Playnite.Models;

namespace PlayniteUI
{
    /// <summary>
    /// Interaction logic for GameTaskView.xaml
    /// </summary>
    public partial class GameTaskView : UserControl
    {
        public GameTaskView()
        {
            InitializeComponent();
        }

        private void ButtonBrowsePath_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog()
            {
                Filter = "All files (*.*)|*.*"
            };

            if (dialog.ShowDialog(Window.GetWindow(this)) == true)
            {
                TextPath.Text = dialog.FileName;
            }
        }

        private void ComboType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DataContext == null)
            {
                return;
            }

            var task = (GameTask)DataContext;

            if (task.Type == GameTaskType.File)
            {
                RowArguments.Height = new GridLength();
                RowWorkingDir.Height = new GridLength();
            }
            else
            {
                RowArguments.Height = new GridLength(0);
                RowWorkingDir.Height = new GridLength(0);
            }
        }
    }
}
