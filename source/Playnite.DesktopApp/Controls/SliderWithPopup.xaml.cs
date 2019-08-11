using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Playnite.DesktopApp.Controls
{
    /// <summary>
    /// Interaction logic for SliderWithPopup.xaml
    /// </summary>
    public partial class SliderWithPopup : UserControl
    {
        public SliderWithPopup()
        {
            InitializeComponent();
        }

        public double SliderValue
        {
            get => (double) GetValue(SliderValueProperty);
            set => SetValue(SliderValueProperty, value);
        }
        public static readonly DependencyProperty SliderValueProperty = DependencyProperty.Register(nameof(SliderValue), typeof(double), typeof(SliderWithPopup));
        public double SliderMaximumValue
        {
            get => (double)GetValue(SliderMaximumValueProperty);
            set => SetValue(SliderMaximumValueProperty, value);
        }
        public static readonly DependencyProperty SliderMaximumValueProperty = DependencyProperty.Register(nameof(SliderMaximumValue), typeof(double), typeof(SliderWithPopup));
        public double SliderMinimumValue
        {
            get => (double)GetValue(SliderMinimumValueProperty);
            set => SetValue(SliderMinimumValueProperty, value);
        }
        public static readonly DependencyProperty SliderMinimumValueProperty = DependencyProperty.Register(nameof(SliderMinimumValue), typeof(double), typeof(SliderWithPopup));
        public string PopupLabel
        {
            get => (string)GetValue(PopupLabelProperty);
            set => SetValue(PopupLabelProperty, value);
        }
        public static readonly DependencyProperty PopupLabelProperty = DependencyProperty.Register(nameof(PopupLabel), typeof(string), typeof(SliderWithPopup));

        private void Slider_MouseMove(object sender, MouseEventArgs e)
        {
            //if (!Popup.IsOpen)
            //{
            //    Popup.IsOpen = true;
            //}

            //PositionPopupBelowTheEndOfTheSlider();
        }

        private void PositionPopupBelowTheEndOfTheSlider()
        {
            //Popup.HorizontalOffset = Slider.Width - PopupGrid.ActualWidth;
            //Popup.VerticalOffset = 0f;
        }

        private void Slider_MouseLeave(object sender, MouseEventArgs e)
        {
            //if (Popup.IsOpen)
            //{
            //    Popup.IsOpen = false;
            //}
        }
    }
}
