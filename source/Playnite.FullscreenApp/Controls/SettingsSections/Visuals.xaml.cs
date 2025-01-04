using Playnite.Common;
using Playnite.FullscreenApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Playnite.FullscreenApp.Controls.SettingsSections
{
    /// <summary>
    /// Interaction logic for Visuals.xaml
    /// </summary>
    public partial class Visuals : SettingsSectionControl
    {
        public Visuals()
        {
            InitializeComponent();
        }

        public Visuals(FullscreenAppViewModel mainModel)
        {
            InitializeComponent();

            SelectTheme.ItemsSource = mainModel.AppSettings.Fullscreen.AvailableThemes;
            SelectTheme.DisplayMemberPath = nameof(ThemeManifest.Name);
            SelectTheme.SelectedValuePath = nameof(ThemeManifest.Id);
            BindingTools.SetBinding(
                SelectTheme,
                Selector.SelectedValueProperty,
                mainModel.AppSettings.Fullscreen,
                nameof(FullscreenSettings.Theme),
                BindingMode.TwoWay,
                UpdateSourceTrigger.PropertyChanged);

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

            BindingTools.SetBinding(
                ToggleGameTitles,
                ToggleButton.IsCheckedProperty,
                mainModel.AppSettings.Fullscreen,
                nameof(FullscreenSettings.ShowGameTitles),
                BindingMode.TwoWay,
                UpdateSourceTrigger.PropertyChanged);

            BindingTools.SetBinding(
                ToggleDarkenUninstalled,
                ToggleButton.IsCheckedProperty,
                mainModel.AppSettings.Fullscreen,
                nameof(FullscreenSettings.DarkenUninstalledGamesGrid),
                BindingMode.TwoWay,
                UpdateSourceTrigger.PropertyChanged);

            BindingTools.SetBinding(
                ToggleBackgroundOnMain,
                ToggleButton.IsCheckedProperty,
                mainModel.AppSettings.Fullscreen,
                nameof(FullscreenSettings.EnableMainBackgroundImage),
                BindingMode.TwoWay,
                UpdateSourceTrigger.PropertyChanged);

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

            SliderFontSize.Minimum = 10;
            SliderFontSize.Maximum = 40;
            SliderFontSize.TickFrequency = 1;
            SliderFontSize.IsSnapToTickEnabled = true;
            BindingTools.SetBinding(
                SliderFontSize,
                Slider.ValueProperty,
                mainModel.AppSettings.Fullscreen,
                nameof(FullscreenSettings.FontSize),
                BindingMode.TwoWay,
                UpdateSourceTrigger.PropertyChanged);

            SliderFontSizeSmall.Minimum = 10;
            SliderFontSizeSmall.Maximum = 40;
            SliderFontSizeSmall.TickFrequency = 1;
            SliderFontSizeSmall.IsSnapToTickEnabled = true;
            BindingTools.SetBinding(
                SliderFontSizeSmall,
                Slider.ValueProperty,
                mainModel.AppSettings.Fullscreen,
                nameof(FullscreenSettings.FontSizeSmall),
                BindingMode.TwoWay,
                UpdateSourceTrigger.PropertyChanged);
        }
    }
}
