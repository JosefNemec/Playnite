using Playnite;
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

namespace PlayniteUI.Windows
{
    /// <summary>
    /// Interaction logic for UpdateWindow.xaml
    /// </summary>
    public partial class UpdateWindow : Window
    {
        private Update update;

        public UpdateWindow()
        {
            InitializeComponent();
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void ButtonInstall_Click(object sender, RoutedEventArgs e)
        {
            update.InstallUpdate();
        }

        public void SetUpdate(Update update)
        {
            this.update = update;
            ItemsNotes.ItemsSource = update.LatestReleaseNotes;
        }
    }
}
