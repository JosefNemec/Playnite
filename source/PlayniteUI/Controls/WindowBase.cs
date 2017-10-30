using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace PlayniteUI.Controls
{
    public class WindowBase : Window
    {
        private Button MinimizeButton;
        private Button MaximizeButton;
        private Button CloseButton;
        private TextBlock TextTitle;

        // TODO: Change to proper dependency properties
        public bool ShowMinimizeButton
        {
            get; set;
        } = true;

        public bool ShowMaximizeButton
        {
            get; set;
        } = true;

        public bool ShowCloseButton
        {
            get; set;
        } = true;

        public bool ShowTitle
        {
            get; set;
        } = true;

        static WindowBase()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(WindowBase), new FrameworkPropertyMetadata(typeof(WindowBase)));
        }

        public WindowBase() : base()
        {
            Loaded += WindowBase_Loaded;

            Style defaultStyle = (Style)Application.Current.TryFindResource(typeof(WindowBase));
            if (defaultStyle != null)
            {
                Style = defaultStyle;
            }
        }

        private void WindowBase_Loaded(object sender, RoutedEventArgs e)
        {
            MinimizeButton = Template.FindName("ButtonMinimize", this) as Button;
            MinimizeButton.Click += MinimizeButton_Click; ;
            MinimizeButton.Visibility = ShowMinimizeButton == true ? Visibility.Visible : Visibility.Collapsed;

            MaximizeButton = Template.FindName("ButtonMaximize", this) as Button;
            MaximizeButton.Click += MaximizeButton_Click;
            MaximizeButton.Visibility = ShowMaximizeButton == true ? Visibility.Visible : Visibility.Collapsed;

            CloseButton = Template.FindName("ButtonClose", this) as Button;
            CloseButton.Click += CloseButton_Click; ;
            CloseButton.Visibility = ShowCloseButton == true ? Visibility.Visible : Visibility.Collapsed;

            TextTitle = Template.FindName("TextTitle", this) as TextBlock;
            TextTitle.Visibility = ShowTitle == true ? Visibility.Visible : Visibility.Collapsed;
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void MinimizeButton_Click(object sender, RoutedEventArgs e)
        {
            WindowState = WindowState.Minimized;
        }

        private void MaximizeButton_Click(object sender, RoutedEventArgs e)
        {
            if (WindowState == WindowState.Maximized)
            {
                WindowState = WindowState.Normal;
            }
            else
            {
                WindowState = WindowState.Maximized;
            }
        }
    }
}
