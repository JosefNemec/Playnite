using Playnite.Behaviors;
using Playnite.Common;
using Playnite.Converters;
using Playnite.DesktopApp.ViewModels;
using Playnite.SDK;
using Playnite.SDK.Plugins;
using Playnite.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using BooleanToVisibilityConverter = System.Windows.Controls.BooleanToVisibilityConverter;

namespace Playnite.DesktopApp.Controls.Views
{
    [TemplatePart(Name = "PART_ElemMainMenu", Type = typeof(FrameworkElement))]
    [TemplatePart(Name = "PART_TextMainSearch", Type = typeof(SearchBox))]
    [TemplatePart(Name = "PART_ToggleFilter", Type = typeof(ToggleButton))]
    [TemplatePart(Name = "PART_ToggleNotifications", Type = typeof(ToggleButton))]
    [TemplatePart(Name = "PART_ProgressGlobal", Type = typeof(ProgressBar))]
    [TemplatePart(Name = "PART_TextProgressText", Type = typeof(TextBlock))]
    [TemplatePart(Name = "PART_ButtonProgressCancel", Type = typeof(ButtonBase))]
    [TemplatePart(Name = "PART_PanelMainItems", Type = typeof(Panel))]
    [TemplatePart(Name = "PART_PanelMainPluginItems", Type = typeof(Panel))]
    public class TopPanel : Control
    {
        private readonly DesktopAppViewModel mainModel;
        private FrameworkElement ElemMainMenu;
        private SearchBox TextMainSearch;
        private ToggleButton ToggleFilter;
        private ToggleButton ToggleNotifications;
        private ProgressBar ProgressGlobal;
        private TextBlock TextProgressText;
        private ButtonBase ButtonProgressCancel;
        private Panel PanelMainItems;
        private Panel PanelMainPluginItems;

        private TopPanelWrapperItem ButtonViewSettings;
        private TopPanelWrapperItem ButtonGroupSettings;
        private TopPanelWrapperItem ButtonSortSettings;
        private TopPanelWrapperItem ButtonFilterPresets;
        private TopPanelWrapperItem ButtonExplorerSwitch;
        private TopPanelWrapperItem ButtonSearch;

        private TopPanelWrapperItem ButtonSwitchDetailsView;
        private TopPanelWrapperItem ButtonSwitchGridView;
        private TopPanelWrapperItem ButtonSwitchListView;
        private TopPanelWrapperItem ButtonSelectRandomGame;
        private TopPanelWrapperItem ButtonViewSelectRandomGame;

        private Canvas LeftViewSeparator = new Canvas();
        private Canvas RightViewSeparator = new Canvas();

        static TopPanel()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TopPanel), new FrameworkPropertyMetadata(typeof(TopPanel)));
        }

        public TopPanel() : this(DesktopApplication.Current?.MainModel)
        {
        }

        public TopPanel(DesktopAppViewModel mainModel)
        {
            if (DesignerProperties.GetIsInDesignMode(this))
            {
                this.mainModel = DesignMainViewModel.DesignIntance;
            }
            else if (mainModel != null)
            {
                this.mainModel = mainModel;
            }

            Loaded += TopPanel_Loaded;
            Unloaded += TopPanel_Unloaded;
        }

        private void TopPanel_Loaded(object sender, RoutedEventArgs e)
        {
            mainModel.AppSettings.PropertyChanged += AppSettings_PropertyChanged;
        }

        private void TopPanel_Unloaded(object sender, RoutedEventArgs e)
        {
            mainModel.AppSettings.PropertyChanged -= AppSettings_PropertyChanged;
        }

        private void AppSettings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName.StartsWith("ShowTopPanel"))
            {
                SetButtonVisibility();
            }
            else if (e.PropertyName == nameof(PlayniteSettings.TopPanelSectionSeparatorWidth))
            {
                LeftViewSeparator.Width = mainModel.AppSettings.TopPanelSectionSeparatorWidth;
                RightViewSeparator.Width = mainModel.AppSettings.TopPanelSectionSeparatorWidth;
            }
        }

        private void SetButtonVisibility()
        {
            ButtonViewSettings.PanelItem.Visible = mainModel.AppSettings.ShowTopPanelGeneralViewItem;
            ButtonGroupSettings.PanelItem.Visible = mainModel.AppSettings.ShowTopPanelGroupingItem;
            ButtonSortSettings.PanelItem.Visible = mainModel.AppSettings.ShowTopPanelSortingItem;
            ButtonFilterPresets.PanelItem.Visible = mainModel.AppSettings.ShowTopPanelFilterPresetsItem;
            ButtonExplorerSwitch.PanelItem.Visible = mainModel.AppSettings.ShowTopPanelExplorerSwitch;
            ButtonSearch.PanelItem.Visible = mainModel.AppSettings.ShowTopPanelSearchButton;

            ButtonSwitchDetailsView.PanelItem.Visible = mainModel.AppSettings.ShowTopPanelDetailsViewSwitch;
            ButtonSwitchGridView.PanelItem.Visible = mainModel.AppSettings.ShowTopPanelGridViewSwitch;
            ButtonSwitchListView.PanelItem.Visible = mainModel.AppSettings.ShowTopPanelListViewSwitch;
            ButtonSelectRandomGame.PanelItem.Visible = mainModel.AppSettings.ShowTopPanelSelectRandomGameButton;
            ButtonViewSelectRandomGame.PanelItem.Visible = mainModel.AppSettings.ShowTopPanelViewSelectRandomGameButton;

            var showSeparators = ButtonSwitchDetailsView.Visible || ButtonSwitchGridView.Visible || ButtonSwitchListView.Visible;
            LeftViewSeparator.Visibility = showSeparators ? Visibility.Visible : Visibility.Collapsed;
            RightViewSeparator.Visibility = showSeparators ? Visibility.Visible : Visibility.Collapsed;
        }

        private TopPanelItem AssignPanelButton(string contentTemplate, ContextMenu menu, string tooltip, out TopPanelWrapperItem panelItem)
        {
            tooltip = tooltip.StartsWith("LOC") ? ResourceProvider.GetString(tooltip) : tooltip;
            panelItem = new TopPanelWrapperItem(new SDK.Plugins.TopPanelItem { Title = tooltip }, mainModel);
            var item = new TopPanelItem() { DataContext = panelItem };
            item.SetResourceReference(TopPanelItem.ContentTemplateProperty, contentTemplate);
            LeftClickContextMenuBehavior.SetEnabled(item, true);
            menu.SetResourceReference(ContextMenu.StyleProperty, "TopPanelMenu");
            item.ContextMenu = menu;
            return item;
        }

        private TopPanelItem AssignPanelButton(string contentTemplate, RelayCommandBase command, string tooltip, out TopPanelWrapperItem panelItem)
        {
            tooltip = tooltip.StartsWith("LOC") ? ResourceProvider.GetString(tooltip) : tooltip;
            panelItem = new TopPanelWrapperItem(new SDK.Plugins.TopPanelItem { Title = tooltip }, mainModel)
            {
                Command = command
            };

            var item = new TopPanelItem() { DataContext = panelItem };
            item.SetResourceReference(TopPanelItem.ContentTemplateProperty, contentTemplate);
            return item;
        }

        private TopPanelItem AssignPluginButton(TopPanelWrapperItem item)
        {
            var button = new TopPanelItem() { DataContext = item };
            return button;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            PanelMainItems = Template.FindName("PART_PanelMainItems", this) as Panel;
            if (PanelMainItems != null)
            {
                PanelMainItems.Children.Add(AssignPanelButton("TopPanelSearchButtonTemplate", mainModel.OpenGlobalSearchCommand, LOC.OpenSearch, out ButtonSearch));
                PanelMainItems.Children.Add(AssignPanelButton("TopPanelGeneralViewSettingsTemplate", new ViewSettingsMenu(mainModel.AppSettings), LOC.SettingsTopPanelGeneralViewItem, out ButtonViewSettings));
                PanelMainItems.Children.Add(AssignPanelButton("TopPanelFilterPresetsSelectionTemplate", new FilterPresetsMenu(mainModel), LOC.SettingsTopPanelFilterPresetsItem, out ButtonFilterPresets));
                PanelMainItems.Children.Add(AssignPanelButton("TopPanelGroupSettingsTemplate", new GroupSettingsMenu(mainModel.AppSettings), LOC.SettingsTopPanelGroupingItem, out ButtonGroupSettings));
                PanelMainItems.Children.Add(AssignPanelButton("TopPanelSortSettingsTemplate", new SortSettingsMenu(mainModel.AppSettings), LOC.SettingsTopPanelSortingItem, out ButtonSortSettings));

                LeftViewSeparator.Width = mainModel.AppSettings.TopPanelSectionSeparatorWidth;
                RightViewSeparator.Width = mainModel.AppSettings.TopPanelSectionSeparatorWidth;
                PanelMainItems.Children.Add(LeftViewSeparator);

                var detailsButton = AssignPanelButton("TopPanelSwitchDetailsViewTemplate", mainModel.SwitchDetailsViewCommand, DesktopView.Details.GetDescription(), out ButtonSwitchDetailsView);
                BindingTools.SetBinding(detailsButton,
                    TopPanelItem.IsToggledProperty,
                    mainModel.AppSettings.ViewSettings,
                    nameof(ViewSettings.GamesViewType),
                    converter: new EnumToBooleanConverter(),
                    converterParameter: DesktopView.Details);
                PanelMainItems.Children.Add(detailsButton);

                var gridButton = AssignPanelButton("TopPanelSwitchGridViewTemplate", mainModel.SwitchGridViewCommand, DesktopView.Grid.GetDescription(), out ButtonSwitchGridView);
                BindingTools.SetBinding(gridButton,
                    TopPanelItem.IsToggledProperty,
                    mainModel.AppSettings.ViewSettings,
                    nameof(ViewSettings.GamesViewType),
                    converter: new EnumToBooleanConverter(),
                    converterParameter: DesktopView.Grid);
                PanelMainItems.Children.Add(gridButton);

                var listButton = AssignPanelButton("TopPanelSwitchListViewTemplate", mainModel.SwitchListViewCommand, DesktopView.List.GetDescription(), out ButtonSwitchListView);
                BindingTools.SetBinding(listButton,
                    TopPanelItem.IsToggledProperty,
                    mainModel.AppSettings.ViewSettings,
                    nameof(ViewSettings.GamesViewType),
                    converter: new EnumToBooleanConverter(),
                    converterParameter: DesktopView.List);
                PanelMainItems.Children.Add(listButton);
                PanelMainItems.Children.Add(RightViewSeparator);

                var updatesButton = AssignPanelButton("TopPanelUpdateButtonTemplate", mainModel.OpenUpdatesCommand, ResourceProvider.GetString(LOC.UpdateIsAvailableNotificationBody), out _);
                BindingTools.SetBinding(updatesButton,
                    Button.VisibilityProperty,
                    mainModel,
                    nameof(mainModel.UpdatesAvailable),
                    converter: new BooleanToVisibilityConverter());
                PanelMainItems.Children.Add(updatesButton);
                PanelMainItems.Children.Add(AssignPanelButton("TopPanelSelectRandomGameButtonTemplate", mainModel.SelectRandomGameCommand, ResourceProvider.GetString(LOC.TopPanelSelectRandomGameButton), out ButtonSelectRandomGame));
                PanelMainItems.Children.Add(AssignPanelButton("TopPanelViewSelectRandomGameButtonTemplate", mainModel.ViewSelectRandomGameCommand, ResourceProvider.GetString(LOC.TopPanelViewSelectRandomGameButtonTooltip), out ButtonViewSelectRandomGame));
                PanelMainItems.Children.Add(AssignPanelButton("TopPanelExplorerSwitchTemplate", mainModel.ToggleExplorerPanelCommand, ResourceProvider.GetString(LOC.TopPanelExplorerSwitch), out ButtonExplorerSwitch));
                SetButtonVisibility();
            }

            PanelMainPluginItems = Template.FindName("PART_PanelMainPluginItems", this) as Panel;
            if (PanelMainPluginItems != null)
            {
                PanelMainPluginItems.Children.Clear();
                foreach (var item in mainModel.GetTopPanelPluginItems())
                {
                    PanelMainPluginItems.Children.Add(AssignPluginButton(item));
                }
            }

            ElemMainMenu = Template.FindName("PART_ElemMainMenu", this) as FrameworkElement;
            if (ElemMainMenu != null)
            {
                LeftClickContextMenuBehavior.SetEnabled(ElemMainMenu, true);
                ElemMainMenu.ContextMenu = new MainMenu(mainModel);
                ElemMainMenu.ContextMenu.SetResourceReference(ContextMenu.StyleProperty, "TopPanelMenu");

                BindingTools.SetBinding(ElemMainMenu,
                    FrameworkElement.VisibilityProperty,
                    mainModel.AppSettings,
                    nameof(PlayniteSettings.ShowMainMenuOnTopPanel),
                    converter: new BooleanToVisibilityConverter());
            }

            TextMainSearch = Template.FindName("PART_TextMainSearch", this) as SearchBox;
            if (TextMainSearch != null)
            {
                BindingTools.SetBinding(TextMainSearch,
                    SearchBox.TextProperty,
                    mainModel.AppSettings.FilterSettings,
                    nameof(FilterSettings.Name),
                    BindingMode.TwoWay,
                    delay: 100);
                BindingTools.SetBinding(TextMainSearch,
                    SearchBox.VisibilityProperty,
                    mainModel.AppSettings,
                    nameof(PlayniteSettings.ShowTopPanelSearchBox),
                    converter: new BooleanToVisibilityConverter());
                BindingTools.SetBinding(TextMainSearch,
                    SearchBox.IsFocusedProperty,
                    mainModel,
                    nameof(mainModel.SearchOpened),
                    BindingMode.TwoWay);
            }

            ToggleFilter = Template.FindName("PART_ToggleFilter", this) as ToggleButton;
            if (ToggleFilter != null)
            {
                BindingTools.SetBinding(ToggleFilter,
                    ToggleButton.IsCheckedProperty,
                    mainModel.AppSettings,
                    nameof(PlayniteSettings.FilterPanelVisible),
                    BindingMode.TwoWay);
                BindingTools.SetBinding(ToggleFilter,
                    ToggleButton.TagProperty,
                    mainModel.AppSettings.FilterSettings,
                    nameof(FilterSettings.IsActive));
                ToggleFilter.MouseRightButtonUp += (_, __) => mainModel.ClearFilters();
            }

            ToggleNotifications = Template.FindName("PART_ToggleNotifications", this) as ToggleButton;
            if (ToggleNotifications != null)
            {
                BindingTools.SetBinding(ToggleNotifications,
                    ToggleButton.IsCheckedProperty,
                    mainModel.AppSettings,
                    nameof(PlayniteSettings.NotificationPanelVisible),
                    BindingMode.TwoWay);
            }

            ProgressGlobal = Template.FindName("PART_ProgressGlobal", this) as ProgressBar;
            if (ProgressGlobal != null)
            {
                BindingTools.SetBinding(ProgressGlobal,
                    ProgressBar.MaximumProperty,
                    mainModel,
                    nameof(mainModel.ProgressTotal));
                BindingTools.SetBinding(ProgressGlobal,
                    ProgressBar.ValueProperty,
                    mainModel,
                    nameof(mainModel.ProgressValue));
                BindingTools.SetBinding(ProgressGlobal,
                    ProgressBar.VisibilityProperty,
                    mainModel,
                    nameof(mainModel.ProgressActive),
                    converter: new BooleanToVisibilityConverter());
            }

            TextProgressText = Template.FindName("PART_TextProgressText", this) as TextBlock;
            if (TextProgressText != null)
            {
                BindingTools.SetBinding(TextProgressText,
                    TextBlock.TextProperty,
                    mainModel,
                    nameof(mainModel.ProgressStatus));
                BindingTools.SetBinding(TextProgressText,
                    TextBlock.VisibilityProperty,
                    mainModel,
                    nameof(mainModel.ProgressActive),
                    converter: new BooleanToVisibilityConverter());
            }

            ButtonProgressCancel = Template.FindName("PART_ButtonProgressCancel", this) as ButtonBase;
            if (ButtonProgressCancel != null)
            {
                ButtonProgressCancel.Command = mainModel.CancelProgressCommand;
                BindingTools.SetBinding(ButtonProgressCancel,
                    ButtonBase.VisibilityProperty,
                    mainModel,
                    nameof(mainModel.ProgressActive),
                    converter: new BooleanToVisibilityConverter());
            }
        }
    }
}
