using Playnite.Common;
using Playnite.Native;
using Playnite.Windows;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace Playnite.Controls
{
    public class EmptyWindowAutomationPeer : FrameworkElementAutomationPeer
    {
        private static readonly List<AutomationPeer> emptyList = new List<AutomationPeer>();

        public EmptyWindowAutomationPeer(FrameworkElement owner) : base(owner)
        {
        }

        protected override string GetNameCore()
        {
            return nameof(EmptyWindowAutomationPeer);
        }

        protected override AutomationControlType GetAutomationControlTypeCore()
        {
            return AutomationControlType.Window;
        }

        protected override List<AutomationPeer> GetChildrenCore()
        {
            return emptyList;
        }
    }

    [TemplatePart(Name = "PART_ButtonMinimize", Type = typeof(Button))]
    [TemplatePart(Name = "PART_ButtonMaximize", Type = typeof(Button))]
    [TemplatePart(Name = "PART_ButtonClose", Type = typeof(Button))]
    [TemplatePart(Name = "PART_TextTitle", Type = typeof(TextBlock))]
    public class WindowBase : Window, INotifyPropertyChanged
    {
        public readonly WindowPositionHandler PositionHandler;
        private readonly EmptyWindowAutomationPeer emptyAutomationPeer;
        private HwndSource hwndSource;
        private readonly Dictionary<int, Action> hotKeyHandlers = new Dictionary<int, Action>();

        private Button MinimizeButton;
        private Button MaximizeButton;
        private Button CloseButton;
        private TextBlock TextTitle;

        public static TextFormattingMode TextFormattingMode { get; private set; } = TextFormattingMode.Ideal;
        public static TextRenderingMode TextRenderingMode { get; private set; } = TextRenderingMode.Auto;
        public event PropertyChangedEventHandler PropertyChanged;
        public readonly Guid Id = Guid.NewGuid();

        public static readonly RoutedEvent ClosedRoutedEvent = EventManager.RegisterRoutedEvent(
            "ClosedRouted",
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(WindowBase));

        public event RoutedEventHandler ClosedRouted
        {
            add { AddHandler(ClosedRoutedEvent, value); }
            remove { RemoveHandler(ClosedRoutedEvent, value); }
        }

        public static readonly RoutedEvent LoadedRoutedEvent = EventManager.RegisterRoutedEvent(
            "LoadedRouted",
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(WindowBase));

        public event RoutedEventHandler LoadedRouted
        {
            add { AddHandler(LoadedRoutedEvent, value); }
            remove { RemoveHandler(LoadedRoutedEvent, value); }
        }

        public static readonly RoutedEvent ActivatedRoutedEvent = EventManager.RegisterRoutedEvent(
            "ActivatedRouted",
            RoutingStrategy.Bubble,
            typeof(RoutedEventHandler),
            typeof(WindowBase));

        public event RoutedEventHandler ActivatedRouted
        {
            add { AddHandler(ActivatedRoutedEvent, value); }
            remove { RemoveHandler(ActivatedRoutedEvent, value); }
        }

        // The reason we currently don't have accessibility/automation interaface enabled is because of performance.
        // For some reason certain controls, like listviews, degrade a lot performance wise when accessibility is enabled.
        // It doesn't seem to be an issue in Playnite itself from my testing (content of listview basically doesn't matter),
        // so most likely WPF bug.
        protected override AutomationPeer OnCreateAutomationPeer()
        {
            var acc = PlayniteApplication.Current?.AppSettings.AccessibilityInterface;
            if (acc != null)
            {
                switch (acc.Value)
                {
                    case AccessibilityInterfaceOptions.Auto:
                        return Computer.GetScreenReaderActive() ? base.OnCreateAutomationPeer() : emptyAutomationPeer;
                    case AccessibilityInterfaceOptions.AlwaysOn:
                        return base.OnCreateAutomationPeer();
                    case AccessibilityInterfaceOptions.AlwaysOff:
                    default:
                        return emptyAutomationPeer;
                }
            }
            else
            {
                return emptyAutomationPeer;
            }
        }

        public bool HasChildWindow
        {
            get => WindowManager.GetHasChild(this);
        }

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

        public bool BlockAltF4
        {
            get
            {
                return (bool)GetValue(BlockAltF4Property);
            }

            set
            {
                SetValue(BlockAltF4Property, value);
            }
        }

        public static readonly DependencyProperty ShowMinimizeButtonProperty =
            DependencyProperty.Register(nameof(ShowMinimizeButton), typeof(bool), typeof(WindowBase), new PropertyMetadata(true, ShowMinimizeButtonPropertyChanged));
        public static readonly DependencyProperty ShowMaximizeButtonProperty =
            DependencyProperty.Register(nameof(ShowMaximizeButton), typeof(bool), typeof(WindowBase), new PropertyMetadata(true, ShowMaximizeButtonPropertyChanged));
        public static readonly DependencyProperty ShowCloseButtonProperty =
            DependencyProperty.Register(nameof(ShowCloseButton), typeof(bool), typeof(WindowBase), new PropertyMetadata(true, ShowCloseButtonPropertyChanged));
        public static readonly DependencyProperty ShowTitleProperty =
            DependencyProperty.Register(nameof(ShowTitle), typeof(bool), typeof(WindowBase), new PropertyMetadata(true, ShowTitlePropertyChanged));
        public static readonly DependencyProperty BlockAltF4Property =
            DependencyProperty.Register(nameof(BlockAltF4), typeof(bool), typeof(WindowBase), new PropertyMetadata(false));

        public bool IsShown { get; private set; }
        public bool WasClosed { get; private set; }
        public IntPtr Handle { get; private set; }
        public bool? DialogResultFixed { get; set; } = null;

        static WindowBase()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(WindowBase), new FrameworkPropertyMetadata(typeof(WindowBase)));
        }

        public WindowBase() : base()
        {
            emptyAutomationPeer = new EmptyWindowAutomationPeer(this);
            Style defaultStyle = (Style)Application.Current?.TryFindResource(typeof(WindowBase));
            if (defaultStyle != null)
            {
                Style = defaultStyle;
            }

            if (Localization.IsRightToLeft)
            {
                FlowDirection = FlowDirection.RightToLeft;
            }

            TextOptions.SetTextFormattingMode(this, TextFormattingMode);
            TextOptions.SetTextRenderingMode(this, TextRenderingMode);
            Closed += (_, __) =>
            {
                hwndSource?.RemoveHook(HwndHook);
                IsShown = false;
                WasClosed = true;
                RaiseEvent(new RoutedEventArgs(ClosedRoutedEvent));
            };

            Loaded += (_, __) =>
            {
                Handle = new WindowInteropHelper(this).Handle;
                hwndSource = HwndSource.FromHwnd(Handle);
                hwndSource.AddHook(HwndHook);
                IsShown = true;
                RaiseEvent(new RoutedEventArgs(LoadedRoutedEvent));
            };

            Activated += (_, __) =>
            {
                RaiseEvent(new RoutedEventArgs(ActivatedRoutedEvent));
            };

            PreviewKeyDown += (_, e) =>
            {
                if (e.Key == Key.System && e.SystemKey == Key.F4 && BlockAltF4)
                {
                    e.Handled = true;
                }
            };

            // This fixes an issue if SizeToContent is used on windows with custom WindowChrome (all Playnite windows)
            // https://stackoverflow.com/questions/29207331/wpf-window-with-custom-chrome-has-unwanted-outline-on-right-and-bottom
            SourceInitialized += (_, e) =>
            {
                if (SizeToContent == SizeToContent.WidthAndHeight)
                {
                    InvalidateMeasure();
                }
            };

            if (PlayniteApplication.Current?.AppSettings != null)
            {
                if (PlayniteApplication.Current.Mode == SDK.ApplicationMode.Fullscreen)
                {
                    IsHitTestVisible = !PlayniteApplication.Current.AppSettings.Fullscreen.HideMouserCursor;
                }
            }
        }

        public WindowBase(string savePositionName, bool saveSize = true) : this()
        {
            if (PlayniteApplication.Current?.AppSettings != null)
            {
                PositionHandler = new WindowPositionHandler(this, savePositionName, PlayniteApplication.Current.AppSettings.WindowPositions, saveSize);
            }
        }

        public static void SetTextRenderingOptions(TextFormattingModeOptions formatting, TextRenderingModeOptions rendering)
        {
            TextFormattingMode = (TextFormattingMode)formatting;
            TextRenderingMode = (TextRenderingMode)rendering;
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

        public void OnPropertyChanged([CallerMemberName]string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private IntPtr HwndHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            if (msg == Winuser.WM_HOTKEY)
            {
                var hotKeyId = wParam.ToInt32();
                if (hotKeyHandlers.TryGetValue(hotKeyId, out var handler))
                {
                    handler();
                    handled = true;
                }
            }

            return IntPtr.Zero;
        }

        public void RegisterHotKeyHandler(int hotKeyId, HotKey hotKey, Action handler)
        {
            var success = User32.RegisterHotKey(Handle, hotKeyId, (uint)hotKey.Modifiers, (uint)KeyInterop.VirtualKeyFromKey(hotKey.Key));
            if (success)
            {
                hotKeyHandlers.AddOrUpdate(hotKeyId, handler);
            }
            else
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
        }

        public void UnregisterHotKeyHandler(int hotKeyId)
        {
            if (!hotKeyHandlers.ContainsKey(hotKeyId))
            {
                return;
            }

            if (WasClosed)
            {
                return;
            }

            var success = User32.UnregisterHotKey(Handle, hotKeyId);
            if (success)
            {
                hotKeyHandlers.Remove(hotKeyId);
            }
            else
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }
        }
    }
}