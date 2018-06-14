using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Interactivity;
using System.Windows.Media;
using ControlzEx.Behaviors;

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
            this.InitializeBehaviors();
        }

        private void InitializeBehaviors()
        {
            this.InitializeWindowChromeBehavior();
            this.InitializeGlowWindowBehavior();
        }

        /// <summary>
        /// Initializes the WindowChromeBehavior which is needed to render the custom WindowChrome.
        /// </summary>
        private void InitializeWindowChromeBehavior()
        {
            var behavior = new WindowChromeBehavior();
            BindingOperations.SetBinding(behavior, WindowChromeBehavior.TryToBeFlickerFreeProperty, new Binding { Path = new PropertyPath(TryToBeFlickerFreeProperty), Source = this });
            BindingOperations.SetBinding(behavior, WindowChromeBehavior.ResizeBorderThicknessProperty, new Binding { Path = new PropertyPath(ResizeBorderThicknessProperty), Source = this });
            BindingOperations.SetBinding(behavior, WindowChromeBehavior.IgnoreTaskbarOnMaximizeProperty, new Binding { Path = new PropertyPath(IgnoreTaskbarOnMaximizeProperty), Source = this });
            BindingOperations.SetBinding(behavior, WindowChromeBehavior.GlowBrushProperty, new Binding { Path = new PropertyPath(GlowBrushProperty), Source = this });
            BindingOperations.SetBinding(behavior, WindowChromeBehavior.CaptionHeightProperty, new Binding { Path = new PropertyPath(CaptionHeightProperty), Source = this });

            Interaction.GetBehaviors(this).Add(behavior);
        }

        /// <summary>
        /// Initializes the WindowChromeBehavior which is needed to render the custom WindowChrome.
        /// </summary>
        private void InitializeGlowWindowBehavior()
        {
            var behavior = new GlowWindowBehavior();
            BindingOperations.SetBinding(behavior, GlowWindowBehavior.ResizeBorderThicknessProperty, new Binding { Path = new PropertyPath(ResizeBorderThicknessProperty), Source = this });
            BindingOperations.SetBinding(behavior, GlowWindowBehavior.GlowBrushProperty, new Binding { Path = new PropertyPath(GlowBrushProperty), Source = this });
            BindingOperations.SetBinding(behavior, GlowWindowBehavior.NonActiveGlowBrushProperty, new Binding { Path = new PropertyPath(NonActiveGlowBrushProperty), Source = this });

            Interaction.GetBehaviors(this).Add(behavior);
        }

        public static readonly DependencyProperty TryToBeFlickerFreeProperty = DependencyProperty.Register(nameof(TryToBeFlickerFree), typeof(bool), typeof(WindowBase), new PropertyMetadata(false));

        public bool TryToBeFlickerFree
        {
            get { return (bool)this.GetValue(TryToBeFlickerFreeProperty); }
            set { this.SetValue(TryToBeFlickerFreeProperty, value); }
        }

        public static readonly DependencyProperty CaptionHeightProperty = DependencyProperty.Register(nameof(CaptionHeight), typeof(double), typeof(WindowBase), new PropertyMetadata(WindowChromeBehavior.CaptionHeightProperty.DefaultMetadata.DefaultValue));

        /// <summary>The extent of the top of the window to treat as the caption.</summary>
        public double CaptionHeight
        {
            get { return (double)this.GetValue(CaptionHeightProperty); }
            set { this.SetValue(CaptionHeightProperty, value); }
        }


        public Thickness ResizeBorderThickness
        {
            get { return (Thickness)this.GetValue(ResizeBorderThicknessProperty); }
            set { this.SetValue(ResizeBorderThicknessProperty, value); }
        }

        // Using a DependencyProperty as the backing store for ResizeBorderThickness.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ResizeBorderThicknessProperty =
            DependencyProperty.Register(nameof(ResizeBorderThickness), typeof(Thickness), typeof(WindowBase), new PropertyMetadata(WindowChromeBehavior.ResizeBorderThicknessProperty.DefaultMetadata.DefaultValue));

        public static readonly DependencyProperty IgnoreTaskbarOnMaximizeProperty = DependencyProperty.Register(nameof(IgnoreTaskbarOnMaximize), typeof(bool), typeof(WindowBase), new PropertyMetadata(WindowChromeBehavior.IgnoreTaskbarOnMaximizeProperty.DefaultMetadata.DefaultValue));

        public bool IgnoreTaskbarOnMaximize
        {
            get { return (bool)this.GetValue(IgnoreTaskbarOnMaximizeProperty); }
            set { this.SetValue(IgnoreTaskbarOnMaximizeProperty, value); }
        }

        /// <summary>
        /// <see cref="DependencyProperty"/> for <see cref="GlowBrush"/>.
        /// </summary>
        public static readonly DependencyProperty GlowBrushProperty = DependencyProperty.Register(nameof(GlowBrush), typeof(Brush), typeof(WindowBase), new PropertyMetadata(default(Brush)));

        /// <summary>
        /// Gets or sets a brush which is used as the glow when the window is active.
        /// </summary>
        public Brush GlowBrush
        {
            get { return (Brush)this.GetValue(GlowBrushProperty); }
            set { this.SetValue(GlowBrushProperty, value); }
        }

        /// <summary>
        /// <see cref="DependencyProperty"/> for <see cref="NonActiveGlowBrush"/>.
        /// </summary>
        public static readonly DependencyProperty NonActiveGlowBrushProperty = DependencyProperty.Register(nameof(NonActiveGlowBrush), typeof(Brush), typeof(WindowBase), new PropertyMetadata(default(Brush)));

        /// <summary>
        /// Gets or sets a brush which is used as the glow when the window is not active.
        /// </summary>
        public Brush NonActiveGlowBrush
        {
            get { return (Brush)this.GetValue(NonActiveGlowBrushProperty); }
            set { this.SetValue(NonActiveGlowBrushProperty, value); }
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
