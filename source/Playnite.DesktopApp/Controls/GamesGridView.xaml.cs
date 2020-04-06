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
using Playnite.Settings;

namespace Playnite.DesktopApp.Controls
{
    public class GamesGridViewColumn : GridViewColumn
    {
        public GameField Field { get; set; }

        public SortOrder? SortOrder { get; set; }

        public GamesGridViewColumn() : base()
        {
        }
    }

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
           DependencyProperty.Register(nameof(SelectedItem), typeof(object), typeof(GamesGridView), new PropertyMetadata(null, OnSelectedItemChange));

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
           DependencyProperty.Register(nameof(SelectedItemsList), typeof(IList<object>), typeof(GamesGridView), new PropertyMetadata(null));

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

        public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable), typeof(GamesGridView));

        public PlayniteSettings AppSettings
        {
            get
            {
                return (PlayniteSettings)GetValue(AppSettingsProperty);
            }

            set
            {
                SetValue(AppSettingsProperty, value);
            }
        }

        public static readonly DependencyProperty AppSettingsProperty = DependencyProperty.Register(nameof(AppSettings), typeof(PlayniteSettings), typeof(GamesGridView));

        private bool ignoreColumnChanges = false;

        public GamesGridView()
        {
            InitializeComponent();
            GridGames.SelectionChanged += GridGames_SelectionChanged;
            ActualGridView.Columns.CollectionChanged += Columns_CollectionChanged;
            Loaded += GamesGridView_Loaded;
            Unloaded += GamesGridView_Unloaded;
        }

        private void GamesGridView_Unloaded(object sender, RoutedEventArgs e)
        {
            AppSettings.ViewSettings.ListViewColumns.Added.PropertyChanged -= ListViewColumn_PropertyChanged;
            AppSettings.ViewSettings.ListViewColumns.AgeRating.PropertyChanged -= ListViewColumn_PropertyChanged;
            AppSettings.ViewSettings.ListViewColumns.Categories.PropertyChanged -= ListViewColumn_PropertyChanged;
            AppSettings.ViewSettings.ListViewColumns.CommunityScore.PropertyChanged -= ListViewColumn_PropertyChanged;
            AppSettings.ViewSettings.ListViewColumns.CompletionStatus.PropertyChanged -= ListViewColumn_PropertyChanged;
            AppSettings.ViewSettings.ListViewColumns.CriticScore.PropertyChanged -= ListViewColumn_PropertyChanged;
            AppSettings.ViewSettings.ListViewColumns.Developers.PropertyChanged -= ListViewColumn_PropertyChanged;
            AppSettings.ViewSettings.ListViewColumns.Features.PropertyChanged -= ListViewColumn_PropertyChanged;
            AppSettings.ViewSettings.ListViewColumns.Genres.PropertyChanged -= ListViewColumn_PropertyChanged;
            AppSettings.ViewSettings.ListViewColumns.Icon.PropertyChanged -= ListViewColumn_PropertyChanged;
            AppSettings.ViewSettings.ListViewColumns.InstallDirectory.PropertyChanged -= ListViewColumn_PropertyChanged;
            AppSettings.ViewSettings.ListViewColumns.IsInstalled.PropertyChanged -= ListViewColumn_PropertyChanged;
            AppSettings.ViewSettings.ListViewColumns.LastActivity.PropertyChanged -= ListViewColumn_PropertyChanged;
            AppSettings.ViewSettings.ListViewColumns.Modified.PropertyChanged -= ListViewColumn_PropertyChanged;
            AppSettings.ViewSettings.ListViewColumns.Name.PropertyChanged -= ListViewColumn_PropertyChanged;
            AppSettings.ViewSettings.ListViewColumns.Platform.PropertyChanged -= ListViewColumn_PropertyChanged;
            AppSettings.ViewSettings.ListViewColumns.PlayCount.PropertyChanged -= ListViewColumn_PropertyChanged;
            AppSettings.ViewSettings.ListViewColumns.Playtime.PropertyChanged -= ListViewColumn_PropertyChanged;
            AppSettings.ViewSettings.ListViewColumns.PluginId.PropertyChanged -= ListViewColumn_PropertyChanged;
            AppSettings.ViewSettings.ListViewColumns.Publishers.PropertyChanged -= ListViewColumn_PropertyChanged;
            AppSettings.ViewSettings.ListViewColumns.Region.PropertyChanged -= ListViewColumn_PropertyChanged;
            AppSettings.ViewSettings.ListViewColumns.ReleaseDate.PropertyChanged -= ListViewColumn_PropertyChanged;
            AppSettings.ViewSettings.ListViewColumns.Series.PropertyChanged -= ListViewColumn_PropertyChanged;
            AppSettings.ViewSettings.ListViewColumns.Source.PropertyChanged -= ListViewColumn_PropertyChanged;
            AppSettings.ViewSettings.ListViewColumns.Tags.PropertyChanged -= ListViewColumn_PropertyChanged;
            AppSettings.ViewSettings.ListViewColumns.UserScore.PropertyChanged -= ListViewColumn_PropertyChanged;
            AppSettings.ViewSettings.ListViewColumns.Version.PropertyChanged -= ListViewColumn_PropertyChanged;
        }

        private void GamesGridView_Loaded(object sender, RoutedEventArgs e)
        {
            InitializeColumns();
            AppSettings.ViewSettings.ListViewColumns.Added.PropertyChanged += ListViewColumn_PropertyChanged;
            AppSettings.ViewSettings.ListViewColumns.AgeRating.PropertyChanged += ListViewColumn_PropertyChanged;
            AppSettings.ViewSettings.ListViewColumns.Categories.PropertyChanged += ListViewColumn_PropertyChanged;
            AppSettings.ViewSettings.ListViewColumns.CommunityScore.PropertyChanged += ListViewColumn_PropertyChanged;
            AppSettings.ViewSettings.ListViewColumns.CompletionStatus.PropertyChanged += ListViewColumn_PropertyChanged;
            AppSettings.ViewSettings.ListViewColumns.CriticScore.PropertyChanged += ListViewColumn_PropertyChanged;
            AppSettings.ViewSettings.ListViewColumns.Developers.PropertyChanged += ListViewColumn_PropertyChanged;
            AppSettings.ViewSettings.ListViewColumns.Features.PropertyChanged += ListViewColumn_PropertyChanged;
            AppSettings.ViewSettings.ListViewColumns.Genres.PropertyChanged += ListViewColumn_PropertyChanged;
            AppSettings.ViewSettings.ListViewColumns.Icon.PropertyChanged += ListViewColumn_PropertyChanged;
            AppSettings.ViewSettings.ListViewColumns.InstallDirectory.PropertyChanged += ListViewColumn_PropertyChanged;
            AppSettings.ViewSettings.ListViewColumns.IsInstalled.PropertyChanged += ListViewColumn_PropertyChanged;
            AppSettings.ViewSettings.ListViewColumns.LastActivity.PropertyChanged += ListViewColumn_PropertyChanged;
            AppSettings.ViewSettings.ListViewColumns.Modified.PropertyChanged += ListViewColumn_PropertyChanged;
            AppSettings.ViewSettings.ListViewColumns.Name.PropertyChanged += ListViewColumn_PropertyChanged;
            AppSettings.ViewSettings.ListViewColumns.Platform.PropertyChanged += ListViewColumn_PropertyChanged;
            AppSettings.ViewSettings.ListViewColumns.PlayCount.PropertyChanged += ListViewColumn_PropertyChanged;
            AppSettings.ViewSettings.ListViewColumns.Playtime.PropertyChanged += ListViewColumn_PropertyChanged;
            AppSettings.ViewSettings.ListViewColumns.PluginId.PropertyChanged += ListViewColumn_PropertyChanged;
            AppSettings.ViewSettings.ListViewColumns.Publishers.PropertyChanged += ListViewColumn_PropertyChanged;
            AppSettings.ViewSettings.ListViewColumns.Region.PropertyChanged += ListViewColumn_PropertyChanged;
            AppSettings.ViewSettings.ListViewColumns.ReleaseDate.PropertyChanged += ListViewColumn_PropertyChanged;
            AppSettings.ViewSettings.ListViewColumns.Series.PropertyChanged += ListViewColumn_PropertyChanged;
            AppSettings.ViewSettings.ListViewColumns.Source.PropertyChanged += ListViewColumn_PropertyChanged;
            AppSettings.ViewSettings.ListViewColumns.Tags.PropertyChanged += ListViewColumn_PropertyChanged;
            AppSettings.ViewSettings.ListViewColumns.UserScore.PropertyChanged += ListViewColumn_PropertyChanged;
            AppSettings.ViewSettings.ListViewColumns.Version.PropertyChanged += ListViewColumn_PropertyChanged;
        }

        private void ListViewColumn_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(ListViewColumnProperty.Visible))
            {
                var prop = sender as ListViewColumnProperty;
                if (AppSettings.ViewSettings.ListViewColumsOrder.Contains(prop.Field) &&
                    !prop.Visible)
                {
                    var column = ActualGridView.Columns.FirstOrDefault(a => ((GamesGridViewColumn)a).Field == prop.Field);
                    if (column != null)
                    {
                        ActualGridView.Columns.Remove(column);
                    }
                }
                else if (!AppSettings.ViewSettings.ListViewColumsOrder.Contains(prop.Field) &&
                          prop.Visible)
                {
                    var newColumn = GetColumn(prop.Field);
                    if (newColumn != null)
                    {
                        ActualGridView.Columns.Add(newColumn);
                    }
                }
            }
        }

        private void InitializeColumns()
        {
            ignoreColumnChanges = true;

            foreach (var field in AppSettings.ViewSettings.ListViewColumsOrder)
            {
                var newColumn = GetColumn(field);
                if (newColumn != null)
                {
                    ActualGridView.Columns.Add(newColumn);
                }
            }

            ignoreColumnChanges = false;
        }

        private void Columns_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (ignoreColumnChanges)
            {
                return;
            }

            AppSettings.ViewSettings.ListViewColumsOrder = ActualGridView.Columns.Select(a => ((GamesGridViewColumn)a).Field).ToList();
        }

        private GamesGridViewColumn GetColumn(GameField field)
        {
            GamesGridViewColumn newColumn = null;
            if (field == GameField.Icon)
            {
                newColumn = CreateColumn(field, null, "CellTemplateIcon", null);
            }
            else if (field == GameField.Name)
            {
                newColumn = CreateColumn(field, SortOrder.Name, "CellTemplateName", "HeaderTemplateName");
            }
            else if (field == GameField.Platform)
            {
                newColumn = CreateColumn(field, SortOrder.Platform, "CellTemplatePlatform", "HeaderTemplatePlatform");
            }
            else if (field == GameField.PluginId)
            {
                newColumn = CreateColumn(field, SortOrder.Library, "CellTemplateLibrary", "HeaderTemplateLibrary");
            }
            else if (field == GameField.Developers)
            {
                newColumn = CreateColumn(field, SortOrder.Developers, "CellTemplateDevelopers", "HeaderTemplateDevelopers");
            }
            else if (field == GameField.Publishers)
            {
                newColumn = CreateColumn(field, SortOrder.Publishers, "CellTemplatePublishers", "HeaderTemplatePublishers");
            }
            else if (field == GameField.ReleaseDate)
            {
                newColumn = CreateColumn(field, SortOrder.ReleaseDate, "CellTemplateReleaseDate", "HeaderTemplateReleaseDate");
            }
            else if (field == GameField.Genres)
            {
                newColumn = CreateColumn(field, SortOrder.Genres, "CellTemplateGenres", "HeaderTemplateGenres");
            }
            else if (field == GameField.Categories)
            {
                newColumn = CreateColumn(field, SortOrder.Categories, "CellTemplateCategories", "HeaderTemplateCategories");
            }
            else if (field == GameField.Features)
            {
                newColumn = CreateColumn(field, SortOrder.Features, "CellTemplateFeatures", "HeaderTemplateFeatures");
            }
            else if (field == GameField.Tags)
            {
                newColumn = CreateColumn(field, SortOrder.Tags, "CellTemplateTags", "HeaderTemplateTags");
            }
            else if (field == GameField.IsInstalled)
            {
                newColumn = CreateColumn(field, SortOrder.IsInstalled, "CellTemplateIsInstalled", "HeaderTemplateIsInstalled");
            }
            else if (field == GameField.InstallDirectory)
            {
                newColumn = CreateColumn(field, SortOrder.InstallDirectory, "CellTemplateInstallDirectory", "HeaderTemplateInstallDirectory");
            }
            else if (field == GameField.LastActivity)
            {
                newColumn = CreateColumn(field, SortOrder.LastActivity, "CellTemplateLastActivity", "HeaderTemplateLastActivity");
            }
            else if (field == GameField.Playtime)
            {
                newColumn = CreateColumn(field, SortOrder.Playtime, "CellTemplatePlaytime", "HeaderTemplatePlaytime");
            }
            else if (field == GameField.PlayCount)
            {
                newColumn = CreateColumn(field, SortOrder.PlayCount, "CellTemplatePlayCount", "HeaderTemplatePlayCount");
            }
            else if (field == GameField.CompletionStatus)
            {
                newColumn = CreateColumn(field, SortOrder.CompletionStatus, "CellTemplateCompletionStatus", "HeaderTemplateCompletionStatus");
            }
            else if (field == GameField.Series)
            {
                newColumn = CreateColumn(field, SortOrder.Series, "CellTemplateSeries", "HeaderTemplateSeries");
            }
            else if (field == GameField.Version)
            {
                newColumn = CreateColumn(field, SortOrder.Version, "CellTemplateVersion", "HeaderTemplateVersion");
            }
            else if (field == GameField.AgeRating)
            {
                newColumn = CreateColumn(field, SortOrder.AgeRating, "CellTemplateAgeRating", "HeaderTemplateAgeRating");
            }
            else if (field == GameField.Region)
            {
                newColumn = CreateColumn(field, SortOrder.Region, "CellTemplateRegion", "HeaderTemplateRegion");
            }
            else if (field == GameField.Source)
            {
                newColumn = CreateColumn(field, SortOrder.Source, "CellTemplateSource", "HeaderTemplateSource");
            }
            else if (field == GameField.Added)
            {
                newColumn = CreateColumn(field, SortOrder.Added, "CellTemplateAdded", "HeaderTemplateAdded");
            }
            else if (field == GameField.Modified)
            {
                newColumn = CreateColumn(field, SortOrder.Modified, "CellTemplateModified", "HeaderTemplateModified");
            }
            else if (field == GameField.UserScore)
            {
                newColumn = CreateColumn(field, SortOrder.UserScore, "CellTemplateUserScore", "HeaderTemplateUserScore");
            }
            else if (field == GameField.CriticScore)
            {
                newColumn = CreateColumn(field, SortOrder.CriticScore, "CellTemplateCriticScore", "HeaderTemplateCriticScore");
            }
            else if (field == GameField.CommunityScore)
            {
                newColumn = CreateColumn(field, SortOrder.CommunityScore, "CellTemplateCommunityScore", "HeaderTemplateCommunityScore");
            }

            return newColumn;
        }

        private GamesGridViewColumn CreateColumn(GameField field, SortOrder? sortOrder, string cellTemplateName, string headerTemplateName)
        {
            var column = new GamesGridViewColumn
            {
                Field = field,
                SortOrder = sortOrder
            };

            BindingOperations.SetBinding(
                column,
                GridViewColumn.WidthProperty,
                new Binding($"{nameof(ViewSettings.ListViewColumns)}.{field.ToString()}.{nameof(ListViewColumnProperty.Width)}")
                {
                    Source = AppSettings.ViewSettings,
                    Mode = BindingMode.TwoWay
                });

            column.CellTemplate = Resources[cellTemplateName] as DataTemplate;
            if (!headerTemplateName.IsNullOrEmpty())
            {
                column.HeaderTemplate = Resources[headerTemplateName] as DataTemplate;
            }

            return column;
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

            var entry = (GamesCollectionViewEntry)GridGames.SelectedItem;
            var game = entry.Game;
            if (game.IsInstalled)
            {
                PlayniteApplication.Current.GamesEditor.PlayGame(game);
            }
            else
            {
                if (game.IsCustomGame)
                {
                    ((DesktopGamesEditor)DesktopApplication.Current.GamesEditor).EditGame(game);
                }
                else
                {
                    DesktopApplication.Current.GamesEditor.InstallGame(game);
                }
            }
        }

        private void GridViewColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            var header = sender as GridViewColumnHeader;
            if (header == null)
            {
                return;
            }

            var column = header.Column as GamesGridViewColumn;
            if (column == null || column.SortOrder == null)
            {
                return;
            }

            if (AppSettings.ViewSettings.SortingOrder == column.SortOrder.Value)
            {
                AppSettings.ViewSettings.SortingOrderDirection = AppSettings.ViewSettings.SortingOrderDirection == SortOrderDirection.Ascending ? SortOrderDirection.Descending : SortOrderDirection.Ascending;
            }
            else
            {
                AppSettings.ViewSettings.SortingOrder = column.SortOrder.Value;
            }
        }
    }
}
