using Playnite.Input;
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
    public class ComboBoxEx : ComboBox
    {
        static ComboBoxEx()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(ComboBoxEx), new FrameworkPropertyMetadata(typeof(ComboBoxEx)));
        }

        public ComboBoxEx() : base()
        {
            PreviewKeyDown += ComboBoxEx_PreviewKeyDown;
            KeyDown += ComboBoxEx_KeyDown;
        }

        private void ComboBoxEx_KeyDown(object sender, KeyEventArgs e)
        {
            if (e is XInputEventArgs xinput)
            {
                switch (xinput.XButton)
                {
                    case XInputButton.A:
                        if (IsDropDownOpen)
                        {
                            OnKeyDown(new KeyEventArgs(e.KeyboardDevice, e.InputSource, e.Timestamp, Key.Enter) { RoutedEvent = Keyboard.KeyDownEvent });
                        }
                        else
                        {
                            OnKeyDown(new KeyEventArgs(e.KeyboardDevice, e.InputSource, e.Timestamp, Key.F4) { RoutedEvent = Keyboard.KeyDownEvent });
                        }
                        e.Handled = true;
                        break;
                    case XInputButton.B:
                        if (IsDropDownOpen)
                        {
                            OnKeyDown(new KeyEventArgs(e.KeyboardDevice, e.InputSource, e.Timestamp, Key.Escape) { RoutedEvent = Keyboard.KeyDownEvent });
                            e.Handled = true;
                        }
                        break;
                }
            }
        }

        private void ComboBoxEx_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Down && !IsDropDownOpen)
            {
                MoveFocus(new TraversalRequest(FocusNavigationDirection.Down));
                e.Handled = true;
            }
            else if (e.Key == Key.Up && !IsDropDownOpen)
            {
                MoveFocus(new TraversalRequest(FocusNavigationDirection.Up));
                e.Handled = true;
            }
            else if (e.Key == Key.Space && !IsDropDownOpen)
            {
                IsDropDownOpen = true;
                e.Handled = true;
            }
            else if (e.Key == Key.Back && IsDropDownOpen)
            {
                IsDropDownOpen = false;
                e.Handled = true;
            }
        }
    }
}
