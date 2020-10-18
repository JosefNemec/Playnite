using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace Playnite.SDK
{
    /// <summary>
    /// Represents message box response options.
    /// </summary>
    public class MessageBoxOption
    {
        /// <summary>
        /// Gets or sets title of response option.
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Gets or sets value indicating whether this is default option.
        /// </summary>
        public bool IsDefault { get; set; }

        /// <summary>
        /// Gets or sets value indicating whether this is option to cancel the request.
        /// </summary>
        public bool IsCancel { get; set; }

        /// <summary>
        /// Creates new instance of <see cref="MessageBoxOption"/>.
        /// </summary>
        /// <param name="title"></param>
        /// <param name="isDefault"></param>
        /// <param name="isCancel"></param>
        public MessageBoxOption(string title, bool isDefault = false, bool isCancel = false)
        {
            Title = title;
            IsDefault = isDefault;
            IsCancel = isCancel;
        }
    }

    /// <summary>
    /// Represents arguments for global progress action.
    /// </summary>
    public class GlobalProgressActionArgs : ObservableObject
    {
        /// <summary>
        /// Gets synchronization context of main thread.
        /// </summary>
        public SynchronizationContext MainContext { get; }

        /// <summary>
        /// Gets dispatcher for main UI thread.
        /// </summary>
        public Dispatcher MainDispatcher { get; }

        /// <summary>
        /// Gets cancelation token source.
        /// </summary>
        public CancellationTokenSource CancelToken { get; }

        private double progressMaxValue = 0;
        /// <summary>
        /// Gets or sets maximum value represented on progress track.
        /// </summary>
        public double ProgressMaxValue
        {
            get => progressMaxValue;
            set
            {
                progressMaxValue = value;
                MainDispatcher?.Invoke(() => OnPropertyChanged(), DispatcherPriority.Send);
            }
        }

        private double currentProgressValue = 0;
        /// <summary>
        /// Gets or sets currect value represented on progress track.
        /// </summary>
        public double CurrentProgressValue
        {
            get => currentProgressValue;
            set
            {
                currentProgressValue = value;
                MainDispatcher?.Invoke(() => OnPropertyChanged(), DispatcherPriority.Send);
            }
        }

        private string text;
        /// <summary>
        /// Gets or sets progress text.
        /// </summary>
        public string Text
        {
            get => text;
            set
            {
                text = value;
                MainDispatcher?.Invoke(() => OnPropertyChanged(), DispatcherPriority.Send);
            }
        }

        /// <summary>
        /// Creates new instance of <see cref="GlobalProgressActionArgs"/>.
        /// </summary>
        /// <param name="mainContext"></param>
        /// <param name="mainDispatcher"></param>
        /// <param name="cancelToken"></param>
        public GlobalProgressActionArgs(SynchronizationContext mainContext, Dispatcher mainDispatcher, CancellationTokenSource cancelToken)
        {
            MainContext = mainContext;
            MainDispatcher = mainDispatcher;
            CancelToken = cancelToken;
        }
    }

    /// <summary>
    /// Represents result of global progress dialog.
    /// </summary>
    public class GlobalProgressResult
    {
        /// <summary>
        /// Gets failure exception record.
        /// </summary>
        public Exception Error { get; }

        /// <summary>
        /// Gets execution result.
        /// </summary>
        public bool? Result { get; }

        /// <summary>
        /// Gets value indicating whether the action was canceled by user.
        /// </summary>
        public bool Canceled { get; }

        /// <summary>
        /// Creates new instance of <see cref="GlobalProgressResult"/>.
        /// </summary>
        /// <param name="result"></param>
        /// <param name="canceled"></param>
        /// <param name="error"></param>
        public GlobalProgressResult(bool? result, bool canceled, Exception error)
        {
            Result = result;
            Error = error;
            Canceled = canceled;
        }
    }

    /// <summary>
    /// Represents option for global progress dialog.
    /// </summary>
    public class GlobalProgressOptions
    {
        /// <summary>
        /// Gets or sets progress text.
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Gets or sets value indicating whether the progress can be canceled.
        /// </summary>
        public bool Cancelable { get; set; }

        /// <summary>
        /// Gets or sets value indicating whether the progress is indeterminated.
        /// </summary>
        public bool IsIndeterminate { get; set; } = true;

        /// <summary>
        /// Creates new instance of <see cref="GlobalProgressOptions"/>.
        /// </summary>
        /// <param name="text"></param>
        public GlobalProgressOptions(string text)
        {
            Text = text;
        }

        /// <summary>
        /// Creates new instance of <see cref="GlobalProgressOptions"/>.
        /// </summary>
        /// <param name="text"></param>
        /// <param name="cancelable"></param>
        public GlobalProgressOptions(string text, bool cancelable) : this(text)
        {
            Cancelable = cancelable;
        }
    }

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
    /// Represents option for new window creation.
    /// </summary>
    public class WindowCreationOptions
    {
        /// <summary>
        /// Gets or sets value indicating whether the minimize button should be shown.
        /// </summary>
        public bool ShowMinimizeButton { get; set; } = true;

        /// <summary>
        /// Gets or sets value indicating whether the maximize button should be shown.
        /// </summary>
        public bool ShowMaximizeButton { get; set; } = true;

        /// <summary>
        /// Gets or sets value indicating whether the close button should be shown.
        /// </summary>
        public bool ShowCloseButton { get; set; } = true;
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
        /// <param name="button">Available response button.</param>
        /// <param name="icon">Dialog icon.</param>
        /// <returns>Selected dialog response.</returns>
        MessageBoxResult ShowMessage(string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon);

        /// <summary>
        /// Displays dialog window with text message.
        /// </summary>
        /// <param name="messageBoxText">Dialog message text.</param>
        /// <param name="caption">Dialog window caption.</param>
        /// <param name="button">Available response button.</param>
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
        /// Displays dialog window custom response options.
        /// </summary>
        /// <param name="messageBoxText">Dialog message text.</param>
        /// <param name="caption">Dialog window caption.</param>
        /// <param name="icon">Dialog icon.</param>
        /// <param name="options">Response options.</param>
        /// <returns>Selected dialog option.</returns>
        MessageBoxOption ShowMessage(string messageBoxText, string caption, MessageBoxImage icon, List<MessageBoxOption> options);

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

        /// <summary>
        /// Activates progress dialog blocking app interaction until progress is finished or canceled.
        /// </summary>
        /// <param name="progresAction">Action to be executed.</param>
        /// <param name="progressOptions">Options for progress dialog.</param>
        /// <returns>Status of the action execution.</returns>
        GlobalProgressResult ActivateGlobalProgress(Action<GlobalProgressActionArgs> progresAction, GlobalProgressOptions progressOptions);

        /// <summary>
        /// Creates new window with Playnite's default styling applied.
        /// </summary>
        /// <param name="options">Custom window options.</param>
        /// <returns>New window instance.</returns>
        Window CreateWindow(WindowCreationOptions options);

        /// <summary>
        /// Gets a window which currently in use an active.
        /// </summary>
        /// <returns>Window object.</returns>
        Window GetCurrentAppWindow();
    }
}
