using System;
using System.Collections.Generic;
using System.ComponentModel;
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

namespace Playnite.DesktopApp.Controls
{
    /// <summary>
    /// Interaction logic for SearchBox.xaml
    /// </summary>
    public partial class SearchBox : UserControl
    {
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

        public SearchBox()
        {
            InitializeComponent();
            TextFilter.KeyUp += TextFilter_KeyUp;
        }

        public void ClearFocus()
        {
            if (previousFocus != null)
            {                
                Keyboard.Focus(previousFocus);
            }
            else
            {
                TextFilter.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
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
            TextFilter.Clear();
        }

        private static void TextPropertyChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var obj = sender as SearchBox;
            if (obj.ignoreTextCallback)
            {
                return;
            }

            var currentCurret = obj.TextFilter.CaretIndex;                 
            if (currentCurret == 0 && obj.TextFilter.Text.Length > 0 && obj.oldCarret != obj.TextFilter.Text.Length)
            {
                obj.TextFilter.CaretIndex = obj.oldCarret;
            }

            obj.oldCarret = obj.TextFilter.CaretIndex;
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

            if (!shouldFocus && !obj.TextFilter.IsFocused)
            {
                return;
            }

            if (shouldFocus == true)
            {
                obj.previousFocus = Keyboard.FocusedElement;
                obj.TextFilter.Focus();
            }
            else
            {
                obj.ClearFocus();
            }
        }

        private void TextFilter_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (ignoreTextCallback)
            {
                return;
            }

            ignoreTextCallback = true;
            Text = TextFilter.Text;
            ignoreTextCallback = false;
        }
    }
}
