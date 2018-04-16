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

namespace PlayniteUI.Controls
{
    [TemplatePart(Name = "PART_TextInput", Type = typeof(TextBox))]
    [TemplatePart(Name = "PART_1", Type = typeof(Button))]
    public class VirtualKeyboard : Control
    {
        private TextBox TextBoxInput;
        private Button Button1;

        public string Text
        {
            get
            {
                return (string)GetValue(TextProperty);
            }

            set
            {
                SetValue(TextProperty, value);
            }
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(VirtualKeyboard), new PropertyMetadata(string.Empty, OnTextChanged));

        static VirtualKeyboard()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(VirtualKeyboard), new FrameworkPropertyMetadata(typeof(VirtualKeyboard)));
        }

        public VirtualKeyboard() : base()
        {
            Loaded += VirtualKeyboard_Loaded;
            
        }

        private void VirtualKeyboard_Loaded(object sender, RoutedEventArgs e)
        {
            IsVisibleChanged += VirtualKeyboard_IsVisibleChanged;
        }

        private void VirtualKeyboard_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            //if ((bool)e.NewValue == true)
            //{
            //    Button1?.Focus();
            //    Keyboard.Focus(Button1);
            //}
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (Template != null)
            {
                TextBoxInput = Template.FindName("PART_TextInput", this) as TextBox;
                Button1 = Template.FindName("PART_1", this) as Button;

                //if (Visibility == Visibility.Visible)
                //{
                //    Button1?.Focus();
                //    Keyboard.Focus(Button1);
                //}
            }
        }

        private static void OnTextChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var keyboard = (VirtualKeyboard)sender;
            //keyboard.TextBoxInput.Text = 
        }
    }
}
