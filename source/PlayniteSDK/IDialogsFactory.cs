using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Playnite.SDK
{
    /// <summary>
    /// Represents item for image selection dialog.
    /// </summary>
    public class ImageFileOption : GenericItemOption
    {
        /// <summary>
        /// Gets or sets image path or URL.
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// Creates new instance of <see cref="ImageFileOption"/>.
        /// </summary>
        public ImageFileOption() : base()
        {
        }

        /// <summary>
        /// Creates new instance of <see cref="ImageFileOption"/>.
        /// </summary>
        /// <param name="path"></param>
        public ImageFileOption(string path)
        {
            Path = path;
        }
    }

    /// <summary>
    /// Represents item for item selection dialogs.
    /// </summary>
    public class GenericItemOption
    {
        /// <summary>
        /// Gets or sets game name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets search result's description.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Creates new instance of <see cref="GenericItemOption"/>.
        /// </summary>
        public GenericItemOption()
        {
        }

        /// <summary>
        /// Creates new instance of <see cref="GenericItemOption"/>.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="description"></param>
        public GenericItemOption(string name, string description)
        {
            Name = name;
            Description = description;
        }
    }

    /// <summary>
    /// Refresents result of selection string dialog operation.
    /// </summary>
    public class StringSelectionDialogResult
    {
        /// <summary>
        /// Gets or sets dialog result. True if user confirmed selected otherwise false.
        /// </summary>
        public bool Result
        {
            get; set;
        }

        /// <summary>
        /// Gets or sets string selected by user.
        /// </summary>
        public string SelectedString
        {
            get; set;
        }

        /// <summary>
        /// Creates new instance of StringSelectionDialogResult.
        /// </summary>
        /// <param name="result">Dialog result.</param>
        /// <param name="selectedString">Selected string.</param>
        public StringSelectionDialogResult(bool result, string selectedString)
        {
            Result = result;
            SelectedString = selectedString;
        }
    }

    /// <summary>
    /// Describes object providing methods for dialog based actions.
    /// </summary>
    public interface IDialogsFactory
    {
        /// <summary>
        /// Displays errod dialog window with text message.
        /// </summary>
        /// <param name="messageBoxText">Dialog message text.</param>
        /// <param name="caption">Dialog window caption.</param>
        /// <returns></returns>
        MessageBoxResult ShowErrorMessage(string messageBoxText, string caption);

        /// <summary>
        /// Displays dialog window with text message.
        /// </summary>
        /// <param name="messageBoxText">Dialog message text.</param>
        /// <param name="caption">Dialog window caption.</param>
        /// <param name="button">Available respose button.</param>
        /// <param name="icon">Dialog icon.</param>
        /// <returns>Selected dialog response.</returns>
        MessageBoxResult ShowMessage(string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon);

        /// <summary>
        /// Displays dialog window with text message.
        /// </summary>
        /// <param name="messageBoxText">Dialog message text.</param>
        /// <param name="caption">Dialog window caption.</param>
        /// <param name="button">Available respose button.</param>
        /// <returns>Selected dialog response.</returns>
        MessageBoxResult ShowMessage(string messageBoxText, string caption, MessageBoxButton button);

        /// <summary>
        /// Displays dialog window with text message.
        /// </summary>
        /// <param name="messageBoxText">Dialog message text.</param>
        /// <param name="caption">Dialog window caption.</param>
        /// <returns>Selected dialog response.</returns>
        MessageBoxResult ShowMessage(string messageBoxText, string caption);

        /// <summary>
        /// Displays dialog window with text message.
        /// </summary>
        /// <param name="messageBoxText">Dialog message text.</param>
        /// <returns>Selected dialog response.</returns>
        MessageBoxResult ShowMessage(string messageBoxText);

        /// <summary>
        /// Displays system dialog for folder selection.
        /// </summary>
        /// <returns>Selected folder path or empty string if user cancels the dialog.</returns>
        string SelectFolder();

        /// <summary>
        /// Displays system open file dialog.
        /// </summary>
        /// <param name="filter">File filter, for example "ZIP Archive|*.zip"</param>
        /// <returns>Selected file path or empty string if user cancels the dialog.</returns>
        string SelectFile(string filter);

        /// <summary>
        /// Displays system open file dialog allowing to select multiple files.
        /// </summary>
        /// <param name="filter">File filter, for example "ZIP Archive|*.zip"</param>
        /// <returns>List of paths or null if user cancels the dialog.</returns>
        List<string> SelectFiles(string filter);

        /// <summary>
        /// Displays file open dialog with file filter set to show only image files used for icons.
        /// </summary>
        /// <returns>Selected icon path or empty string if user cancels the dialog.</returns>
        string SelectIconFile();

        /// <summary>
        /// Displays file open dialog with file filter set to show only image files.
        /// </summary>
        /// <returns>Selected image path or empty string if user cancels the dialog.</returns>
        string SelectImagefile();

        /// <summary>
        /// Displays system file save dialog.
        /// </summary>
        /// <param name="filter">File filter, for example "ZIP Archive|*.zip"</param>
        /// <returns>Selected file path or empty string if user cancels the dialog.</returns>
        string SaveFile(string filter);

        /// <summary>
        /// Displays system file save dialog.
        /// </summary>
        /// <param name="filter">File filter, for example "ZIP Archive|*.zip"</param>
        /// <param name="promptOverwrite">Indicates whether to ask user for file overrite if selected path exists.</param>
        /// <returns>Selected file path or empty string if user cancels the dialog.</returns>
        string SaveFile(string filter, bool promptOverwrite);

        /// <summary>
        /// Displays dialog asking user to input text string.
        /// </summary>
        /// <param name="messageBoxText">Dialog message text.</param>
        /// <param name="caption">Dialog window caption.</param>
        /// <param name="defaultInput">Default string presented in input field.</param>
        /// <returns>Selection result.</returns>
        StringSelectionDialogResult SelectString(string messageBoxText, string caption, string defaultInput);

        /// <summary>
        /// Displays dialog with textbox allowing to select/copy text.
        /// </summary>
        /// <param name="messageBoxText">Dialog message text.</param>
        /// <param name="caption">Dialog window caption.</param>
        /// <param name="defaultInput">String added into selectable field.</param>
        /// <returns>Selection result.</returns>
        void ShowSelectableString(string messageBoxText, string caption, string defaultInput);

        /// <summary>
        /// Displays dialog with an option to choose single image.
        /// </summary>
        /// <param name="files"></param>
        /// <param name="caption"></param>
        /// <param name="itemWidth"></param>
        /// <param name="itemHeight"></param>
        /// <returns>Null if dialog was canceled otherwise selected <see cref="ImageFileOption"/> object.</returns>
        ImageFileOption ChooseImageFile(List<ImageFileOption> files, string caption = null, double itemWidth = 240, double itemHeight = 180);

        /// <summary>
        /// Displays dialog with an option to choose single item and option to search for different items.
        /// </summary>
        /// <param name="items"></param>
        /// <param name="searchFunction"></param>
        /// <param name="defaultSearch"></param>
        /// <param name="caption"></param>
        /// <returns>Null if dialog was canceled otherwise selected <see cref="GenericItemOption"/> object.</returns>
        GenericItemOption ChooseItemWithSearch(List<GenericItemOption> items, Func<string, List<GenericItemOption>> searchFunction, string defaultSearch = null, string caption = null);
    }
}
