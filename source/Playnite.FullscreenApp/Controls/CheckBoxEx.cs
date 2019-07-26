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
    public class CheckBoxEx : CheckBox
    {
        static CheckBoxEx()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(CheckBoxEx), new FrameworkPropertyMetadata(typeof(CheckBoxEx)));
        }

        public CheckBoxEx() : base()
        {
            KeyDown += Ex_KeyDown;
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
