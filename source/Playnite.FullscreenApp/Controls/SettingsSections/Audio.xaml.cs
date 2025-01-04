using Playnite.Audio;
using Playnite.Behaviors;
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
    public partial class Audio : SettingsSectionControl
    {
        public Audio()
        {
            InitializeComponent();
        }

        public Audio(FullscreenAppViewModel mainModel)
        {
            InitializeComponent();

            SliderInterfaceVolume.Minimum = 0;
            SliderInterfaceVolume.Maximum = 1;
            SliderInterfaceVolume.SmallChange = 0.05;
            SliderInterfaceVolume.LargeChange = 0.05;
            SliderInterfaceVolume.TickFrequency = 0.05;
            SliderInterfaceVolume.IsSnapToTickEnabled = true;
            BindingTools.SetBinding(
                SliderInterfaceVolume,
                Slider.ValueProperty,
                mainModel.AppSettings.Fullscreen,
                nameof(FullscreenSettings.InterfaceVolume),
                BindingMode.TwoWay,
                UpdateSourceTrigger.PropertyChanged);

            SliderMusicVolume.Minimum = 0;
            SliderMusicVolume.Maximum = 1;
            SliderMusicVolume.SmallChange = 0.05;
            SliderMusicVolume.LargeChange = 0.05;
            SliderMusicVolume.TickFrequency = 0.05;
            SliderMusicVolume.IsSnapToTickEnabled = true;
            BindingTools.SetBinding(
                SliderMusicVolume,
                Slider.ValueProperty,
                mainModel.AppSettings.Fullscreen,
                nameof(FullscreenSettings.BackgroundVolume),
                BindingMode.TwoWay,
                UpdateSourceTrigger.PropertyChanged);

            BindingTools.SetBinding(
                ToggleMuteInBackground,
                ToggleButton.IsCheckedProperty,
                mainModel.AppSettings.Fullscreen,
                nameof(FullscreenSettings.MuteInBackground),
                BindingMode.TwoWay,
                UpdateSourceTrigger.PropertyChanged);
        }
    }
}
