using Playnite.Behaviors;
using Playnite.Common;
using Playnite.Converters;
using Playnite.DesktopApp.ViewModels;
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
using BooleanToVisibilityConverter = System.Windows.Controls.BooleanToVisibilityConverter;

namespace Playnite.DesktopApp.Controls.Views
{
    [TemplatePart(Name = "PART_ElemMainMenu", Type = typeof(FrameworkElement))]
    [TemplatePart(Name = "PART_ElemViewMenu", Type = typeof(FrameworkElement))]
    [TemplatePart(Name = "PART_TextMainSearch", Type = typeof(SearchBox))]
    [TemplatePart(Name = "PART_ToggleFilter", Type = typeof(ToggleButton))]
    [TemplatePart(Name = "PART_ToggleNotifications", Type = typeof(ToggleButton))]
    [TemplatePart(Name = "PART_ButtonSteamFriends", Type = typeof(ButtonBase))]
    [TemplatePart(Name = "PART_ProgressGlobal", Type = typeof(ProgressBar))]
    [TemplatePart(Name = "PART_TextProgressText", Type = typeof(TextBlock))]
    [TemplatePart(Name = "PART_ButtonProgressCancel", Type = typeof(ButtonBase))]
    public class MainPanel : Control
    {
        private readonly DesktopAppViewModel mainModel;
        private FrameworkElement ElemMainMenu;
        private FrameworkElement ElemViewMenu;
        private SearchBox TextMainSearch;
        private ToggleButton ToggleFilter;
        private ToggleButton ToggleNotifications;
        private ButtonBase ButtonSteamFriends;
        private ProgressBar ProgressGlobal;
        private TextBlock TextProgressText;
        private ButtonBase ButtonProgressCancel;

        static MainPanel()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MainPanel), new FrameworkPropertyMetadata(typeof(MainPanel)));
        }

        public MainPanel() : this(DesktopApplication.Current?.MainModel)
        {
        }

        public MainPanel(DesktopAppViewModel mainModel)
        {
            if (DesignerProperties.GetIsInDesignMode(this))
            {
                this.mainModel = DesignMainViewModel.DesignIntance;
            }
            else if (mainModel != null)
            {
                this.mainModel = mainModel;
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            ElemMainMenu = Template.FindName("PART_ElemMainMenu", this) as FrameworkElement;
            if (ElemMainMenu != null)
            {
                LeftClickContextMenuBehavior.SetEnabled(ElemMainMenu, true);
                ElemMainMenu.ContextMenu = new MainMenu(mainModel)
                {
                    StaysOpen = false,
                    Placement = PlacementMode.Bottom
                };

                BindingTools.SetBinding(ElemMainMenu,
                    FrameworkElement.VisibilityProperty,
                    mainModel.AppSettings,
                    nameof(PlayniteSettings.SidebarVisible),
                    converter: new InvertedBooleanToVisibilityConverter());
            }

            ElemViewMenu = Template.FindName("PART_ElemViewMenu", this) as FrameworkElement;
            if (ElemViewMenu != null)
            {
                LeftClickContextMenuBehavior.SetEnabled(ElemViewMenu, true);
                ElemViewMenu.ContextMenu = new ViewSettingsMenu(mainModel.AppSettings)
                {
                    StaysOpen = false,
                    Placement = PlacementMode.Bottom
                };
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

            ButtonSteamFriends = Template.FindName("PART_ButtonSteamFriends", this) as ButtonBase;
            if (ButtonSteamFriends != null)
            {
                ButtonSteamFriends.Command = mainModel.OpenSteamFriendsCommand;
                BindingTools.SetBinding(ButtonSteamFriends,
                    ButtonBase.VisibilityProperty,
                    mainModel.AppSettings,
                    nameof(PlayniteSettings.ShowSteamFriendsButton),
                    converter: new BooleanToVisibilityConverter());
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
