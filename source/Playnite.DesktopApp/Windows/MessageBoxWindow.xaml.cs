using Playnite.SDK;
using Playnite.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Playnite.Windows;

namespace Playnite.DesktopApp.Windows
{
    /// <summary>
    /// Interaction logic for MessageBoxWindow.xaml
    /// </summary>
    public partial class MessageBoxWindow : WindowBase
    {
        private MessageBoxResult result;
        private object resultCustom;

        private string text = string.Empty;
        public string Text
        {
            get => text;
            set
            {
                text = value;
                OnPropertyChanged(nameof(Text));
            }
        }

        private string caption = string.Empty;
        public string Caption
        {
            get => caption;
            set
            {
                caption = value;
                OnPropertyChanged(nameof(Caption));
            }
        }

        private bool showOKButton = false;
        public bool ShowOKButton
        {
            get => showOKButton;
            set
            {
                showOKButton = value;
                OnPropertyChanged(nameof(ShowOKButton));
            }
        }

        private bool showYesButton = false;
        public bool ShowYesButton
        {
            get => showYesButton;
            set
            {
                showYesButton = value;
                OnPropertyChanged(nameof(ShowYesButton));
            }
        }

        private bool showNoButton = false;
        public bool ShowNoButton
        {
            get => showNoButton;
            set
            {
                showNoButton = value;
                OnPropertyChanged(nameof(ShowNoButton));
            }
        }

        private bool showCancelButton = false;
        public bool ShowCancelButton
        {
            get => showCancelButton;
            set
            {
                showCancelButton = value;
                OnPropertyChanged(nameof(ShowCancelButton));
            }
        }

        private bool showInputField = false;
        public bool ShowInputField
        {
            get => showInputField;
            set
            {
                showInputField = value;
                OnPropertyChanged(nameof(ShowInputField));
            }
        }

        private string inputText = string.Empty;
        public string InputText
        {
            get => inputText;
            set
            {
                inputText = value;
                OnPropertyChanged(nameof(InputText));
            }
        }

        private MessageBoxImage displayIcon;
        public MessageBoxImage DisplayIcon
        {
            get => displayIcon;
            set
            {
                displayIcon = value;
                OnPropertyChanged(nameof(DisplayIcon));
            }
        }

        private bool isTextReadOnly = false;
        public bool IsTextReadOnly
        {
            get => isTextReadOnly;
            set
            {
                isTextReadOnly = value;
                OnPropertyChanged(nameof(IsTextReadOnly));
            }
        }

        public MessageBoxWindow() : base()
        {
            InitializeComponent();
        }

        public void ShowInputReadOnly(
            Window owner,
            string messageBoxText,
            string caption,
            string inputText)
        {
            if (owner == null || owner == this)
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }

            if (this != owner)
            {
                Owner = owner;
            }

            WindowManager.NotifyChildOwnershipChanges();
            TextInputText.Focus();
            SetStrings(messageBoxText, caption);
            ShowInputField = true;
            ShowOKButton = true;
            InputText = inputText ?? string.Empty;
            IsTextReadOnly = true;
            ShowDialog();
            WindowManager.NotifyChildOwnershipChanges();
        }

        public StringSelectionDialogResult ShowInput(
            Window owner,
            string messageBoxText,
            string caption,
            string defaultInput)
        {
            if (owner == null || owner == this)
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }

            if (this != owner)
            {
                Owner = owner;
            }

            WindowManager.NotifyChildOwnershipChanges();
            TextInputText.Focus();
            SetStrings(messageBoxText, caption);
            ShowInputField = true;
            ShowOKButton = true;
            ShowCancelButton = true;
            InputText = defaultInput ?? string.Empty;
            ShowDialog();
            WindowManager.NotifyChildOwnershipChanges();

            if (result == MessageBoxResult.Cancel)
            {
                return new StringSelectionDialogResult(false, InputText);
            }
            else
            {
                return new StringSelectionDialogResult(true, InputText);
            }
        }

        public MessageBoxResult Show(
            Window owner,
            string messageBoxText,
            string caption,
            MessageBoxButton button,
            MessageBoxImage icon,
            MessageBoxResult defaultResult,
            MessageBoxOptions options)
        {
            if (owner == null || owner == this)
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }

            if (this != owner)
            {
                Owner = owner;
            }

            WindowManager.NotifyChildOwnershipChanges();
            result = defaultResult;
            SetStrings(messageBoxText, caption);
            DisplayIcon = icon;

            switch (button)
            {
                case MessageBoxButton.OK:
                    ShowOKButton = true;
                    break;
                case MessageBoxButton.OKCancel:
                    ShowOKButton = true;
                    ShowCancelButton = true;
                    break;
                case MessageBoxButton.YesNoCancel:
                    ShowYesButton = true;
                    ShowNoButton = true;
                    ShowCancelButton = true;
                    break;
                case MessageBoxButton.YesNo:
                    ShowYesButton = true;
                    ShowNoButton = true;
                    break;
                default:
                    ShowOKButton = true;
                    break;
            }

            if (ShowOKButton)
            {
                ButtonOK.Focus();
            }

            ShowDialog();
            WindowManager.NotifyChildOwnershipChanges();
            return result;
        }

        public object ShowCustom(
            Window owner,
            string messageBoxText,
            string caption,
            MessageBoxImage icon,
            List<object> options,
            List<string> optionsTitles)
        {
            if (owner == null || owner == this)
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }

            if (this != owner)
            {
                Owner = owner;
            }

            WindowManager.NotifyChildOwnershipChanges();
            SetStrings(messageBoxText, caption);
            DisplayIcon = icon;

            ShowOKButton = false;
            ShowYesButton = false;
            ShowNoButton = false;
            ShowCancelButton = false;
            ShowInputField = false;

            for (int i = 0; i < options.Count; i++)
            {
                var option = options[i];
                var title = optionsTitles[i];

                var button = new Button();
                button.Content = title;
                button.Style = ResourceProvider.GetResource("BottomButton") as Style;
                button.Tag = option;
                button.Click += (s, __) =>
                {
                    resultCustom = (s as Button).Tag;
                    Close();
                };

                StackButtons.Children.Add(button);
            }

            ShowDialog();
            WindowManager.NotifyChildOwnershipChanges();
            return resultCustom;
        }

        private void SetStrings(string messageText, string messageCaption)
        {
            if (messageText?.StartsWith("LOC") == true)
            {
                Text = ResourceProvider.GetString(messageText);
            }
            else
            {
                Text = messageText;
            }

            if (messageCaption?.StartsWith("LOC") == true)
            {
                Caption = ResourceProvider.GetString(messageCaption);
            }
            else
            {
                Caption = messageCaption;
            }
        }

        private void ButtonOK_Click(object sender, RoutedEventArgs e)
        {
            result = MessageBoxResult.OK;
            Close();
        }

        private void ButtonYes_Click(object sender, RoutedEventArgs e)
        {
            result = MessageBoxResult.Yes;
            Close();
        }

        private void ButtonNo_Click(object sender, RoutedEventArgs e)
        {
            result = MessageBoxResult.No;
            Close();
        }

        private void ButtonCancel_Click(object sender, RoutedEventArgs e)
        {
            result = MessageBoxResult.Cancel;
            Close();
        }
    }
}
