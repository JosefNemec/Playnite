using PlayniteUI.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PlayniteUI
{
    public interface IDialogsFactory
    {
        MessageBoxResult ShowMessage(string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult, MessageBoxOptions options);
        MessageBoxResult ShowMessage(string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult);
        MessageBoxResult ShowMessage(string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon);
        MessageBoxResult ShowMessage(string messageBoxText, string caption, MessageBoxButton button);
        MessageBoxResult ShowMessage(string messageBoxText, string caption);
        MessageBoxResult ShowMessage(string messageBoxText);
        string SelectFolder();
        string SelectFile(string filter);
        string SelectIconFile();
        string SelectImagefile();
        string SaveFile(string filter);
        string SaveFile(string filter, bool promptOverwrite);
        MessageBoxResult SelectString(string messageBoxText, string caption, out string input);
    }

    public class DialogsFactory : IDialogsFactory
    {
        public string SaveFile(string filter)
        {
            return Dialogs.SaveFile(PlayniteWindows.CurrentWindow, filter);
        }

        public string SaveFile(string filter, bool promptOverwrite)
        {
            return Dialogs.SaveFile(PlayniteWindows.CurrentWindow, filter, promptOverwrite);
        }

        public string SelectFile(string filter)
        {
            return Dialogs.SelectFile(PlayniteWindows.CurrentWindow, filter);
        }

        public string SelectFolder()
        {
            return Dialogs.SelectFolder(PlayniteWindows.CurrentWindow);
        }

        public string SelectIconFile()
        {
            return Dialogs.SelectIconFile(PlayniteWindows.CurrentWindow);
        }

        public string SelectImagefile()
        {
            return Dialogs.SelectImageFile(PlayniteWindows.CurrentWindow);
        }

        public MessageBoxResult SelectString(string messageBoxText, string caption, out string input)
        {
            return Dialogs.SelectString(PlayniteWindows.CurrentWindow, messageBoxText, caption, out input);
        }

        public MessageBoxResult ShowMessage(string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult, MessageBoxOptions options)
        {
            return PlayniteMessageBox.Show(messageBoxText, caption, button, icon, defaultResult, options);
        }

        public MessageBoxResult ShowMessage(string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult)
        {
            return PlayniteMessageBox.Show(messageBoxText, caption, button, icon, defaultResult);
        }

        public MessageBoxResult ShowMessage(string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon)
        {
            return PlayniteMessageBox.Show(messageBoxText, caption, button, icon);
        }

        public MessageBoxResult ShowMessage(string messageBoxText, string caption, MessageBoxButton button)
        {
            return PlayniteMessageBox.Show(messageBoxText, caption, button);
        }

        public MessageBoxResult ShowMessage(string messageBoxText, string caption)
        {
            return PlayniteMessageBox.Show(messageBoxText, caption);
        }

        public MessageBoxResult ShowMessage(string messageBoxText)
        {
            return PlayniteMessageBox.Show(messageBoxText);
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
