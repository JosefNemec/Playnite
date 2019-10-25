using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Playnite.Tests
{
    public class MockDialogsFactory : IDialogsFactory
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
            return new List<string>();
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

        public System.Windows.MessageBoxResult ShowErrorMessage(string messageBoxText, string caption)
        {
            return MessageBoxResult.None;
        }

        public System.Windows.MessageBoxResult ShowMessage(string messageBoxText, string caption, System.Windows.MessageBoxButton button, System.Windows.MessageBoxImage icon)
        {
            return MessageBoxResult.None;
        }

        public System.Windows.MessageBoxResult ShowMessage(string messageBoxText, string caption, System.Windows.MessageBoxButton button)
        {
            return MessageBoxResult.None;
        }

        public System.Windows.MessageBoxResult ShowMessage(string messageBoxText, string caption)
        {
            return MessageBoxResult.None;
        }

        public System.Windows.MessageBoxResult ShowMessage(string messageBoxText)
        {
            return MessageBoxResult.None;
        }

        public void ShowSelectableString(string messageBoxText, string caption, string defaultInput)
        {
            
        }

        public ImageFileOption ChooseImageFile(List<ImageFileOption> files, string caption = null, double itemWidth = 240, double itemHeight = 180)
        {
            return null;
        }

        public GenericItemOption ChooseItemWithSearch(List<GenericItemOption> items, Func<string, List<GenericItemOption>> searchFunction, string defaultSearch = null, string caption = null)
        {
            return null;
        }
    }
}
