using Playnite.Behaviors;
using Playnite.Common;
using Playnite.Controls;
using Playnite.Converters;
using Playnite.DesktopApp.ViewModels;
using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Xml.Linq;
using BooleanToVisibilityConverter = Playnite.Converters.BooleanToVisibilityConverter;

namespace Playnite.DesktopApp.Controls.Views
{
    [TemplatePart(Name = "PART_SliderZoom", Type = typeof(SliderWithPopup))]
    public class LibraryGridView : BaseGamesView
    {
        private SliderWithPopup SliderZoom;

        private readonly ItemsPanelTemplate groupItemsPanel;
        private readonly ItemsPanelTemplate standardItemsPanel;

        static LibraryGridView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(LibraryGridView), new FrameworkPropertyMetadata(typeof(LibraryGridView)));
        }

        public LibraryGridView() : base(DesktopView.Grid)
        {
            Loaded += LibraryGridView_Loaded;
            Unloaded += LibraryGridView_Unloaded;
            groupItemsPanel = GetItemsPanelTemplate();
            standardItemsPanel = GetItemsPanelTemplate();
        }

        public LibraryGridView(DesktopAppViewModel mainModel) : base(DesktopView.Grid, mainModel)
        {
            Loaded += LibraryGridView_Loaded;
            Unloaded += LibraryGridView_Unloaded;
        }

        private void LibraryGridView_Loaded(object sender, RoutedEventArgs e)
        {
            mainModel.AppSettings.ViewSettings.PropertyChanged += ViewSettings_PropertyChanged;
            mainModel.AppSettings.FilterSettings.FilterChanged += FilterSettings_FilterChanged;
        }

        private void LibraryGridView_Unloaded(object sender, RoutedEventArgs e)
        {
            mainModel.AppSettings.ViewSettings.PropertyChanged -= ViewSettings_PropertyChanged;
            mainModel.AppSettings.FilterSettings.FilterChanged -= FilterSettings_FilterChanged;
        }

        private void ViewSettings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (mainModel.AppSettings.ViewSettings.GamesViewType != viewType)
            {
                return;
            }

            if (e.PropertyName == nameof(ViewSettings.GroupingOrder))
            {
                ListGames.ItemsPanel = GetItemsPanelTemplateCache();
                var scrollViewer = ElementTreeHelper.FindVisualChildren< ScrollViewer>(ListGames).FirstOrDefault();
                scrollViewer?.ScrollToTop();
            }
        }

        private void FilterSettings_FilterChanged(object sender, FilterChangedEventArgs e)
        {
            if (mainModel.AppSettings.ViewSettings.GamesViewType != viewType)
            {
                return;
            }

            var scrollViewer = ElementTreeHelper.FindVisualChildren<ScrollViewer>(ListGames).FirstOrDefault();
            scrollViewer?.ScrollToTop();
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (ListGames != null)
            {
                ListGames.ItemsPanel = GetItemsPanelTemplateCache();
                ScrollViewerBehaviours.SetCustomScrollEnabled(ListGames, true);
                BindingTools.SetBinding(ListGames,
                    ScrollViewerBehaviours.SensitivityProperty,
                    mainModel.AppSettings,
                    nameof(PlayniteSettings.GridViewScrollSensitivity));
                BindingTools.SetBinding(ListGames,
                    ScrollViewerBehaviours.SpeedProperty,
                    mainModel.AppSettings,
                    nameof(PlayniteSettings.GridViewScrollSpeed),
                    converter: new TicksToTimeSpanConverter());
                BindingTools.SetBinding(ListGames,
                    ScrollViewerBehaviours.SmoothScrollEnabledProperty,
                    mainModel.AppSettings,
                    nameof(PlayniteSettings.GridViewSmoothScrollEnabled));
            }

            if (ControlGameView != null)
            {
                BindingTools.SetBinding(ControlGameView,
                    Control.VisibilityProperty,
                    mainModel.AppSettings,
                    nameof(PlayniteSettings.GridViewSideBarVisible),
                    converter: new BooleanToVisibilityConverter());
            }

            SliderZoom = Template.FindName("PART_SliderZoom", this) as SliderWithPopup;
            if (SliderZoom != null)
            {
                SliderZoom.SliderMaximumValue = ViewSettings.MaxGridItemWidth;
                SliderZoom.SliderMinimumValue = ViewSettings.MinGridItemWidth;
                BindingTools.SetBinding(SliderZoom,
                    SliderWithPopup.SliderValueProperty,
                    mainModel.AppSettings,
                    nameof(PlayniteSettings.GridItemWidth),
                    BindingMode.TwoWay);
                BindingTools.SetBinding(SliderZoom,
                    SliderWithPopup.PopupLabelProperty,
                    mainModel.AppSettings,
                    nameof(PlayniteSettings.GridItemWidth),
                    converter: new CoversZoomToPercentageConverter(),
                    stringFormat: "{0}%");
            }
        }

        private ItemsPanelTemplate GetItemsPanelTemplateCache()
        {
            // This fixes an issue where WPF incorrectly sets parent virtualizing panel
            // when switching from standard to gourping views.
            return mainModel.AppSettings.ViewSettings.GroupingOrder == GroupableField.None ? standardItemsPanel : groupItemsPanel;
        }

        private ItemsPanelTemplate GetItemsPanelTemplate()
        {
            XNamespace pns = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
            XNamespace pctrls = "clr-namespace:Playnite.DesktopApp.Controls;assembly=Playnite.DesktopApp";
            var templateDoc = new XDocument(
                new XElement(pns + nameof(ItemsPanelTemplate),
                    new XElement(pctrls + nameof(GridViewPanel))));

            return Xaml.FromString<ItemsPanelTemplate>(templateDoc.ToString());
        }
    }
}
