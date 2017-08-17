using PlayniteUI.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PlayniteUI
{
    public class PlayniteMessageBox
    {
        private static Window CurrentWindow
        {
            get
            {
                var window = Application.Current.Windows.OfType<Window>().SingleOrDefault(w => w.IsActive);
                return window ?? Application.Current.MainWindow;
            }
        }

        public static MessageBoxResult Show(string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult, MessageBoxOptions options)
        {
            return Show(CurrentWindow, messageBoxText, caption, button, icon, defaultResult, options);
        }

        public static MessageBoxResult Show(string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult)
        {
            return Show(CurrentWindow, messageBoxText, caption, button, icon, defaultResult);
        }

        public static MessageBoxResult Show(string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon)
        {
            return Show(CurrentWindow, messageBoxText, caption, button, icon);
        }

        public static MessageBoxResult Show(string messageBoxText, string caption, MessageBoxButton button)
        {
            return Show(CurrentWindow, messageBoxText, caption, button);
        }

        public static MessageBoxResult Show(string messageBoxText, string caption)
        {
            return Show(CurrentWindow, messageBoxText, caption);
        }

        public static MessageBoxResult Show(string messageBoxText)
        {
            return Show(CurrentWindow, messageBoxText);
        }

        public static MessageBoxResult Show(Window owner, string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult, MessageBoxOptions options)
        {
            return (new MessageBoxWindow()).Show(owner, messageBoxText, caption, button, icon, defaultResult, options);
        }

        public static MessageBoxResult Show(Window owner, string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult)
        {
            return (new MessageBoxWindow()).Show(owner, messageBoxText, caption, button, icon, defaultResult, MessageBoxOptions.None);
        }

        public static MessageBoxResult Show(Window owner, string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon)
        {
            return (new MessageBoxWindow()).Show(owner, messageBoxText, caption, button, icon, MessageBoxResult.None, MessageBoxOptions.None);
        }

        public static MessageBoxResult Show(Window owner, string messageBoxText, string caption, MessageBoxButton button)
        {
            return (new MessageBoxWindow()).Show(owner, messageBoxText, caption, button, MessageBoxImage.None, MessageBoxResult.None, MessageBoxOptions.None);
        }

        public static MessageBoxResult Show(Window owner, string messageBoxText, string caption)
        {
            return (new MessageBoxWindow()).Show(owner, messageBoxText, caption, MessageBoxButton.OK, MessageBoxImage.None, MessageBoxResult.None, MessageBoxOptions.None);
        }

        public static MessageBoxResult Show(Window owner, string messageBoxText)
        {
            return (new MessageBoxWindow()).Show(owner, messageBoxText, string.Empty, MessageBoxButton.OK, MessageBoxImage.None, MessageBoxResult.None, MessageBoxOptions.None);
        }

    }
}
