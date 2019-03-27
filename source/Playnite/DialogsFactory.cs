using Playnite.Common;
using Playnite.SDK;
using Playnite.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace Playnite
{
    public class Dialogs
    {
        public static IDialogsFactory DialogsHandler { get; private set; }

        public static void SetHandler(IDialogsFactory factory)
        {
            DialogsHandler = factory;
        }

        public static MessageBoxResult ShowErrorMessage(string messageBoxText, string caption)
        {
            return DialogsHandler.ShowErrorMessage(messageBoxText, caption);
        }

        public static MessageBoxResult ShowMessage(string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon)
        {
            return DialogsHandler.ShowMessage(messageBoxText, caption, button, icon);
        }    

        public static MessageBoxResult ShowMessage(string messageBoxText, string caption, MessageBoxButton button)
        {
            return DialogsHandler.ShowMessage(messageBoxText, caption, button);
        }

        public static MessageBoxResult ShowMessage(string messageBoxText, string caption)
        {
            return DialogsHandler.ShowMessage(messageBoxText, caption);
        }

        public static MessageBoxResult ShowMessage(string messageBoxText)
        {
            return DialogsHandler.ShowMessage(messageBoxText);
        }

        public static string SelectFolder()
        {
            return DialogsHandler.SelectFolder();
        }

        public static string SelectFile(string filter)
        {
            return DialogsHandler.SelectFile(filter);
        }

        public static List<string> SelectFiles(string filter)
        {
            return DialogsHandler.SelectFiles(filter);
        }

        public static string SelectIconFile()
        {
            return DialogsHandler.SelectIconFile();
        }

        public static string SelectImagefile()
        {
            return DialogsHandler.SelectImagefile();
        }

        public static string SaveFile(string filter)
        {
            return DialogsHandler.SaveFile(filter);
        }

        public static string SaveFile(string filter, bool promptOverwrite)
        {
            return DialogsHandler.SaveFile(filter, promptOverwrite);
        }

        public static StringSelectionDialogResult SelectString(string messageBoxText, string caption, string defaultInput)
        {
            return DialogsHandler.SelectString(messageBoxText, caption, defaultInput);
        }

        public static void ShowSelectableString(string messageBoxText, string caption, string defaultInput)
        {
            DialogsHandler.ShowSelectableString(messageBoxText, caption, defaultInput);
        }
    }
}
