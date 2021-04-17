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

        private Button ButtonViewSettings;
        private Button ButtonGroupSettings;
        private Button ButtonSortSettings;
        private Button ButtonFilterPresets;

        private Button ButtonSwitchDetailsView;
        private Button ButtonSwitchGridView;
        private Button ButtonSwitchListView;

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
        }

        private void SetButtonVisibility()
        {
            ButtonViewSettings.Visibility = mainModel.AppSettings.ShowTopPanelGeneralViewItem ? Visibility.Visible : Visibility.Collapsed;
            ButtonGroupSettings.Visibility = mainModel.AppSettings.ShowTopPanelGroupingItem ? Visibility.Visible : Visibility.Collapsed;
            ButtonSortSettings.Visibility = mainModel.AppSettings.ShowTopPanelSortingItem ? Visibility.Visible : Visibility.Collapsed;
            ButtonFilterPresets.Visibility = mainModel.AppSettings.ShowTopPanelFilterPresetsItem ? Visibility.Visible : Visibility.Collapsed;

            ButtonSwitchDetailsView.Visibility = mainModel.AppSettings.ShowTopPanelDetailsViewSwitch ? Visibility.Visible : Visibility.Collapsed;
            ButtonSwitchGridView.Visibility = mainModel.AppSettings.ShowTopPanelGridViewSwitch ? Visibility.Visible : Visibility.Collapsed;
            ButtonSwitchListView.Visibility = mainModel.AppSettings.ShowTopPanelListViewSwitch ? Visibility.Visible : Visibility.Collapsed;

            var showSeparators = ButtonSwitchDetailsView.Visibility == Visibility.Visible || ButtonSwitchGridView.Visibility == Visibility.Visible || ButtonSwitchListView.Visibility == Visibility.Visible;
            LeftViewSeparator.Visibility = showSeparators ? Visibility.Visible : Visibility.Collapsed;
            RightViewSeparator.Visibility = showSeparators ? Visibility.Visible : Visibility.Collapsed;
        }

        private Button AssignPanelButton(string contentTemplate, ContextMenu menu, string tooltip)
        {
            var button = new Button();
            button.SetResourceReference(Button.ContentTemplateProperty, contentTemplate);
            button.SetResourceReference(Button.StyleProperty, "TopPanelButton");
            LeftClickContextMenuBehavior.SetEnabled(button, true);
            menu.SetResourceReference(ContextMenu.StyleProperty, "TopPanelMenu");
            button.ContextMenu = menu;
            button.ToolTip = ResourceProvider.GetString(tooltip);
            return button;
        }

        private Button AssignPanelButton(string contentTemplate, ICommand command, string tooltip)
        {
            var button = new Button();
            button.SetResourceReference(Button.ContentTemplateProperty, contentTemplate);
            button.SetResourceReference(Button.StyleProperty, "TopPanelButton");
            button.Command = command;
            button.ToolTip = tooltip;
            return button;
        }

        private Button AssignPluginButton(TopPanelItem item)
        {
            var button = new Button();
            button.SetResourceReference(Button.StyleProperty, "TopPanelButton");
            button.Content = item.Icon;
            button.Command = new RelayCommand<object>((_) => item.Action());
            button.ToolTip = item.ToolTip;
            return button;
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            PanelMainItems = Template.FindName("PART_PanelMainItems", this) as Panel;
            if (PanelMainItems != null)
            {
                PanelMainItems.Children.Add(ButtonViewSettings = AssignPanelButton("TopPanelGeneralViewSettingsTemplate", new ViewSettingsMenu(mainModel.AppSettings), LOC.TopPanelViewSettings));
                PanelMainItems.Children.Add(ButtonFilterPresets = AssignPanelButton("TopPanelFilterPresetsSelectionTemplate", new FilterPresetsMenu(mainModel), LOC.TopPanelFilterPresets));
                PanelMainItems.Children.Add(ButtonGroupSettings = AssignPanelButton("TopPanelGroupSettingsTemplate", new GroupSettingsMenu(mainModel.AppSettings), LOC.TopPanelGroupSettings));
                PanelMainItems.Children.Add(ButtonSortSettings = AssignPanelButton("TopPanelSortSettingsTemplate", new SortSettingsMenu(mainModel.AppSettings), LOC.TopPanelSortSettings));

                var separatorWidth = ResourceProvider.GetResource<double>("TopPanelSectionSeparatorWidth");
                LeftViewSeparator.Width = separatorWidth;
                RightViewSeparator.Width = separatorWidth;
                PanelMainItems.Children.Add(LeftViewSeparator);
                PanelMainItems.Children.Add(ButtonSwitchDetailsView = AssignPanelButton("TopPanelSwitchDetailsViewTemplate", mainModel.SwitchDetailsViewCommand, ViewType.Details.GetDescription()));
                PanelMainItems.Children.Add(ButtonSwitchGridView = AssignPanelButton("TopPanelSwitchGridViewTemplate", mainModel.SwitchGridViewCommand, ViewType.Grid.GetDescription()));
                PanelMainItems.Children.Add(ButtonSwitchListView = AssignPanelButton("TopPanelSwitchListViewTemplate", mainModel.SwitchListViewCommand, ViewType.List.GetDescription()));
                PanelMainItems.Children.Add(RightViewSeparator);

                var updatesButton = new Button();
                updatesButton.SetResourceReference(Button.StyleProperty, "TopPanelUpdatesButton");
                updatesButton.Command = mainModel.OpenUpdatesCommand;
                updatesButton.ToolTip = ResourceProvider.GetString(LOC.UpdateIsAvailableNotificationBody);
                BindingTools.SetBinding(updatesButton,
                    Button.VisibilityProperty,
                    mainModel,
                    nameof(mainModel.UpdatesAvailable),
                    converter: new BooleanToVisibilityConverter());
                PanelMainItems.Children.Add(updatesButton);

                SetButtonVisibility();
            }

            PanelMainPluginItems = Template.FindName("PART_PanelMainPluginItems", this) as Panel;
            if (PanelMainPluginItems != null)
            {
                foreach (object item in mainModel.Extensions.GetTopPanelPluginItems())
                {
                    if (item is TopPanelItem tpItem)
                    {
                        PanelMainPluginItems.Children.Add(AssignPluginButton(tpItem));
                    }
                    else if (item is FrameworkElement fElem)
                    {
                        PanelMainPluginItems.Children.Add(fElem);
                    }
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
                    nameof(PlayniteSettings.SidebarVisible),
                    converter: new InvertedBooleanToVisibilityConverter());
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
                    nameof(mainModel.ProgressVisible),
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
                    nameof(mainModel.ProgressVisible),
                    converter: new BooleanToVisibilityConverter());
            }

            ButtonProgressCancel = Template.FindName("PART_ButtonProgressCancel", this) as ButtonBase;
            if (ButtonProgressCancel != null)
            {
                ButtonProgressCancel.Command = mainModel.CancelProgressCommand;
                BindingTools.SetBinding(ButtonProgressCancel,
                    ButtonBase.VisibilityProperty,
                    mainModel,
                    nameof(mainModel.ProgressVisible),
                    converter: new BooleanToVisibilityConverter());
            }
        }
    }
}
