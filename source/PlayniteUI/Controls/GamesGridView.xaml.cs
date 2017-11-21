using System;
using System.Collections;
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
using Playnite.Database;
using Playnite.Models;
using Playnite;
using System.ComponentModel;

namespace PlayniteUI.Controls
{
    /// <summary>
    /// Interaction logic for GamesGridView.xaml
    /// </summary>
    public partial class GamesGridView : UserControl
    {
        public IEnumerable ItemsSource
        {
            get
            {
                return (IEnumerable)GetValue(ItemsSourceProperty);
            }

            set
            {
                SetValue(ItemsSourceProperty, value);
            }
        }

        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(GamesGridView));
        
        public Settings AppSettings
        {
            get
            {
                return (Settings)GetValue(AppSettingsProperty);
            }

            set
            {
                SetValue(AppSettingsProperty, value);
            }
        }

        public static readonly DependencyProperty AppSettingsProperty = DependencyProperty.Register("AppSettings", typeof(Settings), typeof(GamesGridView));

        public GamesGridView()
        {
            InitializeComponent();
        }

        private void Grid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed || GridGames.SelectedItem == null)
            {
                return;
            }

            var entry = (GameViewEntry)GridGames.SelectedItem;
            var game = entry.Game;
            if (game.IsInstalled)
            {
                App.GamesEditor.PlayGame(game);
            }
            else
            {
                if (game.Provider == Provider.Custom)
                {
                    App.GamesEditor.EditGame(game);
                }
                else
                {
                    App.GamesEditor.InstallGame(game);
                }
            }
        }

        private void GridViewColumnHeader_Loaded(object sender, RoutedEventArgs e)
        {
            var header = sender as GridViewColumnHeader;
            if (header.Tag == null)
            {
                return;
            }

            BindingOperations.SetBinding(header, GridViewColumnHeader.VisibilityProperty, new Binding(string.Format("GridViewHeaders[{0}]", header.Tag.ToString()))
            {
                Source = AppSettings,
                Mode = BindingMode.OneWay,
                Converter = new BooleanToVisibilityConverter()
            });
        }

        private void GridViewColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            var header = sender as GridViewColumnHeader;            
            if (header == null || header.Tag == null)
            {
                return;
            }

            var sortOrder = (SortOrder)header.Tag;
            if (sortOrder == SortOrder.Icon)
            {
                return;
            }

            if (AppSettings.SortingOrder == sortOrder)
            {
                AppSettings.SortingOrderDirection = AppSettings.SortingOrderDirection == SortOrderDirection.Ascending ? SortOrderDirection.Descending : SortOrderDirection.Ascending;
            }
            else
            {
                AppSettings.SortingOrder = sortOrder;
            }
        }

        private void Expander_Collapsed(object sender, RoutedEventArgs e)
        {
            var group = (CollectionViewGroup)((Expander)sender).DataContext;
            if (group.Name is CategoryView category)
            {
                if (!Settings.Instance.CollapsedCategories.Contains(category.Category))
                {
                    Settings.Instance.CollapsedCategories.Add(category.Category);
                }
            }
        }

        private void Expander_Expanded(object sender, RoutedEventArgs e)
        {
            var group = (CollectionViewGroup)((Expander)sender).DataContext;
            if (group.Name is CategoryView category)
            {
                if (Settings.Instance.CollapsedCategories.Contains(category.Category))
                {
                    Settings.Instance.CollapsedCategories.Remove(category.Category);
                }
            }
        }
    }
}
