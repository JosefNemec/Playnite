using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Playnite.Database;
using Playnite.Models;
using PlayniteUI.Windows;

namespace PlayniteUI.Controls
{
    /// <summary>
    /// Interaction logic for GamePopupMenu.xaml
    /// </summary>
    public partial class GamePopupMenu : Popup, INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private bool showPlayInstallButton = true;
        public bool ShowPlayInstallButton
        {
            get
            {
                return showPlayInstallButton;
            }

            set
            {
                showPlayInstallButton = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("ShowPlayInstallButton"));
            }
        }

        public GamePopupMenu()
        {
            InitializeComponent();
            DataContextChanged += GamePopupMenu_DataContextChanged;
        }

        private void GamePopupMenu_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue == null)
            {
                return;
            }

            var game = (IGame)e.NewValue;
            if (game.Provider == Provider.Custom && !game.IsInstalled)
            {
                SeparatorPlayInstall.Visibility = Visibility.Collapsed;
            }
            else
            {
                SeparatorPlayInstall.Visibility = Visibility.Visible;
            }

            if (game.OtherTasks == null || game.OtherTasks.Count == 0)
            {
                SeparatorTasks.Visibility = Visibility.Collapsed;
            }
            else
            {
                SeparatorTasks.Visibility = Visibility.Visible;
            }
        }

        private void Task_Click(object sender, RoutedEventArgs e)
        {
            var gameTask = (GameTask)(sender as FrameworkElement).DataContext;
            try
            {
                gameTask.Activate();
            }
            catch (Exception exc)
            {
                MessageBox.Show("Cannot start action: " + exc.Message, "Action Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsOpen = false;
            }
        }

        private void Play_Click(object sender, RoutedEventArgs e)
        {
            var game = (IGame)(sender as FrameworkElement).DataContext;
            GamesEditor.Instance.PlayGame(game);
            IsOpen = false;
        }

        private void Install_Click(object sender, RoutedEventArgs e)
        {
            var game = (IGame)(sender as FrameworkElement).DataContext;
            GamesEditor.Instance.InstallGame(game);
            IsOpen = false;
        }

        private void Uninstall_Click(object sender, RoutedEventArgs e)
        {
            var game = (IGame)(sender as FrameworkElement).DataContext;
            GamesEditor.Instance.UnInstallGame(game);
            IsOpen = false;
        }

        private void Edit_Click(object sender, RoutedEventArgs e)
        {
            GamesEditor.Instance.EditGame((IGame)DataContext);
            IsOpen = false;
        }

        private void Category_Click(object sender, RoutedEventArgs e)
        {
            GamesEditor.Instance.SetGameCategories((IGame)DataContext);
            IsOpen = false;
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show(
                "Are you sure you want to remove this game?",
                "Remove game?",
                MessageBoxButton.YesNo) != MessageBoxResult.Yes)
            {
                return;
            }

            if (DataContext == null)
            {
                return;
            }

            var game = (DataContext as IGame);
            if (game is Game)
            {
                GameDatabase.Instance.DeleteGame(game);
            }

            IsOpen = false;
        }

        private void OpenLocation_Click(object sender, RoutedEventArgs e)
        {
            var game = (IGame)(sender as FrameworkElement).DataContext;
            try
            {
                Process.Start(game.InstallDirectory);
            }
            catch (Exception exc)
            {
                MessageBox.Show("Cannot open game location: " + exc.Message, "Game Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsOpen = false;
            }
        }

        private void Hide_Click(object sender, RoutedEventArgs e)
        {
            var game = (IGame)(sender as FrameworkElement).DataContext;
            game.Hidden = !game.Hidden;
            GameDatabase.Instance.UpdateGameInDatabase(game);
            IsOpen = false;
        }
    }
}
