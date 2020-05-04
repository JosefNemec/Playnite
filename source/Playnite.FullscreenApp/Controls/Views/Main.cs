using Playnite.Behaviors;
using Playnite.Common;
using Playnite.Controls;
using Playnite.Converters;
using Playnite.FullscreenApp.ViewModels;
using Playnite.Input;
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
using System.Windows.Media;
using System.Xml.Linq;

namespace Playnite.FullscreenApp.Controls.Views
{
    [TemplatePart(Name = "PART_ViewHost", Type = typeof(FrameworkElement))]
    [TemplatePart(Name = "PART_MainHost", Type = typeof(FrameworkElement))]
    [TemplatePart(Name = "PART_ToggleFilterRecently", Type = typeof(ToggleButton))]
    [TemplatePart(Name = "PART_ToggleFilterFavorite", Type = typeof(ToggleButton))]
    [TemplatePart(Name = "PART_ToggleFilterMostPlayed", Type = typeof(ToggleButton))]
    [TemplatePart(Name = "PART_ToggleFilterAll", Type = typeof(ToggleButton))]
    [TemplatePart(Name = "PART_ButtonMainMenu", Type = typeof(ButtonBase))]
    [TemplatePart(Name = "PART_ButtonNotifications", Type = typeof(ButtonBase))]
    [TemplatePart(Name = "PART_TextClock", Type = typeof(TextBlock))]
    [TemplatePart(Name = "PART_TextBatteryPercentage", Type = typeof(TextBlock))]
    [TemplatePart(Name = "PART_ElemBatteryStatus", Type = typeof(FrameworkElement))]
    [TemplatePart(Name = "PART_TextProgressTooltip", Type = typeof(TextBlock))]
    [TemplatePart(Name = "PART_ElemProgressIndicator", Type = typeof(FrameworkElement))]
    [TemplatePart(Name = "PART_ElemExtraFilterActive", Type = typeof(FrameworkElement))]
    [TemplatePart(Name = "PART_ElemSearchActive", Type = typeof(FrameworkElement))]
    [TemplatePart(Name = "PART_ListGameItems", Type = typeof(ListBox))]
    [TemplatePart(Name = "PART_ButtonInstall", Type = typeof(ButtonBase))]
    [TemplatePart(Name = "PART_ButtonPlay", Type = typeof(ButtonBase))]
    [TemplatePart(Name = "PART_ButtonSearch", Type = typeof(ButtonBase))]
    [TemplatePart(Name = "PART_ButtonFilter", Type = typeof(ButtonBase))]
    [TemplatePart(Name = "PART_ButtonDetails", Type = typeof(ButtonBase))]
    [TemplatePart(Name = "PART_ButtonGameOptions", Type = typeof(ButtonBase))]
    [TemplatePart(Name = "PART_ElemNotifications", Type = typeof(FrameworkElement))]
    [TemplatePart(Name = "PART_ElemMainMenu", Type = typeof(FrameworkElement))]
    [TemplatePart(Name = "PART_ElemSettingsMenu", Type = typeof(FrameworkElement))]
    [TemplatePart(Name = "PART_ElemGameMenu", Type = typeof(FrameworkElement))]
    [TemplatePart(Name = "PART_ElemFilters", Type = typeof(FrameworkElement))]
    [TemplatePart(Name = "PART_ElemFiltersAdditional", Type = typeof(FrameworkElement))]
    [TemplatePart(Name = "PART_ContentFilterItems", Type = typeof(ContentControl))]
    [TemplatePart(Name = "PART_ElemGameDetails", Type = typeof(FrameworkElement))]
    [TemplatePart(Name = "PART_ImageBackground", Type = typeof(FadeImage))]
    public class Main : Control
    {
        private FullscreenAppViewModel mainModel;
        private FrameworkElement ViewHost;
        private FrameworkElement MainHost;
        private ToggleButton ToggleFilterRecently;
        private ToggleButton ToggleFilterFavorite;
        private ToggleButton ToggleFilterMostPlayed;
        private ToggleButton ToggleFilterAll;
        private ButtonBase ButtonMainMenu;
        private ButtonBase ButtonNotifications;
        private TextBlock TextClock;
        private TextBlock TextBatteryPercentage;
        private FrameworkElement ElemBatteryStatus;
        private TextBlock TextProgressTooltip;
        private FrameworkElement ElemProgressIndicator;
        private FrameworkElement ElemExtraFilterActive;
        private FrameworkElement ElemSearchActive;
        private ListBox ListGameItems;
        private ButtonBase ButtonInstall;
        private ButtonBase ButtonPlay;
        private ButtonBase ButtonSearch;
        private ButtonBase ButtonFilter;
        private ButtonBase ButtonDetails;
        private ButtonBase ButtonGameOptions;
        private FrameworkElement ElemNotifications;
        private FrameworkElement ElemMainMenu;
        private FrameworkElement ElemSettingsMenu;
        private FrameworkElement ElemGameMenu;
        private FrameworkElement ElemFilters;
        private FrameworkElement ElemFiltersAdditional;
        private ContentControl ContentFilterItems;
        private FrameworkElement ElemGameDetails;
        private FadeImage ImageBackground;

        static Main()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(Main), new FrameworkPropertyMetadata(typeof(Main)));
        }

        public Main() : this(FullscreenApplication.Current?.MainModel)
        {
        }

        public Main(FullscreenAppViewModel mainModel) : base()
        {
            if (DesignerProperties.GetIsInDesignMode(this))
            {
                this.mainModel = DesignMainViewModel.DesignIntance;
                DataContext = this.mainModel;
            }
            else if (mainModel != null)
            {
                this.mainModel = mainModel;
            }

            this.mainModel.AppSettings.Fullscreen.PropertyChanged += Fullscreen_PropertyChanged;
        }

        private void Fullscreen_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(FullscreenSettings.EnableMainBackgroundImage))
            {
                SetBackgroundBinding();
            }
            else if (e.PropertyName == nameof(FullscreenSettings.MainBackgroundImageBlurAmount) ||
                     e.PropertyName == nameof(FullscreenSettings.MainBackgroundImageDarkAmount))
            {
                SetBackgroundEffect();
            }
        }

        private void SetBackgroundBinding()
        {
            if (ImageBackground == null)
            {
                return;
            }

            if (mainModel.AppSettings.Fullscreen.EnableMainBackgroundImage)
            {
                BindingTools.SetBinding(ImageBackground,
                    FadeImage.SourceProperty,
                    mainModel,
                    $"{nameof(mainModel.SelectedGame)}.{nameof(GamesCollectionViewEntry.DisplayBackgroundImageObject)}");
            }
            else
            {
                ImageBackground.Source = null;
            }
        }

        private void SetBackgroundEffect()
        {
            if (ImageBackground == null)
            {
                return;
            }

            if (mainModel.AppSettings.Fullscreen.MainBackgroundImageDarkAmount > 0)
            {
                ImageBackground.ImageDarkeningBrush = null;
                ImageBackground.ImageDarkeningBrush = new SolidColorBrush(new Color()
                {
                    ScA = mainModel.AppSettings.Fullscreen.MainBackgroundImageDarkAmount / 100,
                    ScR = 0,
                    ScG = 0,
                    ScB = 0
                });
            }
            else
            {
                ImageBackground.ImageDarkeningBrush = null;
            }

            if (mainModel.AppSettings.Fullscreen.MainBackgroundImageBlurAmount > 0)
            {
                ImageBackground.IsBlurEnabled = true;
                ImageBackground.HighQualityBlur = true;
                ImageBackground.BlurAmount = mainModel.AppSettings.Fullscreen.MainBackgroundImageBlurAmount;
            }
            else
            {
                ImageBackground.IsBlurEnabled = false;
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            if (Template != null)
            {
                ViewHost = Template.FindName("PART_ViewHost", this) as FrameworkElement;
                if (ViewHost != null)
                {
                    ViewHost.InputBindings.Add(new KeyBinding() { Command = mainModel.ToggleMainMenuCommand, Key = Key.F1 });
                    ViewHost.InputBindings.Add(new KeyBinding() { Command = mainModel.PrevFilterViewCommand, Key = Key.F2 });
                    ViewHost.InputBindings.Add(new KeyBinding() { Command = mainModel.NextFilterViewCommand, Key = Key.F3 });
                    ViewHost.InputBindings.Add(new KeyBinding() { Command = mainModel.SelectRandomGameCommand, Key = Key.F6 });
                    ViewHost.InputBindings.Add(new KeyBinding() { Command = mainModel.SwitchToDesktopCommand, Key = Key.F11 });
                    ViewHost.InputBindings.Add(new KeyBinding() { Command = mainModel.OpenSearchCommand, Key = Key.Y });
                    ViewHost.InputBindings.Add(new KeyBinding() { Command = mainModel.ToggleFiltersCommand, Key = Key.F });

                    ViewHost.InputBindings.Add(new XInputBinding(mainModel.ToggleMainMenuCommand, XInputButton.Back));
                    ViewHost.InputBindings.Add(new XInputBinding(mainModel.PrevFilterViewCommand, XInputButton.LeftShoulder));
                    ViewHost.InputBindings.Add(new XInputBinding(mainModel.NextFilterViewCommand, XInputButton.RightShoulder));
                    ViewHost.InputBindings.Add(new XInputBinding(mainModel.OpenSearchCommand, XInputButton.Y));
                    ViewHost.InputBindings.Add(new XInputBinding(mainModel.ToggleFiltersCommand, XInputButton.RightStick));
                }

                MainHost = Template.FindName("PART_MainHost", this) as FrameworkElement;
                if (MainHost != null)
                {
                    BindingTools.SetBinding(MainHost, FrameworkElement.WidthProperty, mainModel, nameof(FullscreenAppViewModel.ViewportWidth));
                    BindingTools.SetBinding(MainHost, FrameworkElement.HeightProperty, mainModel, nameof(FullscreenAppViewModel.ViewportHeight));
                }

                AssignButtonWithCommand(ref ButtonMainMenu, "PART_ButtonMainMenu", mainModel.ToggleMainMenuCommand);
                AssignButtonWithCommand(ref ButtonNotifications, "PART_ButtonNotifications", mainModel.ToggleNotificationsCommand);

                ImageBackground = Template.FindName("PART_ImageBackground", this) as FadeImage;
                if (ImageBackground != null)
                {
                    SetBackgroundBinding();
                    SetBackgroundEffect();
                }

                ToggleFilterRecently = Template.FindName("PART_ToggleFilterRecently", this) as ToggleButton;
                if (ToggleFilterRecently != null)
                {
                    BindingTools.SetBinding(
                        ToggleFilterRecently,
                        ToggleButton.IsCheckedProperty,
                        mainModel.AppSettings.Fullscreen,
                        nameof(FullscreenSettings.ActiveView),
                        BindingMode.TwoWay,
                        converter: new EnumToBooleanConverter(),
                        converterParameter: ActiveFullscreenView.RecentlyPlayed);
                }

                ToggleFilterFavorite = Template.FindName("PART_ToggleFilterFavorite", this) as ToggleButton;
                if (ToggleFilterFavorite != null)
                {
                    BindingTools.SetBinding(
                        ToggleFilterFavorite,
                        ToggleButton.IsCheckedProperty,
                        mainModel.AppSettings.Fullscreen,
                        nameof(FullscreenSettings.ActiveView),
                        BindingMode.TwoWay,
                        converter: new EnumToBooleanConverter(),
                        converterParameter: ActiveFullscreenView.Favorites);
                }

                ToggleFilterMostPlayed = Template.FindName("PART_ToggleFilterMostPlayed", this) as ToggleButton;
                if (ToggleFilterMostPlayed != null)
                {
                    BindingTools.SetBinding(
                        ToggleFilterMostPlayed,
                        ToggleButton.IsCheckedProperty,
                        mainModel.AppSettings.Fullscreen,
                        nameof(FullscreenSettings.ActiveView),
                        BindingMode.TwoWay,
                        converter: new EnumToBooleanConverter(),
                        converterParameter: ActiveFullscreenView.MostPlayed);
                }

                ToggleFilterAll = Template.FindName("PART_ToggleFilterAll", this) as ToggleButton;
                if (ToggleFilterAll != null)
                {
                    BindingTools.SetBinding(
                        ToggleFilterAll,
                        ToggleButton.IsCheckedProperty,
                        mainModel.AppSettings.Fullscreen,
                        nameof(FullscreenSettings.ActiveView),
                        BindingMode.TwoWay,
                        converter: new EnumToBooleanConverter(),
                        converterParameter: ActiveFullscreenView.All);
                }

                TextClock = Template.FindName("PART_TextClock", this) as TextBlock;
                if (TextClock != null)
                {
                    BindingTools.SetBinding(TextClock, TextBlock.TextProperty, mainModel.CurrentTime, nameof(ObservableTime.Time));
                    BindingTools.SetBinding(
                        TextClock,
                        TextBlock.VisibilityProperty,
                        mainModel.AppSettings.Fullscreen,
                        nameof(FullscreenSettings.ShowClock),
                        converter: new Converters.BooleanToVisibilityConverter());
                }

                TextBatteryPercentage = Template.FindName("PART_TextBatteryPercentage", this) as TextBlock;
                if (TextBatteryPercentage != null)
                {
                    BindingTools.SetBinding(TextBatteryPercentage,
                        TextBlock.TextProperty,
                        mainModel.PowerStatus,
                        nameof(ObservablePowerStatus.PercentCharge),
                        stringFormat: "{0}%");
                    BindingTools.SetBinding(TextBatteryPercentage,
                        TextBlock.VisibilityProperty,
                        mainModel.AppSettings.Fullscreen,
                        nameof(FullscreenSettings.ShowBatteryPercentage),
                        converter: new Converters.BooleanToVisibilityConverter());
                }

                ElemBatteryStatus = Template.FindName("PART_ElemBatteryStatus", this) as FrameworkElement;
                if (ElemBatteryStatus != null)
                {
                    BindingTools.SetBinding(
                        ElemBatteryStatus,
                        TextBlock.VisibilityProperty,
                        mainModel.AppSettings.Fullscreen,
                        nameof(FullscreenSettings.ShowBattery),
                        converter: new Converters.BooleanToVisibilityConverter());
                }

                TextProgressTooltip = Template.FindName("PART_TextProgressTooltip", this) as TextBlock;
                if (TextProgressTooltip != null)
                {
                    BindingTools.SetBinding(TextProgressTooltip, TextBlock.TextProperty, mainModel, nameof(FullscreenAppViewModel.ProgressStatus));
                    BindingTools.SetBinding(TextProgressTooltip,
                        TextBlock.VisibilityProperty,
                        mainModel,
                        nameof(FullscreenAppViewModel.ProgressVisible),
                        converter: new Converters.BooleanToVisibilityConverter());
                }

                ElemProgressIndicator = Template.FindName("PART_ElemProgressIndicator", this) as FrameworkElement;
                if (ElemProgressIndicator != null)
                {
                    BindingTools.SetBinding(ElemProgressIndicator,
                        ToggleButton.VisibilityProperty,
                        mainModel,
                        nameof(FullscreenAppViewModel.ProgressVisible),
                        converter: new Converters.BooleanToVisibilityConverter());
                }

                ElemExtraFilterActive = Template.FindName("PART_ElemExtraFilterActive", this) as FrameworkElement;
                if (ElemExtraFilterActive != null)
                {
                    BindingTools.SetBinding(ElemExtraFilterActive,
                        FrameworkElement.VisibilityProperty,
                        mainModel,
                        nameof(FullscreenAppViewModel.IsExtraFilterActive),
                        converter: new Converters.BooleanToVisibilityConverter());
                }

                ElemSearchActive = Template.FindName("PART_ElemSearchActive", this) as FrameworkElement;
                if (ElemSearchActive != null)
                {
                    BindingTools.SetBinding(ElemSearchActive,
                        FrameworkElement.VisibilityProperty,
                        mainModel,
                        nameof(FullscreenAppViewModel.IsSearchActive),
                        converter: new Converters.BooleanToVisibilityConverter());
                }

                ListGameItems = Template.FindName("PART_ListGameItems", this) as ListBox;
                if (ListGameItems != null)
                {
                    ListGameItems.ItemsPanel = GetItemsPanelTemplate();
                    ListGameItems.InputBindings.Add(new KeyBinding() { Command = mainModel.ToggleGameOptionsCommand, Key = Key.X });
                    ListGameItems.InputBindings.Add(new KeyBinding() { Command = mainModel.ToggleGameDetailsCommand, Key = Key.A });
                    ListGameItems.InputBindings.Add(new KeyBinding() { Command = mainModel.ActivateSelectedCommand, Key = Key.Enter });

                    ListGameItems.InputBindings.Add(new XInputBinding(mainModel.ToggleGameOptionsCommand, XInputButton.Start));
                    ListGameItems.InputBindings.Add(new XInputBinding(mainModel.ToggleGameDetailsCommand, XInputButton.A));
                    ListGameItems.InputBindings.Add(new XInputBinding(mainModel.ActivateSelectedCommand, XInputButton.X));

                    BindingTools.SetBinding(ListGameItems,
                        ListBox.VisibilityProperty,
                        mainModel,
                        nameof(FullscreenAppViewModel.GameListVisible),
                        converter: new Converters.BooleanToVisibilityConverter());
                    BindingTools.SetBinding(ListGameItems,
                        ListBox.SelectedItemProperty,
                        mainModel,
                        nameof(FullscreenAppViewModel.SelectedGame),
                        BindingMode.TwoWay);
                    BindingTools.SetBinding(ListGameItems,
                        ListBox.ItemsSourceProperty,
                        mainModel,
                        $"{nameof(FullscreenAppViewModel.GamesView)}.{nameof(FullscreenCollectionView.CollectionView)}");
                    BindingTools.SetBinding(ListGameItems,
                        FocusBahaviors.FocusBindingProperty,
                        mainModel,
                        nameof(mainModel.GameListFocused));
                }

                AssignButtonWithCommand(ref ButtonInstall, "PART_ButtonInstall", mainModel.ActivateSelectedCommand);
                BindingTools.SetBinding(
                    ButtonInstall,
                    ButtonBase.VisibilityProperty,
                    mainModel,
                    $"{nameof(FullscreenAppViewModel.SelectedGame)}.{nameof(GamesCollectionViewEntry.IsInstalled)}",
                    converter: new InvertedBooleanToVisibilityConverter(),
                    fallBackValue: Visibility.Collapsed);

                AssignButtonWithCommand(ref ButtonPlay, "PART_ButtonPlay", mainModel.ActivateSelectedCommand);
                BindingTools.SetBinding(
                    ButtonPlay,
                    ButtonBase.VisibilityProperty,
                    mainModel,
                    $"{nameof(FullscreenAppViewModel.SelectedGame)}.{nameof(GamesCollectionViewEntry.IsInstalled)}",
                    converter: new Converters.BooleanToVisibilityConverter(),
                    fallBackValue: Visibility.Collapsed);

                AssignButtonWithCommand(ref ButtonDetails, "PART_ButtonDetails", mainModel.ToggleGameDetailsCommand);
                BindingTools.SetBinding(
                    ButtonDetails,
                    ButtonBase.VisibilityProperty,
                    mainModel,
                    nameof(FullscreenAppViewModel.GameDetailsButtonVisible),
                    converter: new Converters.BooleanToVisibilityConverter());

                AssignButtonWithCommand(ref ButtonGameOptions, "PART_ButtonGameOptions", mainModel.ToggleGameOptionsCommand);
                BindingTools.SetBinding(
                    ButtonGameOptions,
                    ButtonBase.VisibilityProperty,
                    mainModel,
                    nameof(FullscreenAppViewModel.GameDetailsButtonVisible),
                    converter: new Converters.BooleanToVisibilityConverter());

                AssignButtonWithCommand(ref ButtonSearch, "PART_ButtonSearch", mainModel.OpenSearchCommand);
                AssignButtonWithCommand(ref ButtonFilter, "PART_ButtonFilter", mainModel.ToggleFiltersCommand);

                ElemNotifications = Template.FindName("PART_ElemNotifications", this) as FrameworkElement;
                if (ElemNotifications != null)
                {
                    BindingTools.SetBinding(ElemNotifications,
                        FrameworkElement.VisibilityProperty,
                        mainModel,
                        nameof(FullscreenAppViewModel.NotificationsVisible),
                        converter: new Converters.BooleanToVisibilityConverter());
                }

                ElemMainMenu = Template.FindName("PART_ElemMainMenu", this) as FrameworkElement;
                if (ElemMainMenu != null)
                {
                    BindingTools.SetBinding(ElemMainMenu,
                        FrameworkElement.VisibilityProperty,
                        mainModel,
                        nameof(FullscreenAppViewModel.MainMenuVisible),
                        converter: new Converters.BooleanToVisibilityConverter());
                }

                ElemSettingsMenu = Template.FindName("PART_ElemSettingsMenu", this) as FrameworkElement;
                if (ElemSettingsMenu != null)
                {
                    BindingTools.SetBinding(ElemSettingsMenu,
                        FrameworkElement.VisibilityProperty,
                        mainModel,
                        nameof(FullscreenAppViewModel.SettingsMenuVisible),
                        converter: new Converters.BooleanToVisibilityConverter());
                }

                ElemGameMenu = Template.FindName("PART_ElemGameMenu", this) as FrameworkElement;
                if (ElemGameMenu != null)
                {
                    BindingTools.SetBinding(ElemGameMenu,
                        FrameworkElement.VisibilityProperty,
                        mainModel,
                        nameof(FullscreenAppViewModel.GameMenuVisible),
                        converter: new Converters.BooleanToVisibilityConverter());
                }

                ElemFilters = Template.FindName("PART_ElemFilters", this) as FrameworkElement;
                if (ElemFilters != null)
                {
                    BindingTools.SetBinding(ElemFilters,
                        FrameworkElement.VisibilityProperty,
                        mainModel,
                        nameof(FullscreenAppViewModel.FilterPanelVisible),
                        converter: new Converters.BooleanToVisibilityConverter());
                }

                ElemFiltersAdditional = Template.FindName("PART_ElemFiltersAdditional", this) as FrameworkElement;
                if (ElemFiltersAdditional != null)
                {
                    BindingTools.SetBinding(ElemFiltersAdditional,
                        FrameworkElement.VisibilityProperty,
                        mainModel,
                        nameof(FullscreenAppViewModel.FilterAdditionalPanelVisible),
                        converter: new Converters.BooleanToVisibilityConverter());
                }

                ContentFilterItems = Template.FindName("PART_ContentFilterItems", this) as ContentControl;
                if (ContentFilterItems != null)
                {
                    BindingTools.SetBinding(ContentFilterItems,
                        ContentControl.VisibilityProperty,
                        mainModel,
                        nameof(FullscreenAppViewModel.SubFilterVisible),
                        converter: new Converters.BooleanToVisibilityConverter());
                    BindingTools.SetBinding(ContentFilterItems,
                        ContentControl.ContentProperty,
                        mainModel,
                        nameof(FullscreenAppViewModel.SubFilterControl));
                }

                ElemGameDetails = Template.FindName("PART_ElemGameDetails", this) as FrameworkElement;
                if (ElemGameDetails != null)
                {
                    BindingTools.SetBinding(ElemGameDetails,
                        FrameworkElement.VisibilityProperty,
                        mainModel,
                        nameof(FullscreenAppViewModel.GameDetailsVisible),
                        converter: new Converters.BooleanToVisibilityConverter());
                    BindingTools.SetBinding(ElemGameDetails,
                        FrameworkElement.DataContextProperty,
                        mainModel,
                        nameof(FullscreenAppViewModel.GameDetailsEntry));
                }
            }
        }

        private void AssignButtonWithCommand(ref ButtonBase button, string partId, ICommand command)
        {
            button = Template.FindName(partId, this) as ButtonBase;
            if (button != null)
            {
                button.Command = command;
            }
        }

        private ItemsPanelTemplate GetItemsPanelTemplate()
        {
            XNamespace pns = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
            var templateDoc = new XDocument(
                new XElement(pns + nameof(ItemsPanelTemplate),
                    new XElement(pns + nameof(FullscreenTilePanel),
                        new XAttribute("Rows", "{Settings Fullscreen.Rows}"),
                        new XAttribute("Columns", "{Settings Fullscreen.Columns}"),
                        new XAttribute("UseHorizontalLayout", "{Settings Fullscreen.HorizontalLayout}"),
                        new XAttribute("ItemAspectRatio", "{Settings CoverAspectRatio}"),
                        new XAttribute("ItemSpacing", "{Settings FullscreenItemSpacing}"))));

            var str = templateDoc.ToString();
            return Xaml.FromString<ItemsPanelTemplate>(str);
        }
    }
}
