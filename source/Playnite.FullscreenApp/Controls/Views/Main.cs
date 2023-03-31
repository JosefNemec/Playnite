using Playnite.Behaviors;
using Playnite.Common;
using Playnite.Controls;
using Playnite.Converters;
using Playnite.FullscreenApp.ViewModels;
using Playnite.Input;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
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
    [TemplatePart(Name = "PART_ButtonInstall", Type = typeof(ButtonEx))]
    [TemplatePart(Name = "PART_ButtonPlay", Type = typeof(ButtonEx))]
    [TemplatePart(Name = "PART_ButtonSearch", Type = typeof(ButtonEx))]
    [TemplatePart(Name = "PART_ButtonFilter", Type = typeof(ButtonEx))]
    [TemplatePart(Name = "PART_ButtonDetails", Type = typeof(ButtonEx))]
    [TemplatePart(Name = "PART_ButtonGameOptions", Type = typeof(ButtonEx))]
    [TemplatePart(Name = "PART_ElemFilters", Type = typeof(FrameworkElement))]
    [TemplatePart(Name = "PART_ElemFiltersAdditional", Type = typeof(FrameworkElement))]
    [TemplatePart(Name = "PART_ContentFilterItems", Type = typeof(ContentControl))]
    [TemplatePart(Name = "PART_ElemGameDetails", Type = typeof(FrameworkElement))]
    [TemplatePart(Name = "PART_ImageBackground", Type = typeof(FadeImage))]
    [TemplatePart(Name = "PART_FilterPresetSelector", Type = typeof(FilterPresetSelector))]
    [TemplatePart(Name = "PART_ElemGameStatus", Type = typeof(FrameworkElement))]
    [TemplatePart(Name = "PART_ButtonProgramUpdate", Type = typeof(ButtonBase))]
    public class Main : Control
    {
        private FullscreenAppViewModel mainModel;
        private FrameworkElement ViewHost;
        private FrameworkElement MainHost;
        private ButtonBase ButtonMainMenu;
        private ButtonBase ButtonNotifications;
        private ButtonBase ButtonProgramUpdate;
        private TextBlock TextClock;
        private TextBlock TextBatteryPercentage;
        private FrameworkElement ElemBatteryStatus;
        private TextBlock TextProgressTooltip;
        private FrameworkElement ElemProgressIndicator;
        private FrameworkElement ElemExtraFilterActive;
        private FrameworkElement ElemSearchActive;
        private ListBox ListGameItems;
        private ButtonEx ButtonInstall;
        private ButtonEx ButtonPlay;
        private ButtonEx ButtonSearch;
        private ButtonEx ButtonFilter;
        private ButtonEx ButtonDetails;
        private ButtonEx ButtonGameOptions;
        private FrameworkElement ElemFilters;
        private FrameworkElement ElemFiltersAdditional;
        private ContentControl ContentFilterItems;
        private FrameworkElement ElemGameDetails;
        private FadeImage ImageBackground;
        private FrameworkElement ElemGameStatus;

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
            this.mainModel.PropertyChanged += MainModel_PropertyChanged;
        }

        private void MainModel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(FullscreenAppViewModel.GameDetailsVisible))
            {
                SetDetailsElemBindings();
            }

            if (e.PropertyName == nameof(FullscreenAppViewModel.ActiveFilterPreset))
            {
                var panel = ElementTreeHelper.FindVisualChildren<FullscreenTilePanel>(ListGameItems).FirstOrDefault();
                if (panel == null)
                {
                    return;
                }

                if (panel.UseHorizontalLayout)
                {
                    panel.SetHorizontalOffset(0);
                }
                else
                {
                    panel.SetVerticalOffset(0);
                }
            }
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
            else if (e.PropertyName == nameof(FullscreenSettings.SwapStartDetailsAction))
            {
                SetListCommandBindings();
            }
        }

        private void SetListCommandBindings()
        {
            if (ListGameItems == null)
            {
                return;
            }

            var swapStartInput = mainModel.AppSettings.Fullscreen.SwapStartDetailsAction;
            ListGameItems.InputBindings.Clear();
            ListGameItems.InputBindings.Add(new KeyBinding() { Command = mainModel.OpenGameMenuCommand, Key = swapStartInput ? Key.A : Key.X });
            ListGameItems.InputBindings.Add(new KeyBinding() { Command = mainModel.ToggleGameDetailsCommand, Key = swapStartInput ? Key.X : Key.A });
            ListGameItems.InputBindings.Add(new KeyBinding() { Command = mainModel.ActivateSelectedCommand, Key = Key.Enter });
            ListGameItems.InputBindings.Add(new XInputBinding(mainModel.OpenGameMenuCommand, XInputButton.Start));
            ListGameItems.InputBindings.Add(new XInputBinding(mainModel.ToggleGameDetailsCommand, swapStartInput ? XInputButton.X : XInputButton.A));
            ListGameItems.InputBindings.Add(new XInputBinding(mainModel.ActivateSelectedCommand, swapStartInput ? XInputButton.A : XInputButton.X));

            ButtonPlay?.SetResourceReference(ButtonEx.InputHintProperty, swapStartInput ? "ButtonPromptA" : "ButtonPromptX");
            ButtonInstall?.SetResourceReference(ButtonEx.InputHintProperty, swapStartInput ? "ButtonPromptA" : "ButtonPromptX");
            ButtonDetails?.SetResourceReference(ButtonEx.InputHintProperty, swapStartInput ? "ButtonPromptX" : "ButtonPromptA");
        }

        private void SetBackgroundBinding()
        {
            if (ImageBackground == null)
            {
                return;
            }

            if (mainModel.AppSettings.Fullscreen.EnableMainBackgroundImage)
            {
                ImageBackground.SourceUpdateDelay = 300;
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
                    ViewHost.InputBindings.Add(new KeyBinding() { Command = mainModel.OpenMainMenuCommand, Key = Key.F1 });
                    ViewHost.InputBindings.Add(new KeyBinding() { Command = mainModel.PrevFilterViewCommand, Key = Key.F2 });
                    ViewHost.InputBindings.Add(new KeyBinding() { Command = mainModel.NextFilterViewCommand, Key = Key.F3 });
                    ViewHost.InputBindings.Add(new KeyBinding() { Command = mainModel.OpenSearchCommand, Key = Key.Y });
                    ViewHost.InputBindings.Add(new KeyBinding() { Command = mainModel.ToggleFiltersCommand, Key = Key.F });
                    ViewHost.InputBindings.Add(new KeyBinding() { Command = mainModel.SwitchToDesktopCommand, Key = Key.F11 });
                    ViewHost.InputBindings.Add(new KeyBinding() { Command = mainModel.SelectFilterPresetCommand, Key = Key.R });

                    ViewHost.InputBindings.Add(new XInputBinding(mainModel.PrevFilterViewCommand, XInputButton.LeftShoulder));
                    ViewHost.InputBindings.Add(new XInputBinding(mainModel.NextFilterViewCommand, XInputButton.RightShoulder));
                    ViewHost.InputBindings.Add(new XInputBinding(mainModel.OpenSearchCommand, XInputButton.Y));
                    ViewHost.InputBindings.Add(new XInputBinding(mainModel.ToggleFiltersCommand, XInputButton.RightStick));
                    ViewHost.InputBindings.Add(new XInputBinding(mainModel.OpenMainMenuCommand, XInputButton.Back));
                    ViewHost.InputBindings.Add(new XInputBinding(mainModel.SelectFilterPresetCommand, XInputButton.LeftStick));
                }

                MainHost = Template.FindName("PART_MainHost", this) as FrameworkElement;
                if (MainHost != null)
                {
                    BindingTools.SetBinding(MainHost, FrameworkElement.WidthProperty, mainModel, nameof(FullscreenAppViewModel.ViewportWidth));
                    BindingTools.SetBinding(MainHost, FrameworkElement.HeightProperty, mainModel, nameof(FullscreenAppViewModel.ViewportHeight));
                }

                AssignButtonWithCommand(ref ButtonProgramUpdate, "PART_ButtonProgramUpdate", mainModel.OpenUpdatesCommand);
                AssignButtonWithCommand(ref ButtonMainMenu, "PART_ButtonMainMenu", mainModel.OpenMainMenuCommand);
                AssignButtonWithCommand(ref ButtonNotifications, "PART_ButtonNotifications", mainModel.OpenNotificationsMenuCommand);
                if (ButtonProgramUpdate != null) AutomationProperties.SetName(ButtonProgramUpdate, LOC.UpdateIsAvailableNotificationBody.GetLocalized());
                if (ButtonMainMenu != null) AutomationProperties.SetName(ButtonMainMenu, LOC.ApplicationMenu.GetLocalized());
                if (ButtonNotifications != null) AutomationProperties.SetName(ButtonNotifications, LOC.Notifications.GetLocalized());

                if (ButtonProgramUpdate != null)
                {
                    BindingTools.SetBinding(ButtonProgramUpdate,
                         Button.VisibilityProperty,
                         mainModel,
                         nameof(mainModel.UpdatesAvailable),
                         converter: new Converters.BooleanToVisibilityConverter());
                }

                ImageBackground = Template.FindName("PART_ImageBackground", this) as FadeImage;
                if (ImageBackground != null)
                {
                    ImageBackground.SourceUpdateDelay = 300;
                    SetBackgroundBinding();
                    SetBackgroundEffect();
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
                        nameof(FullscreenAppViewModel.ProgressActive),
                        converter: new Converters.BooleanToVisibilityConverter());
                }

                ElemProgressIndicator = Template.FindName("PART_ElemProgressIndicator", this) as FrameworkElement;
                if (ElemProgressIndicator != null)
                {
                    BindingTools.SetBinding(ElemProgressIndicator,
                        ToggleButton.VisibilityProperty,
                        mainModel,
                        nameof(FullscreenAppViewModel.ProgressActive),
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
                    XNamespace pns = "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
                    ListGameItems.ItemsPanel = Xaml.FromString<ItemsPanelTemplate>(new XDocument(
                        new XElement(pns + nameof(ItemsPanelTemplate),
                            new XElement(pns + nameof(FullscreenTilePanel),
                                new XAttribute(nameof(FullscreenTilePanel.Rows), "{Settings Fullscreen.Rows}"),
                                new XAttribute(nameof(FullscreenTilePanel.Columns), "{Settings Fullscreen.Columns}"),
                                new XAttribute(nameof(FullscreenTilePanel.UseHorizontalLayout), "{Settings Fullscreen.HorizontalLayout}"),
                                new XAttribute(nameof(FullscreenTilePanel.ItemAspectRatio), "{Settings CoverAspectRatio}"),
                                new XAttribute(nameof(FullscreenTilePanel.ItemSpacing), "{Settings FullscreenItemSpacing}"),
                                new XAttribute(nameof(FullscreenTilePanel.SmoothScrollEnabled), "{Settings Fullscreen.SmoothScrolling}")))
                    ).ToString());

                    ListGameItems.ItemTemplate = Xaml.FromString<DataTemplate>(new XDocument(
                        new XElement(pns + nameof(DataTemplate),
                            new XElement(pns + nameof(GameListItem),
                                new XAttribute(nameof(GameListItem.Style), "{StaticResource ListGameItemTemplate}")))
                    ).ToString());

                    ListGameItems.SetResourceReference(ListBoxEx.ItemContainerStyleProperty, "ListGameItemStyle");

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
                if (ButtonInstall != null)
                {
                    BindingTools.SetBinding(
                        ButtonInstall,
                        ButtonBase.VisibilityProperty,
                        mainModel,
                        $"{nameof(FullscreenAppViewModel.SelectedGame)}.{nameof(GamesCollectionViewEntry.IsInstalled)}",
                        converter: new InvertedBooleanToVisibilityConverter(),
                        fallBackValue: Visibility.Collapsed);
                }

                AssignButtonWithCommand(ref ButtonPlay, "PART_ButtonPlay", mainModel.ActivateSelectedCommand);
                if (ButtonPlay != null)
                {
                    ButtonPlay.Command = mainModel.ActivateSelectedCommand;
                    BindingTools.SetBinding(
                        ButtonPlay,
                        ButtonBase.VisibilityProperty,
                        mainModel,
                        $"{nameof(FullscreenAppViewModel.SelectedGame)}.{nameof(GamesCollectionViewEntry.IsInstalled)}",
                        converter: new Converters.BooleanToVisibilityConverter(),
                        fallBackValue: Visibility.Collapsed);
                }

                AssignButtonWithCommand(ref ButtonDetails, "PART_ButtonDetails", mainModel.ToggleGameDetailsCommand);
                if (ButtonDetails != null)
                {
                    BindingTools.SetBinding(
                        ButtonDetails,
                        ButtonBase.VisibilityProperty,
                        mainModel,
                        nameof(FullscreenAppViewModel.GameDetailsButtonVisible),
                        converter: new Converters.BooleanToVisibilityConverter());
                }

                AssignButtonWithCommand(ref ButtonGameOptions, "PART_ButtonGameOptions", mainModel.OpenGameMenuCommand);
                if (ButtonGameOptions != null)
                {
                    BindingTools.SetBinding(
                        ButtonGameOptions,
                        ButtonBase.VisibilityProperty,
                        mainModel,
                        nameof(FullscreenAppViewModel.GameDetailsButtonVisible),
                        converter: new Converters.BooleanToVisibilityConverter());
                    ButtonGameOptions.SetResourceReference(ButtonEx.InputHintProperty, "ButtonPromptStart");
                }

                AssignButtonWithCommand(ref ButtonSearch, "PART_ButtonSearch", mainModel.OpenSearchCommand);
                ButtonSearch?.SetResourceReference(ButtonEx.InputHintProperty, "ButtonPromptY");
                AssignButtonWithCommand(ref ButtonFilter, "PART_ButtonFilter", mainModel.ToggleFiltersCommand);
                ButtonFilter?.SetResourceReference(ButtonEx.InputHintProperty, "ButtonPromptRS");

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
                    SetDetailsElemBindings();
                }

                ElemGameStatus = Template.FindName("PART_ElemGameStatus", this) as FrameworkElement;
                if (ElemGameStatus != null)
                {
                    BindingTools.SetBinding(ElemGameStatus,
                        FrameworkElement.VisibilityProperty,
                        mainModel,
                        nameof(FullscreenAppViewModel.GameStatusVisible),
                        converter: new Converters.BooleanToVisibilityConverter());
                    BindingTools.SetBinding(ElemGameStatus,
                        FrameworkElement.DataContextProperty,
                        mainModel,
                        nameof(FullscreenAppViewModel.GameStatusView));
                }

                SetListCommandBindings();

                ControlTemplateTools.InitializePluginControls(
                    mainModel.Extensions,
                    Template,
                    this,
                    ApplicationMode.Fullscreen,
                    mainModel,
                    $"{nameof(FullscreenAppViewModel.SelectedGameDetails)}.{nameof(GameDetailsViewModel.Game)}.{nameof(GameDetailsViewModel.Game.Game)}");
            }
        }

        private void SetDetailsElemBindings()
        {
            if (ElemGameDetails == null)
            {
                return;
            }

            if (mainModel.GameDetailsVisible)
            {
                BindingTools.SetBinding(ElemGameDetails,
                    FrameworkElement.DataContextProperty,
                    mainModel,
                    nameof(FullscreenAppViewModel.SelectedGame));
            }
            else
            {
                ElemGameDetails.DataContext = null;
            }
        }

        private void AssignButtonWithCommand(ref ButtonEx button, string partId, ICommand command)
        {
            button = Template.FindName(partId, this) as ButtonEx;
            if (button != null)
            {
                button.Command = command;
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
    }
}
