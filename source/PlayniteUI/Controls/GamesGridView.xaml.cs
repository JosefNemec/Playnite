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
using Playnite.SDK.Models;
using Playnite;
using System.ComponentModel;

namespace PlayniteUI.Controls
{
    /// <summary>
    /// Interaction logic for GamesGridView.xaml
    /// </summary>
    public partial class GamesGridView : UserControl
    {
        public object SelectedItem
        {
            get
            {
                return GetValue(SelectedItemProperty);
            }

            set
            {
                SetValue(SelectedItemProperty, value);
            }
        }

        public static readonly DependencyProperty SelectedItemProperty =
           DependencyProperty.Register("SelectedItem", typeof(object), typeof(GamesGridView), new PropertyMetadata(null, OnSelectedItemChange));

        private static void OnSelectedItemChange(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var obj = sender as GamesGridView;
            obj.GridGames.SelectedItem = e.NewValue;                
        }

        public IList<object> SelectedItemsList
        {
            get
            {
                return (IList<object>)GetValue(SelectedItemsListProperty);
            }

            set
            {
                SetValue(SelectedItemsListProperty, value);
            }
        }

        public static readonly DependencyProperty SelectedItemsListProperty =
           DependencyProperty.Register("SelectedItemsList", typeof(IList<object>), typeof(GamesGridView), new PropertyMetadata(null));

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
            GridGames.SelectionChanged += GridGames_SelectionChanged;
        }

        private void GridGames_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (GridGames.SelectedItems?.Count == 1)
            {
                SelectedItem = GridGames.SelectedItems[0];
            }

            SelectedItemsList = (IList<object>)GridGames.SelectedItems;
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
                if (game.PluginId == null)
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

            if (AppSettings.ViewSettings.SortingOrder == sortOrder)
            {
                AppSettings.ViewSettings.SortingOrderDirection = AppSettings.ViewSettings.SortingOrderDirection == SortOrderDirection.Ascending ? SortOrderDirection.Descending : SortOrderDirection.Ascending;
            }
            else
            {
                AppSettings.ViewSettings.SortingOrder = sortOrder;
            }
        }
    }
}
