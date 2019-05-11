using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Playnite.FullscreenApp.Controls
{
    public class SliderEx : Slider
    {
        static SliderEx()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SliderEx), new FrameworkPropertyMetadata(typeof(SliderEx)));
        }

        public SliderEx() : base()
        {
            PreviewKeyDown += ComboBoxEx_PreviewKeyDown;
        }

        private void ComboBoxEx_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Left || e.Key == Key.Right)
            {

            }
            else if (e.Key == Key.Down)
            {
                MoveFocus(new TraversalRequest(FocusNavigationDirection.Down));
                e.Handled = true;
            }
            else if (e.Key == Key.Up)
            {
                MoveFocus(new TraversalRequest(FocusNavigationDirection.Up));
                e.Handled = true;
            }
        }
    }
}
