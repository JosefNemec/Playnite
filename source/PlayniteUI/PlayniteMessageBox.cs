using Playnite.SDK;
using PlayniteUI.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace PlayniteUI
{
    public class DialogsFactory : IDialogsFactory
    {
        private readonly SynchronizationContext context;

        public DialogsFactory()
        {
            context = SynchronizationContext.Current;
        }

        private T Invoke<T>(Func<T> action)
        {
            T result = default(T);
            context.Send((a) =>
            {
                result = action();
            }, null);

            return result;
        }

        public string SaveFile(string filter)
        {
            return Invoke(() => Dialogs.SaveFile(PlayniteWindows.CurrentWindow, filter));
        }

        public string SaveFile(string filter, bool promptOverwrite)
        {
            return Invoke(() => Dialogs.SaveFile(PlayniteWindows.CurrentWindow, filter, promptOverwrite));
        }

        public string SelectFile(string filter)
        {
            return Invoke(() => Dialogs.SelectFile(PlayniteWindows.CurrentWindow, filter));
        }

        public List<string> SelectFiles(string filter)
        {
            return Invoke(() => Dialogs.SelectFiles(PlayniteWindows.CurrentWindow, filter));
        }

        public string SelectFolder()
        {
            return Invoke(() => Dialogs.SelectFolder(PlayniteWindows.CurrentWindow));
        }

        public string SelectIconFile()
        {
            return Invoke(() => Dialogs.SelectIconFile(PlayniteWindows.CurrentWindow));
        }

        public string SelectImagefile()
        {
            return Invoke(() => Dialogs.SelectImageFile(PlayniteWindows.CurrentWindow));
        }

        public MessageBoxResult SelectString(string messageBoxText, string caption, out string input)
        {
            var result = MessageBoxResult.None;
            input = string.Empty;
            var inpt = input ?? string.Empty;
            context.Send((a) =>
            {
                result = Dialogs.SelectString(PlayniteWindows.CurrentWindow, messageBoxText, caption, out inpt);
            }, null);

            input = inpt;
            return result;
        }

        public MessageBoxResult ShowMessage(string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult, MessageBoxOptions options)
        {
            return Invoke(() => PlayniteMessageBox.Show(messageBoxText, caption, button, icon, defaultResult, options));
        }

        public MessageBoxResult ShowMessage(string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult)
        {
            return Invoke(() => PlayniteMessageBox.Show(messageBoxText, caption, button, icon, defaultResult));
        }

        public MessageBoxResult ShowMessage(string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon)
        {
            return Invoke(() => PlayniteMessageBox.Show(messageBoxText, caption, button, icon));
        }

        public MessageBoxResult ShowMessage(string messageBoxText, string caption, MessageBoxButton button)
        {
            return Invoke(() => PlayniteMessageBox.Show(messageBoxText, caption, button));
        }

        public MessageBoxResult ShowMessage(string messageBoxText, string caption)
        {
            return Invoke(() => PlayniteMessageBox.Show(messageBoxText, caption));
        }

        public MessageBoxResult ShowMessage(string messageBoxText)
        {
            return Invoke(() => PlayniteMessageBox.Show(messageBoxText));
        }
    }

    public class PlayniteMessageBox
    {
        public static MessageBoxResult Show(string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult, MessageBoxOptions options)
        {
            return Show(PlayniteWindows.CurrentWindow, messageBoxText, caption, button, icon, defaultResult, options);
        }

        public static MessageBoxResult Show(string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult)
        {
            return Show(PlayniteWindows.CurrentWindow, messageBoxText, caption, button, icon, defaultResult);
        }

        public static MessageBoxResult Show(string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon)
        {
            return Show(PlayniteWindows.CurrentWindow, messageBoxText, caption, button, icon);
        }

        public static MessageBoxResult Show(string messageBoxText, string caption, MessageBoxButton button)
        {
            return Show(PlayniteWindows.CurrentWindow, messageBoxText, caption, button);
        }

        public static MessageBoxResult Show(string messageBoxText, string caption)
        {
            return Show(PlayniteWindows.CurrentWindow, messageBoxText, caption);
        }

        public static MessageBoxResult Show(string messageBoxText)
        {
            return Show(PlayniteWindows.CurrentWindow, messageBoxText);
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
