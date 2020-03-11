using Playnite.Behaviors;
using Playnite.Common;
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

namespace Playnite.FullscreenApp.Controls.Views
{
    [TemplatePart(Name = "PART_MenuHost", Type = typeof(FrameworkElement))]
    [TemplatePart(Name = "PART_SelectMonitor", Type = typeof(Selector))]
    [TemplatePart(Name = "PART_SelectTheme", Type = typeof(Selector))]
    [TemplatePart(Name = "PART_ToggleGameTitles", Type = typeof(ToggleButton))]
    [TemplatePart(Name = "PART_ToggleClock", Type = typeof(ToggleButton))]
    [TemplatePart(Name = "PART_ToggleBattery", Type = typeof(ToggleButton))]
    [TemplatePart(Name = "PART_ToggleBatteryPercentage", Type = typeof(ToggleButton))]
    [TemplatePart(Name = "PART_ToggleInstalledInQFilter", Type = typeof(ToggleButton))]
    [TemplatePart(Name = "PART_ToggleHorizontalLayout", Type = typeof(ToggleButton))]
    [TemplatePart(Name = "PART_ToggleDarkenUninstalled", Type = typeof(ToggleButton))]
    [TemplatePart(Name = "PART_SliderColumns", Type = typeof(Slider))]
    [TemplatePart(Name = "PART_SliderRows", Type = typeof(Slider))]
    [TemplatePart(Name = "PART_SliderItemSpacing", Type = typeof(Slider))]
    [TemplatePart(Name = "PART_SelectButtonPrompts", Type = typeof(Selector))]
    [TemplatePart(Name = "PART_ToggleBackgroundOnMain", Type = typeof(ToggleButton))]
    [TemplatePart(Name = "PART_SliderBackgroundDarkenAmount", Type = typeof(Slider))]
    [TemplatePart(Name = "PART_SliderBackgroundBlurAmount", Type = typeof(Slider))]
    public class SettingsMenu : Control
    {
        private FullscreenAppViewModel mainModel;
        private FrameworkElement MenuHost;
        private Selector SelectMonitor;
        private Selector SelectTeme;
        private Selector SelectButtonPrompts;
        private ToggleButton ToggleGameTitles;
        private ToggleButton ToggleClock;
        private ToggleButton ToggleBattery;
        private ToggleButton ToggleBatteryPercentage;
        private ToggleButton ToggleInstalledInQFilter;
        private ToggleButton ToggleHorizontalLayout;
        private ToggleButton ToggleDarkenUninstalled;
        private Slider SliderColumns;
        private Slider SliderRows;
        private Slider SliderItemSpacing;
        private ToggleButton ToggleBackgroundOnMain;
        private Slider SliderBackgroundDarkenAmount;
        private Slider SliderBackgroundBlurAmount;

        static SettingsMenu()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SettingsMenu), new FrameworkPropertyMetadata(typeof(SettingsMenu)));
        }

        public SettingsMenu() : this(FullscreenApplication.Current?.MainModel)
        {
        }

        public SettingsMenu(FullscreenAppViewModel mainModel) : base()
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

            if (Template != null)
            {
                MenuHost = Template.FindName("PART_MenuHost", this) as FrameworkElement;
                if (MenuHost != null)
                {
                    MenuHost.InputBindings.Add(new KeyBinding(mainModel.ToggleSettingsMenuCommand, new KeyGesture(Key.Back)));
                    MenuHost.InputBindings.Add(new KeyBinding(mainModel.ToggleSettingsMenuCommand, new KeyGesture(Key.Escape)));
                    MenuHost.InputBindings.Add(new XInputBinding(mainModel.ToggleSettingsMenuCommand, XInputButton.B));
                    BindingTools.SetBinding(MenuHost,
                         FocusBahaviors.FocusBindingProperty,
                         mainModel,
                         nameof(mainModel.SettingsMenuVisible));
                }

                SelectMonitor = Template.FindName("PART_SelectMonitor", this) as Selector;
                if (SelectMonitor != null)
                {
                    SelectMonitor.ItemsSource = mainModel.AppSettings.Fullscreen.AvailableScreens;
                    SelectMonitor.DisplayMemberPath = nameof(System.Windows.Forms.Screen.DeviceName);
                    BindingTools.SetBinding(
                        SelectMonitor,
                        Selector.SelectedIndexProperty,
                        mainModel.AppSettings.Fullscreen,
                        nameof(FullscreenSettings.Monitor),
                        BindingMode.TwoWay,
                        UpdateSourceTrigger.PropertyChanged);
                }

                SelectTeme = Template.FindName("PART_SelectTheme", this) as Selector;
                if (SelectTeme != null)
                {
                    SelectTeme.ItemsSource = mainModel.AppSettings.Fullscreen.AvailableThemes;
                    SelectTeme.DisplayMemberPath = nameof(ThemeDescription.Name);
                    SelectTeme.SelectedValuePath = nameof(ThemeDescription.DirectoryName);
                    BindingTools.SetBinding(
                        SelectTeme,
                        Selector.SelectedValueProperty,
                        mainModel.AppSettings.Fullscreen,
                        nameof(FullscreenSettings.Theme),
                        BindingMode.TwoWay,
                        UpdateSourceTrigger.PropertyChanged);
                }

                SelectButtonPrompts = Template.FindName("PART_SelectButtonPrompts", this) as Selector;
                if (SelectButtonPrompts != null)
                {
                    SelectButtonPrompts.ItemsSource = ItemsSource.GetEnumSources(typeof(FullscreenButtonPrompts));
                    SelectButtonPrompts.DisplayMemberPath = nameof(ItemsSource.EnumItem.Name);
                    SelectButtonPrompts.SelectedValuePath = nameof(ItemsSource.EnumItem.Value);
                    BindingTools.SetBinding(
                        SelectButtonPrompts,
                        Selector.SelectedValueProperty,
                        mainModel.AppSettings.Fullscreen,
                        nameof(FullscreenSettings.ButtonPrompts),
                        BindingMode.TwoWay,
                        UpdateSourceTrigger.PropertyChanged);
                }

                ToggleGameTitles = Template.FindName("PART_ToggleGameTitles", this) as ToggleButton;
                if (ToggleGameTitles != null)
                {
                    BindingTools.SetBinding(
                       ToggleGameTitles,
                       ToggleButton.IsCheckedProperty,
                       mainModel.AppSettings.Fullscreen,
                       nameof(FullscreenSettings.ShowGameTitles),
                       BindingMode.TwoWay,
                       UpdateSourceTrigger.PropertyChanged);
                }

                ToggleBattery = Template.FindName("PART_ToggleBattery", this) as ToggleButton;
                if (ToggleBattery != null)
                {
                    BindingTools.SetBinding(
                        ToggleBattery,
                        ToggleButton.IsCheckedProperty,
                        mainModel.AppSettings.Fullscreen,
                        nameof(FullscreenSettings.ShowBattery),
                        BindingMode.TwoWay,
                        UpdateSourceTrigger.PropertyChanged);
                }

                ToggleBatteryPercentage = Template.FindName("PART_ToggleBatteryPercentage", this) as ToggleButton;
                if (ToggleBatteryPercentage != null)
                {
                    BindingTools.SetBinding(
                        ToggleBatteryPercentage,
                        ToggleButton.IsCheckedProperty,
                        mainModel.AppSettings.Fullscreen,
                        nameof(FullscreenSettings.ShowBatteryPercentage),
                        BindingMode.TwoWay,
                        UpdateSourceTrigger.PropertyChanged);
                }

                ToggleClock = Template.FindName("PART_ToggleClock", this) as ToggleButton;
                if (ToggleClock != null)
                {
                    BindingTools.SetBinding(
                        ToggleClock,
                        ToggleButton.IsCheckedProperty,
                        mainModel.AppSettings.Fullscreen,
                        nameof(FullscreenSettings.ShowClock),
                        BindingMode.TwoWay,
                        UpdateSourceTrigger.PropertyChanged);
                }

                ToggleInstalledInQFilter = Template.FindName("PART_ToggleInstalledInQFilter", this) as ToggleButton;
                if (ToggleInstalledInQFilter != null)
                {
                    BindingTools.SetBinding(
                        ToggleInstalledInQFilter,
                        ToggleButton.IsCheckedProperty,
                        mainModel.AppSettings.Fullscreen,
                        nameof(FullscreenSettings.InstalledOnlyInQuickFilters),
                        BindingMode.TwoWay,
                        UpdateSourceTrigger.PropertyChanged);
                }

                SliderColumns = Template.FindName("PART_SliderColumns", this) as Slider;
                if (SliderColumns != null)
                {
                    SliderColumns.Minimum = 1;
                    SliderColumns.Maximum = 10;
                    SliderColumns.TickFrequency = 1;
                    SliderColumns.IsSnapToTickEnabled = true;
                    BindingTools.SetBinding(
                       SliderColumns,
                       Slider.ValueProperty,
                       mainModel.AppSettings.Fullscreen,
                       nameof(FullscreenSettings.Columns),
                       BindingMode.TwoWay,
                       UpdateSourceTrigger.PropertyChanged);
                    BindingTools.SetBinding(
                         SliderColumns,
                         Slider.IsEnabledProperty,
                         mainModel.AppSettings.Fullscreen,
                         nameof(FullscreenSettings.HorizontalLayout),
                         BindingMode.OneWay,
                         UpdateSourceTrigger.PropertyChanged);
                }

                SliderRows = Template.FindName("PART_SliderRows", this) as Slider;
                if (SliderRows != null)
                {
                    SliderRows.Minimum = 1;
                    SliderRows.Maximum = 10;
                    SliderRows.TickFrequency = 1;
                    SliderRows.IsSnapToTickEnabled = true;
                    BindingTools.SetBinding(
                       SliderRows,
                       Slider.ValueProperty,
                       mainModel.AppSettings.Fullscreen,
                       nameof(FullscreenSettings.Rows),
                       BindingMode.TwoWay,
                       UpdateSourceTrigger.PropertyChanged);
                    BindingTools.SetBinding(
                         SliderRows,
                         Slider.IsEnabledProperty,
                         mainModel.AppSettings.Fullscreen,
                         nameof(FullscreenSettings.HorizontalLayout),
                         BindingMode.OneWay,
                         UpdateSourceTrigger.PropertyChanged,
                         new InvertedBoolenConverter());
                }

                SliderItemSpacing = Template.FindName("PART_SliderItemSpacing", this) as Slider;
                if (SliderItemSpacing != null)
                {
                    SliderItemSpacing.Minimum = 0;
                    SliderItemSpacing.Maximum = 40;
                    SliderItemSpacing.TickFrequency = 1;
                    SliderItemSpacing.IsSnapToTickEnabled = true;
                    BindingTools.SetBinding(
                       SliderItemSpacing,
                       Slider.ValueProperty,
                       mainModel.AppSettings,
                       nameof(PlayniteSettings.FullscreenItemSpacing),
                       BindingMode.TwoWay,
                       UpdateSourceTrigger.PropertyChanged);
                }

                ToggleHorizontalLayout = Template.FindName("PART_ToggleHorizontalLayout", this) as ToggleButton;
                if (ToggleHorizontalLayout != null)
                {
                    BindingTools.SetBinding(
                       ToggleHorizontalLayout,
                       ToggleButton.IsCheckedProperty,
                       mainModel.AppSettings.Fullscreen,
                       nameof(FullscreenSettings.HorizontalLayout),
                       BindingMode.TwoWay,
                       UpdateSourceTrigger.PropertyChanged);
                }

                ToggleDarkenUninstalled = Template.FindName("PART_ToggleDarkenUninstalled", this) as ToggleButton;
                if (ToggleDarkenUninstalled != null)
                {
                    BindingTools.SetBinding(
                       ToggleDarkenUninstalled,
                       ToggleButton.IsCheckedProperty,
                       mainModel.AppSettings.Fullscreen,
                       nameof(FullscreenSettings.DarkenUninstalledGamesGrid),
                       BindingMode.TwoWay,
                       UpdateSourceTrigger.PropertyChanged);
                }

                ToggleBackgroundOnMain = Template.FindName("PART_ToggleBackgroundOnMain", this) as ToggleButton;
                if (ToggleBackgroundOnMain != null)
                {
                    BindingTools.SetBinding(
                        ToggleBackgroundOnMain,
                        ToggleButton.IsCheckedProperty,
                        mainModel.AppSettings.Fullscreen,
                        nameof(FullscreenSettings.EnableMainBackgroundImage),
                        BindingMode.TwoWay,
                        UpdateSourceTrigger.PropertyChanged);
                }

                SliderBackgroundDarkenAmount = Template.FindName("PART_SliderBackgroundDarkenAmount", this) as Slider;
                if (SliderBackgroundDarkenAmount != null)
                {
                    SliderBackgroundDarkenAmount.Minimum = 0;
                    SliderBackgroundDarkenAmount.Maximum = 100;
                    SliderBackgroundDarkenAmount.TickFrequency = 5;
                    SliderBackgroundDarkenAmount.IsSnapToTickEnabled = true;
                    BindingTools.SetBinding(
                       SliderBackgroundDarkenAmount,
                       Slider.ValueProperty,
                       mainModel.AppSettings.Fullscreen,
                       nameof(FullscreenSettings.MainBackgroundImageDarkAmount),
                       BindingMode.TwoWay,
                       UpdateSourceTrigger.PropertyChanged);
                    BindingTools.SetBinding(
                         SliderBackgroundDarkenAmount,
                         Slider.IsEnabledProperty,
                         mainModel.AppSettings.Fullscreen,
                         nameof(FullscreenSettings.EnableMainBackgroundImage),
                         BindingMode.OneWay,
                         UpdateSourceTrigger.PropertyChanged);
                }

                SliderBackgroundBlurAmount = Template.FindName("PART_SliderBackgroundBlurAmount", this) as Slider;
                if (SliderBackgroundBlurAmount != null)
                {
                    SliderBackgroundBlurAmount.Minimum = 0;
                    SliderBackgroundBlurAmount.Maximum = 100;
                    SliderBackgroundBlurAmount.TickFrequency = 5;
                    SliderBackgroundBlurAmount.IsSnapToTickEnabled = true;
                    BindingTools.SetBinding(
                       SliderBackgroundBlurAmount,
                       Slider.ValueProperty,
                       mainModel.AppSettings.Fullscreen,
                       nameof(FullscreenSettings.MainBackgroundImageBlurAmount),
                       BindingMode.TwoWay,
                       UpdateSourceTrigger.PropertyChanged);
                    BindingTools.SetBinding(
                         SliderBackgroundBlurAmount,
                         Slider.IsEnabledProperty,
                         mainModel.AppSettings.Fullscreen,
                         nameof(FullscreenSettings.EnableMainBackgroundImage),
                         BindingMode.OneWay,
                         UpdateSourceTrigger.PropertyChanged);
                }
            }
        }
    }
}
