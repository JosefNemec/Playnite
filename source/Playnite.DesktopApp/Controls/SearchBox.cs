using Playnite.Common;
using Playnite.DesktopApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Playnite.DesktopApp.Controls
{
    [TemplatePart(Name = "PART_SeachIcon", Type = typeof(FrameworkElement))]
    [TemplatePart(Name = "PART_ClearTextIcon", Type = typeof(FrameworkElement))]
    [TemplatePart(Name = "PART_TextInpuText", Type = typeof(TextBox))]
    public class SearchBox : Control
    {
        private FrameworkElement ElemSeachIcon;
        private FrameworkElement ElemClearTextIcon;
        private TextBox TextInputText;

        private int oldCarret;
        private bool ignoreTextCallback;
        internal IInputElement previousFocus;

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

        public static readonly DependencyProperty TextProperty = DependencyProperty.Register(nameof(Text), typeof(string), typeof(SearchBox), new PropertyMetadata(string.Empty, TextPropertyChangedCallback));

        public bool ShowImage
        {
            get
            {
                return (bool)GetValue(ShowImageProperty);
            }

            set
            {
                SetValue(ShowImageProperty, value);
            }
        }

        public static readonly DependencyProperty ShowImageProperty = DependencyProperty.Register(nameof(ShowImage), typeof(bool), typeof(SearchBox), new PropertyMetadata(true, ShowImagePropertyChangedCallback));

        public new bool IsFocused
        {
            get
            {
                return (bool)GetValue(IsFocusedProperty);
            }

            set
            {
                SetValue(IsFocusedProperty, value);
            }
        }

        public new static readonly DependencyProperty IsFocusedProperty = DependencyProperty.Register(nameof(IsFocused), typeof(bool), typeof(SearchBox), new PropertyMetadata(false, IsFocusedPropertyChangedCallback));

        static SearchBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(SearchBox), new FrameworkPropertyMetadata(typeof(SearchBox)));
        }

        public SearchBox()
        {
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            ElemSeachIcon = Template.FindName("PART_SeachIcon", this) as FrameworkElement;
            if (ElemSeachIcon != null)
            {
            }

            ElemClearTextIcon = Template.FindName("PART_ClearTextIcon", this) as FrameworkElement;
            if (ElemClearTextIcon != null)
            {
                ElemClearTextIcon.MouseUp += ClearImage_MouseUp;
            }

            TextInputText = Template.FindName("PART_TextInpuText", this) as TextBox;
            if (TextInputText != null)
            {
                TextInputText.TextChanged += TextFilter_TextChanged;
                TextInputText.KeyUp += TextFilter_KeyUp;
                TextInputText.GotFocus += TextInputText_GotFocus;
                TextInputText.LostFocus += TextInputText_GotFocus;

                BindingTools.SetBinding(
                    TextInputText,
                    TextBox.TextProperty,
                    this,
                    nameof(Text),
                    mode: System.Windows.Data.BindingMode.OneWay,
                    trigger: System.Windows.Data.UpdateSourceTrigger.PropertyChanged);
            }

            UpdateIconStates();
        }

        private void TextInputText_GotFocus(object sender, RoutedEventArgs e)
        {
            UpdateIconStates();
        }

        private void UpdateIconStates()
        {
            if (TextInputText.IsFocused)
            {
                ElemSeachIcon.Visibility = Visibility.Collapsed;
            }

            if (Text.IsNullOrEmpty())
            {
                ElemClearTextIcon.Visibility = Visibility.Collapsed;
                if (!TextInputText.IsFocused)
                {
                    ElemSeachIcon.Visibility = ShowImage ? Visibility.Visible : Visibility.Collapsed;
                }
            }
            else
            {
                ElemClearTextIcon.Visibility = Visibility.Visible;
                if (!TextInputText.IsFocused)
                {
                    ElemSeachIcon.Visibility = Visibility.Collapsed;
                }
            }
        }

        public void ClearFocus()
        {
            if (previousFocus != null)
            {
                Keyboard.Focus(previousFocus);
            }
            else
            {
                TextInputText.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
            }

            previousFocus = null;
            IsFocused = false;
        }

        private void TextFilter_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape || e.Key == Key.Enter)
            {
                ClearFocus();
            }
        }

        private void ClearImage_MouseUp(object sender, MouseButtonEventArgs e)
        {
            TextInputText.Clear();
        }

        private void TextFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ignoreTextCallback)
            {
                return;
            }

            ignoreTextCallback = true;
            Text = TextInputText.Text;
            ignoreTextCallback = false;
            UpdateIconStates();
        }

        private static void TextPropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var obj = sender as SearchBox;
            if (obj.ignoreTextCallback)
            {
                return;
            }

            if (obj.TextInputText != null)
            {
                var currentCurret = obj.TextInputText.CaretIndex;
                if (currentCurret == 0 && obj.TextInputText.Text.Length > 0 && obj.oldCarret != obj.TextInputText.Text.Length)
                {
                    obj.TextInputText.CaretIndex = obj.oldCarret;
                }

                obj.oldCarret = obj.TextInputText.CaretIndex;
            }
        }

        private static void ShowImagePropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var obj = sender as SearchBox;
            obj.ShowImage = (bool)e.NewValue;
        }

        private static void IsFocusedPropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var obj = sender as SearchBox;
            var shouldFocus = (bool)e.NewValue;

            if (!shouldFocus && !obj.TextInputText.IsFocused)
            {
                return;
            }

            if (shouldFocus == true)
            {
                obj.previousFocus = Keyboard.FocusedElement;
                obj.TextInputText.Focus();
            }
            else
            {
                obj.ClearFocus();
            }

            obj.UpdateIconStates();
        }
    }
}
