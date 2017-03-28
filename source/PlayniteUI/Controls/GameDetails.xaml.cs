using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Playnite.Database;
using Playnite.Models;

namespace PlayniteUI
{
    /// <summary>
    /// Interaction logic for GameDetails.xaml
    /// </summary>
    public partial class GameDetails : UserControl, INotifyPropertyChanged
    {
        public bool ShowContent
        {
            get
            {
                return DataContext != null;
            }
        }

        public GameDetails()
        {
            InitializeComponent();
            PopupMore.ShowPlayInstallButton = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void ButtonPlay_Click(object sender, RoutedEventArgs e)
        {
            var game = (IGame)DataContext;
            GamesEditor.Instance.PlayGame(game);
        }

        private void ButtonInstall_Click(object sender, RoutedEventArgs e)
        {
            var game = (IGame)DataContext;
            GamesEditor.Instance.InstallGame(game);
        }

        private void ButtonMore_Click(object sender, RoutedEventArgs e)
        {
            PopupMore.PlacementTarget = (UIElement)sender;
            PopupMore.IsOpen = true;
        }

        private void Link_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            try
            {
                Process.Start(e.Uri.AbsoluteUri);
            }
            catch
            {
                MessageBox.Show("Cannot open link. URL is not in valid Format.\nURL: " + e.Uri.ToString(), "Invalid URL", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null)
            {
                var game = (IGame)e.NewValue;
                this.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(Theme.BackgroundColors[game.Provider]));
            }

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ShowContent"));
        }
    }
}
