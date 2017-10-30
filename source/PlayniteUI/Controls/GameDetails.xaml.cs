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
using Playnite;

namespace PlayniteUI.Controls
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

        public bool ShowInfoPanel
        {
            get
            {
                if (DataContext != null && DataContext is GameViewEntry)
                {
                    var game = DataContext as GameViewEntry;
                    return
                        (game.Genres != null && game.Genres.Count > 0) ||
                        (game.Publishers != null && game.Publishers.Count > 0) ||
                        (game.Developers != null && game.Developers.Count > 0) ||
                        (game.Categories != null && game.Categories.Count > 0) ||
                        game.ReleaseDate != null ||
                        (game.Links != null && game.Links.Count > 0) ||
                        !string.IsNullOrEmpty(game.Platform.Name);
                }
                else
                {
                    return false;
                }
            }
        }

        public GameDetails()
        {
            InitializeComponent();
            //PopupMore.ShowPlayInstallButton = false;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void ButtonPlay_Click(object sender, RoutedEventArgs e)
        {
            var game = (GameViewEntry)DataContext;
            GamesEditor.Instance.PlayGame(game.Game);
        }

        private void ButtonInstall_Click(object sender, RoutedEventArgs e)
        {
            var game = (GameViewEntry)DataContext;
            GamesEditor.Instance.InstallGame(game.Game);
        }

        private void ButtonMore_Click(object sender, RoutedEventArgs e)
        {
            //PopupMore.PlacementTarget = (UIElement)sender;
            //PopupMore.IsOpen = true;
        }

        private void ButtonSetupProgress_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is GameViewEntry)
            {
                var game = (DataContext as GameViewEntry).Game as Game;
                game.UnregisetrStateMonitor();
            }            
        }

        private void Link_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            try
            {
                Process.Start(e.Uri.AbsoluteUri);
            }
            catch
            {
                PlayniteMessageBox.Show("Cannot open link. URL is not in valid Format.\nURL: " + e.Uri.ToString(), "Invalid URL", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void UserControl_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null && e.NewValue is GameViewEntry)
            {
                var game = (GameViewEntry)e.NewValue;

                switch (game.Provider)
                {
                    case Provider.Custom:
                        BorderGameInfo.Background = Brushes.Transparent;
                        Background = FindResource("ControlBackgroundBrush") as Brush;
                        break;
                    case Provider.GOG:
                        BorderGameInfo.Background = FindResource("NormalBrushDark") as Brush;
                        Background = FindResource("GogGameBackgroundBrush") as Brush;
                        break;
                    case Provider.Origin:
                        BorderGameInfo.Background = Brushes.Transparent;
                        Background = FindResource("ControlBackgroundBrush") as Brush;
                        break;
                    case Provider.Steam:
                        BorderGameInfo.Background = Brushes.Transparent;
                        Background = FindResource("SteamGameBackgroundBrush") as Brush;
                        break;
                    default:
                        Background = FindResource("ControlBackgroundBrush") as Brush;
                        break;
                }
            }

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ShowContent"));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ShowInfoPanel"));            
        }

        private void Filter_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            var uri = ((Hyperlink)sender).NavigateUri.OriginalString;
            var tag = ((Hyperlink)sender).Tag as string;

            switch (tag)
            {
                case "Genres":
                    Settings.Instance.FilterSettings.Genres = new List<string>() { uri };
                    break;
                case "Developers":
                    Settings.Instance.FilterSettings.Developers = new List<string>() { uri };
                    break;
                case "Publishers":
                    Settings.Instance.FilterSettings.Publishers = new List<string>() { uri };
                    break;
                case "ReleaseDate":
                    Settings.Instance.FilterSettings.ReleaseDate = uri;
                    break;
                case "Categories":
                    Settings.Instance.FilterSettings.Categories = new List<string>() { uri };
                    break;
                case "Platform":
                    Settings.Instance.FilterSettings.Platforms = new List<string>() { uri };
                    break;
                default:
                    break;
            }
        }
        
        private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            // Sadly we can't use WrapPannel to achieve the same effect and this workaround has to be used 
            // to move description view below other controls when control width becames too small
            // (without streching height of all controls)
            if (e.WidthChanged)
            {
                if (e.NewSize.Width < 450)
                {
                    Grid.SetColumn(ScrollDescription, 0);
                    Grid.SetColumnSpan(ScrollDescription, 2);
                    Grid.SetRow(ScrollDescription, 1);
                }
                else
                {
                    Grid.SetColumn(ScrollDescription, 1);
                    Grid.SetColumnSpan(ScrollDescription, 1);
                    Grid.SetRow(ScrollDescription, 0);
                }
            }
        }
    }
}
