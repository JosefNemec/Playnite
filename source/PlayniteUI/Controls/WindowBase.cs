﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace PlayniteUI.Controls
{
    [TemplatePart(Name = "PART_ButtonMinimize", Type = typeof(Button))]
    [TemplatePart(Name = "PART_ButtonMaximize", Type = typeof(Button))]
    [TemplatePart(Name = "PART_ButtonClose", Type = typeof(Button))]
    [TemplatePart(Name = "PART_TextTitle", Type = typeof(TextBlock))]
    public class WindowBase : Window
    {
        private Button MinimizeButton;
        private Button MaximizeButton;
        private Button CloseButton;
        private TextBlock TextTitle;

        public bool ShowMinimizeButton
        {
            get
            {
                return (bool)GetValue(ShowMinimizeButtonProperty);
            }

            set
            {
                SetValue(ShowMinimizeButtonProperty, value);
            }
        }

        public bool ShowMaximizeButton
        {
            get
            {
                return (bool)GetValue(ShowMaximizeButtonProperty);
            }

            set
            {
                SetValue(ShowMaximizeButtonProperty, value);
            }
        }

        public bool ShowCloseButton
        {
            get
            {
                return (bool)GetValue(ShowCloseButtonProperty);
            }

            set
            {
                SetValue(ShowCloseButtonProperty, value);
            }
        }

        public bool ShowTitle
        {
            get
            {
                return (bool)GetValue(ShowTitleProperty);
            }

            set
            {
                SetValue(ShowTitleProperty, value);
            }
        }

        public static readonly DependencyProperty ShowMinimizeButtonProperty =
            DependencyProperty.Register("ShowMinimizeButton", typeof(bool), typeof(WindowBase), new PropertyMetadata(true, ShowMinimizeButtonPropertyChanged));
        public static readonly DependencyProperty ShowMaximizeButtonProperty =
            DependencyProperty.Register("ShowMaximizeButton", typeof(bool), typeof(WindowBase), new PropertyMetadata(true, ShowMaximizeButtonPropertyChanged));
        public static readonly DependencyProperty ShowCloseButtonProperty =
            DependencyProperty.Register("ShowCloseButton", typeof(bool), typeof(WindowBase), new PropertyMetadata(true, ShowCloseButtonPropertyChanged));
        public static readonly DependencyProperty ShowTitleProperty =
            DependencyProperty.Register("ShowTitle", typeof(bool), typeof(WindowBase), new PropertyMetadata(true, ShowTitlePropertyChanged));

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

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (Template != null)
            {
                MinimizeButton = Template.FindName("PART_ButtonMinimize", this) as Button;
                if (MinimizeButton != null)
                {
                    MinimizeButton.Click += MinimizeButton_Click;
                    MinimizeButton.Visibility = ShowMinimizeButton == true ? Visibility.Visible : Visibility.Collapsed;
                }

                MaximizeButton = Template.FindName("PART_ButtonMaximize", this) as Button;
                if (MaximizeButton != null)
                {
                    MaximizeButton.Click += MaximizeButton_Click;
                    MaximizeButton.Visibility = ShowMaximizeButton == true ? Visibility.Visible : Visibility.Collapsed;
                }

                CloseButton = Template.FindName("PART_ButtonClose", this) as Button;
                if (CloseButton != null)
                {
                    CloseButton.Click += CloseButton_Click;
                    CloseButton.Visibility = ShowCloseButton == true ? Visibility.Visible : Visibility.Collapsed;
                }

                TextTitle = Template.FindName("PART_TextTitle", this) as TextBlock;
                if (TextTitle != null)
                {
                    TextTitle.Visibility = ShowTitle == true ? Visibility.Visible : Visibility.Collapsed;
                }
            }
        }

        private void WindowBase_Loaded(object sender, RoutedEventArgs e)
        {

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

        private static void ShowTitlePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var window = (WindowBase)sender;
            if (window.TextTitle != null)
            {
                window.TextTitle.Visibility = (bool)e.NewValue == true ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private static void ShowCloseButtonPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var window = (WindowBase)sender;
            if (window.CloseButton != null)
            {
                window.CloseButton.Visibility = (bool)e.NewValue == true ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private static void ShowMaximizeButtonPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var window = (WindowBase)sender;
            if (window.MaximizeButton != null)
            {
                window.MaximizeButton.Visibility = (bool)e.NewValue == true ? Visibility.Visible : Visibility.Collapsed;
            }
        }

        private static void ShowMinimizeButtonPropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var window = (WindowBase)sender;
            if (window.MinimizeButton != null)
            {
                window.MinimizeButton.Visibility = (bool)e.NewValue == true ? Visibility.Visible : Visibility.Collapsed;
            }
        }
    }
}
