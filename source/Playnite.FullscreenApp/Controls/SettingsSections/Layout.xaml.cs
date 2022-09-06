using Playnite.Common;
using Playnite.Converters;
using Playnite.FullscreenApp.ViewModels;
using System;
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

namespace Playnite.FullscreenApp.Controls.SettingsSections
{
    /// <summary>
    /// Interaction logic for Visuals.xaml
    /// </summary>
    public partial class Layout : UserControl
    {
        public Layout()
        {
            InitializeComponent();
        }

        public Layout(FullscreenAppViewModel mainModel)
        {
            InitializeComponent();

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

            SliderItemSpacing.Minimum = 0;
            SliderItemSpacing.Maximum = 200;
            SliderItemSpacing.TickFrequency = 1;
            SliderItemSpacing.IsSnapToTickEnabled = true;
            BindingTools.SetBinding(
                SliderItemSpacing,
                Slider.ValueProperty,
                mainModel.AppSettings,
                nameof(PlayniteSettings.FullscreenItemSpacing),
                BindingMode.TwoWay,
                UpdateSourceTrigger.PropertyChanged);

            BindingTools.SetBinding(
                ToggleHorizontalLayout,
                CheckBox.IsCheckedProperty,
                mainModel.AppSettings.Fullscreen,
                nameof(FullscreenSettings.HorizontalLayout),
                BindingMode.TwoWay,
                UpdateSourceTrigger.PropertyChanged);

            BindingTools.SetBinding(
                ToggleSmoothScrolling,
                CheckBox.IsCheckedProperty,
                mainModel.AppSettings.Fullscreen,
                nameof(FullscreenSettings.SmoothScrolling),
                BindingMode.TwoWay,
                UpdateSourceTrigger.PropertyChanged);
        }
    }
}
