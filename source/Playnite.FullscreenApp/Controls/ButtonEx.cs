using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Playnite.Input;

namespace Playnite.FullscreenApp.Controls
{
    public class ButtonEx : Button
    {
        public string TooltipEx
        {
            get
            {
                return (string)GetValue(TooltipExProperty);
            }

            set
            {
                SetValue(TooltipExProperty, value);
            }
        }

        public static readonly DependencyProperty TooltipExProperty = DependencyProperty.Register(
            nameof(TooltipEx),
            typeof(string),
            typeof(ButtonEx),
            new PropertyMetadata(null));

        static ButtonEx()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ButtonEx), new FrameworkPropertyMetadata(typeof(ButtonEx)));
        }

        public ButtonEx() : base()
        {
            KeyDown += Ex_KeyDown;
            GotFocus += ButtonEx_GotFocus;
            LostFocus += ButtonEx_LostFocus;
        }

        private void ButtonEx_LostFocus(object sender, RoutedEventArgs e)
        {
            if (!TooltipEx.IsNullOrEmpty() && ToolTip is ToolTip tooltip)
            {
                tooltip.StaysOpen = false;
                tooltip.IsOpen = false;
                ToolTip = null;
            }
        }

        private void ButtonEx_GotFocus(object sender, RoutedEventArgs e)
        {
            if (!TooltipEx.IsNullOrEmpty())
            {
                var tooltipObj = new ToolTip
                {
                    Content = TooltipEx,
                    StaysOpen = true,
                    IsOpen = true,
                    PlacementTarget = this,
                    Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom
                };

                ToolTip = tooltipObj;
            }
        }

        private void Ex_KeyDown(object sender, KeyEventArgs e)
        {
            if (e is XInputEventArgs xinput)
            {
                if (xinput.XButton == XInputButton.A)
                {
                    OnClick();
                    e.Handled = true;
                }
            }
        }
    }
}
