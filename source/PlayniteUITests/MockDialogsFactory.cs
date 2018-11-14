using Playnite.SDK;
using PlayniteUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace PlayniteUITests
{
    class MockDialogsFactory : IDialogsFactory
    {
        public string SaveFile(string filter)
        {
            return string.Empty;
        }

        public string SaveFile(string filter, bool promptOverwrite)
        {
            return string.Empty;
        }

        public string SelectFile(string filter)
        {
            return string.Empty;
        }

        public List<string> SelectFiles(string filter)
        {
            return null;
        }

        public string SelectFolder()
        {
            return string.Empty;
        }

        public string SelectIconFile()
        {
            return string.Empty;
        }

        public string SelectImagefile()
        {
            return string.Empty;
        }

        public StringSelectionDialogResult SelectString(string messageBoxText, string caption, string defaultInput)
        {
            return new StringSelectionDialogResult(false, string.Empty);
        }

        public MessageBoxResult ShowErrorMessage(string messageBoxText, string caption)
        {
            return MessageBoxResult.None;
        }

        public MessageBoxResult ShowMessage(string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult, MessageBoxOptions options)
        {
            return MessageBoxResult.None;
        }

        public MessageBoxResult ShowMessage(string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult)
        {
            return MessageBoxResult.None;
        }

        public MessageBoxResult ShowMessage(string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon)
        {
            return MessageBoxResult.None;
        }

        public MessageBoxResult ShowMessage(string messageBoxText, string caption, MessageBoxButton button)
        {
            return MessageBoxResult.None;
        }

        public MessageBoxResult ShowMessage(string messageBoxText, string caption)
        {
            return MessageBoxResult.None;
        }

        public MessageBoxResult ShowMessage(string messageBoxText)
        {
            return MessageBoxResult.None;
        }

        public void ShowSelectableString(string messageBoxText, string caption, string defaultInput)
        {
            throw new NotImplementedException();
        }
    }
}
