﻿using Playnite.Common;
using Playnite.Controls;
using Playnite.FullscreenApp.Windows;
using Playnite.SDK;
using Playnite.ViewModels;
using Playnite.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Playnite.FullscreenApp
{
    public class FullscreenDialogs : IDialogsFactory
    {
        private readonly SynchronizationContext context;

        public FullscreenDialogs()
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

        private void Invoke(Action action)
        {
            context.Send((a) =>
            {
                action();
            }, null);
        }

        public string SaveFile(string filter)
        {
            return Invoke(() => SystemDialogs.SaveFile(WindowManager.CurrentWindow, filter));
        }

        public string SaveFile(string filter, bool promptOverwrite)
        {
            return Invoke(() => SystemDialogs.SaveFile(WindowManager.CurrentWindow, filter, promptOverwrite));
        }

        public string SelectFile(string filter)
        {
            return Invoke(() => SystemDialogs.SelectFile(WindowManager.CurrentWindow, filter));
        }

        public List<string> SelectFiles(string filter)
        {
            return Invoke(() => SystemDialogs.SelectFiles(WindowManager.CurrentWindow, filter));
        }

        public string SelectFolder()
        {
            return Invoke(() => SystemDialogs.SelectFolder(WindowManager.CurrentWindow));
        }

        public string SelectIconFile()
        {
            return Invoke(() => SystemDialogs.SelectIconFile(WindowManager.CurrentWindow));
        }

        public string SelectImagefile()
        {
            return Invoke(() => SystemDialogs.SelectImageFile(WindowManager.CurrentWindow));
        }

        public StringSelectionDialogResult SelectString(string messageBoxText, string caption, string defaultInput)
        {
            return Invoke(() => new TextInputWindow().ShowInput(WindowManager.CurrentWindow, messageBoxText, caption, defaultInput));
        }

        public StringSelectionDialogResult SelectString(string messageBoxText, string caption, string defaultInput, List<MessageBoxToggle> options)
        {
            return Invoke(() => new TextInputWindow().ShowInput(WindowManager.CurrentWindow, messageBoxText, caption, defaultInput, options));
        }

        public MessageBoxResult ShowMessage(string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult, MessageBoxOptions options)
        {
            return Invoke(() => new MessageBoxWindow().Show(WindowManager.CurrentWindow, messageBoxText, caption, button, icon, defaultResult, options));
        }

        public MessageBoxResult ShowMessage(string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult)
        {
            return ShowMessage(messageBoxText, caption, button, icon, defaultResult, MessageBoxOptions.None);
        }

        public MessageBoxResult ShowMessage(string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon)
        {
            return ShowMessage(messageBoxText, caption, button, icon, MessageBoxResult.None, MessageBoxOptions.None);
        }

        public MessageBoxResult ShowMessage(string messageBoxText, string caption, MessageBoxButton button)
        {
            return ShowMessage(messageBoxText, caption, button, MessageBoxImage.None, MessageBoxResult.None, MessageBoxOptions.None);
        }

        public MessageBoxResult ShowMessage(string messageBoxText, string caption)
        {
            return ShowMessage(messageBoxText, caption, MessageBoxButton.OK, MessageBoxImage.None, MessageBoxResult.None, MessageBoxOptions.None);
        }

        public MessageBoxResult ShowMessage(string messageBoxText)
        {
            return ShowMessage(messageBoxText, string.Empty, MessageBoxButton.OK, MessageBoxImage.None, MessageBoxResult.None, MessageBoxOptions.None);
        }

        public void ShowSelectableString(string messageBoxText, string caption, string inputText)
        {
            ShowMessage(messageBoxText + Environment.NewLine + inputText, caption);
        }

        public MessageBoxResult ShowErrorMessage(string messageBoxText, string caption)
        {
            return ShowMessage(messageBoxText, caption, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public MessageBoxResult ShowErrorMessage(string messageBoxText)
        {
            return ShowMessage(messageBoxText, string.Empty, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public MessageBoxOption ShowMessage(string messageBoxText, string caption, MessageBoxImage icon, List<MessageBoxOption> options)
        {
            return Invoke(() => new MessageBoxWindow().ShowCustom(WindowManager.CurrentWindow, messageBoxText, caption, icon, options));
        }

        public ImageFileOption ChooseImageFile(List<ImageFileOption> files, string caption = null, double itemWidth = 240, double itemHeight = 180)
        {
            throw new NotImplementedException();
        }

        public GenericItemOption ChooseItemWithSearch(List<GenericItemOption> items, Func<string, List<GenericItemOption>> searchFunction, string defaultSearch = null, string caption = null)
        {
            throw new NotImplementedException();
        }

        public GlobalProgressResult ActivateGlobalProgress(Action<GlobalProgressActionArgs> progresAction, GlobalProgressOptions progressArgs)
        {
            return Invoke(() => GlobalProgress.ActivateProgress(progresAction, progressArgs));
        }

        public Window CreateWindow(WindowCreationOptions options)
        {
            throw new NotImplementedException();
        }

        public Window GetCurrentAppWindow()
        {
            return WindowManager.CurrentWindow;
        }
    }
}
