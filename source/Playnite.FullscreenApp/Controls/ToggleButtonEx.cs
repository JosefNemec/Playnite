using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using Playnite.Input;

namespace Playnite.FullscreenApp.Controls
{
    public class ToggleButtonEx : ToggleButton
    {
        static ToggleButtonEx()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ToggleButtonEx), new FrameworkPropertyMetadata(typeof(ToggleButtonEx)));
        }

        public ToggleButtonEx() : base()
        {
            KeyDown += Ex_KeyDown;
        }

        private void Ex_KeyDown(object sender, KeyEventArgs e)
        {
            if (e is XInputEventArgs xinput)
            {
                if (xinput.XButton == XInputGesture.ConfirmationBinding)
                {
                    OnClick();
                    e.Handled = true;
                }
            }
        }
    }
}
