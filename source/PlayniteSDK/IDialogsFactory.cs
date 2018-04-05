using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Playnite.SDK
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
        List<string> SelectFiles(string filter);
        string SelectIconFile();
        string SelectImagefile();
        string SaveFile(string filter);
        string SaveFile(string filter, bool promptOverwrite);
        MessageBoxResult SelectString(string messageBoxText, string caption, out string input);
    }
}
