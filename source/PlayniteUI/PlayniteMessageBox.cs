﻿using Playnite.SDK;
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

        public bool IsFullscreen
        {
            get; set;
        }

        public DialogsFactory()
        {
            context = SynchronizationContext.Current;
        }

        public DialogsFactory(bool fullscreen)
        {
            IsFullscreen = fullscreen;
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

        private void Invoke(Action action)
        {
            context.Send((a) =>
            {
                action();
            }, null);
        }

        public string SaveFile(string filter)
        {
            return Invoke(() => SystemDialogs.SaveFile(PlayniteWindows.CurrentWindow, filter));
        }

        public string SaveFile(string filter, bool promptOverwrite)
        {
            return Invoke(() => SystemDialogs.SaveFile(PlayniteWindows.CurrentWindow, filter, promptOverwrite));
        }

        public string SelectFile(string filter)
        {
            return Invoke(() => SystemDialogs.SelectFile(PlayniteWindows.CurrentWindow, filter));
        }

        public List<string> SelectFiles(string filter)
        {
            return Invoke(() => SystemDialogs.SelectFiles(PlayniteWindows.CurrentWindow, filter));
        }

        public string SelectFolder()
        {
            return Invoke(() => SystemDialogs.SelectFolder(PlayniteWindows.CurrentWindow));
        }

        public string SelectIconFile()
        {
            return Invoke(() => SystemDialogs.SelectIconFile(PlayniteWindows.CurrentWindow));
        }

        public string SelectImagefile()
        {
            return Invoke(() => SystemDialogs.SelectImageFile(PlayniteWindows.CurrentWindow));
        }

        public StringSelectionDialogResult SelectString(string messageBoxText, string caption, string defaultInput)
        {
            if (IsFullscreen)
            {
                return Invoke(() => PlayniteMessageBoxFullscreen.SelectString(PlayniteWindows.CurrentWindow, messageBoxText, caption, defaultInput));
            }
            else
            {
                return Invoke(() => PlayniteMessageBox.SelectString(PlayniteWindows.CurrentWindow, messageBoxText, caption, defaultInput));
            }
        }

        public MessageBoxResult ShowMessage(string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult, MessageBoxOptions options)
        {
            if (IsFullscreen)
            {
                return Invoke(() => PlayniteMessageBoxFullscreen.Show(messageBoxText, caption, button, icon, defaultResult, options));
            }
            else
            {
                return Invoke(() => PlayniteMessageBox.Show(messageBoxText, caption, button, icon, defaultResult, options));
            }
        }

        public MessageBoxResult ShowMessage(string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult)
        {
            if (IsFullscreen)
            {
                return Invoke(() => PlayniteMessageBoxFullscreen.Show(messageBoxText, caption, button, icon, defaultResult));
            }
            else
            {
                return Invoke(() => PlayniteMessageBox.Show(messageBoxText, caption, button, icon, defaultResult));
            }
        }

        public MessageBoxResult ShowMessage(string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon)
        {
            if (IsFullscreen)
            {
                return Invoke(() => PlayniteMessageBoxFullscreen.Show(messageBoxText, caption, button, icon));
            }
            else
            {
                return Invoke(() => PlayniteMessageBox.Show(messageBoxText, caption, button, icon));
            }
        }

        public MessageBoxResult ShowMessage(string messageBoxText, string caption, MessageBoxButton button)
        {
            if (IsFullscreen)
            {
                return Invoke(() => PlayniteMessageBoxFullscreen.Show(messageBoxText, caption, button));
            }
            else
            {
                return Invoke(() => PlayniteMessageBox.Show(messageBoxText, caption, button));
            }
        }

        public MessageBoxResult ShowMessage(string messageBoxText, string caption)
        {
            if (IsFullscreen)
            {
                return Invoke(() => PlayniteMessageBoxFullscreen.Show(messageBoxText, caption));
            }
            else
            {
                return Invoke(() => PlayniteMessageBox.Show(messageBoxText, caption));
            }
        }

        public MessageBoxResult ShowMessage(string messageBoxText)
        {
            if (IsFullscreen)
            {
                return Invoke(() => PlayniteMessageBoxFullscreen.Show(messageBoxText));
            }
            else
            {
                return Invoke(() => PlayniteMessageBox.Show(messageBoxText));
            }
        }

        public void ShowSelectableString(string messageBoxText, string caption, string inputText)
        {
            if (IsFullscreen)
            {
                Invoke(() => PlayniteMessageBoxFullscreen.ShowSelectableString(PlayniteWindows.CurrentWindow, messageBoxText, caption, inputText));
            }
            else
            {
                Invoke(() => PlayniteMessageBox.ShowSelectableString(PlayniteWindows.CurrentWindow, messageBoxText, caption, inputText));
            }
        }

        public MessageBoxResult ShowErrorMessage(string messageBoxText, string caption)
        {
            return ShowMessage(messageBoxText, caption, MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    public class PlayniteMessageBoxFullscreen
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
            return (new FullscreenMessageBoxWindow()).Show(owner, messageBoxText, caption, button, icon, defaultResult, options);
        }

        public static MessageBoxResult Show(Window owner, string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult)
        {
            return (new FullscreenMessageBoxWindow()).Show(owner, messageBoxText, caption, button, icon, defaultResult, MessageBoxOptions.None);
        }

        public static MessageBoxResult Show(Window owner, string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon)
        {
            return (new FullscreenMessageBoxWindow()).Show(owner, messageBoxText, caption, button, icon, MessageBoxResult.None, MessageBoxOptions.None);
        }

        public static MessageBoxResult Show(Window owner, string messageBoxText, string caption, MessageBoxButton button)
        {
            return (new FullscreenMessageBoxWindow()).Show(owner, messageBoxText, caption, button, MessageBoxImage.None, MessageBoxResult.None, MessageBoxOptions.None);
        }

        public static MessageBoxResult Show(Window owner, string messageBoxText, string caption)
        {
            return (new FullscreenMessageBoxWindow()).Show(owner, messageBoxText, caption, MessageBoxButton.OK, MessageBoxImage.None, MessageBoxResult.None, MessageBoxOptions.None);
        }

        public static MessageBoxResult Show(Window owner, string messageBoxText)
        {
            return (new FullscreenMessageBoxWindow()).Show(owner, messageBoxText, string.Empty, MessageBoxButton.OK, MessageBoxImage.None, MessageBoxResult.None, MessageBoxOptions.None);
        }

        public static StringSelectionDialogResult SelectString(Window owner, string messageBoxText, string caption, string defaultInput)
        {
            return (new FullscreenTextInputWindow()).ShowInput(owner, messageBoxText, caption, defaultInput);
        }

        public static void ShowSelectableString(Window owner, string messageBoxText, string caption, string inputText)
        {
            Show(owner, messageBoxText, caption, MessageBoxButton.OK, MessageBoxImage.None);
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

        public static StringSelectionDialogResult SelectString(Window owner, string messageBoxText, string caption, string defaultInput)
        {
            return (new MessageBoxWindow()).ShowInput(owner, messageBoxText, caption, defaultInput);
        }

        public static void ShowSelectableString(Window owner, string messageBoxText, string caption, string inputText)
        {
            (new MessageBoxWindow()).ShowInputReadOnly(owner, messageBoxText, caption, inputText);
        }
    }
}
