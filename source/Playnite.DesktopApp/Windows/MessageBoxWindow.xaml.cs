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
        private MessageBoxOption resultCustom;

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

            TextInputText.Focus();
            SetStrings(messageBoxText, caption);
            ShowInputField = true;
            ShowOKButton = true;
            ButtonOK.IsDefault = true;
            ButtonOK.Focus();
            InputText = inputText ?? string.Empty;
            IsTextReadOnly = true;
            ShowDialog();
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

            TextInputText.Focus();
            SetStrings(messageBoxText, caption);
            ShowInputField = true;
            ShowOKButton = true;
            ButtonOK.IsDefault = true;
            ShowCancelButton = true;
            ButtonCancel.IsCancel = true;
            InputText = defaultInput ?? string.Empty;
            ShowDialog();

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

            result = defaultResult;
            SetStrings(messageBoxText, caption);
            DisplayIcon = icon;

            switch (button)
            {
                case MessageBoxButton.OK:
                    ShowOKButton = true;
                    ButtonOK.IsDefault = true;
                    ButtonOK.Focus();
                    break;
                case MessageBoxButton.OKCancel:
                    ShowOKButton = true;
                    ButtonOK.IsDefault = true;
                    ButtonOK.Focus();
                    ShowCancelButton = true;
                    ButtonCancel.IsCancel = true;
                    break;
                case MessageBoxButton.YesNoCancel:
                    ShowYesButton = true;
                    ButtonYes.Focus();
                    ButtonYes.IsDefault = true;
                    ShowNoButton = true;
                    ShowCancelButton = true;
                    ButtonCancel.IsCancel = true;
                    break;
                case MessageBoxButton.YesNo:
                    ShowYesButton = true;
                    ButtonYes.Focus();
                    ButtonYes.IsDefault = true;
                    ShowNoButton = true;
                    ButtonNo.IsCancel = true;
                    break;
                default:
                    ShowOKButton = true;
                    ButtonOK.Focus();
                    ButtonOK.IsDefault = true;
                    break;
            }

            ShowDialog();
            return result;
        }

        public MessageBoxOption ShowCustom(
            Window owner,
            string messageBoxText,
            string caption,
            MessageBoxImage icon,
            List<MessageBoxOption> options)
        {
            if (owner == null || owner == this)
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }

            if (this != owner)
            {
                Owner = owner;
            }

            SetStrings(messageBoxText, caption);
            DisplayIcon = icon;

            ShowOKButton = false;
            ShowYesButton = false;
            ShowNoButton = false;
            ShowCancelButton = false;
            ShowInputField = false;

            foreach (var option in options)
            {
                var title = option.Title;
                var button = new Button();
                button.Content = title.StartsWith("LOC") ? ResourceProvider.GetString(title) : title;
                button.Style = ResourceProvider.GetResource("BottomButton") as Style;
                button.Tag = option;
                button.IsDefault = option.IsDefault;
                button.IsCancel = option.IsCancel;
                button.Click += (s, __) =>
                {
                    resultCustom = (s as Button).Tag as MessageBoxOption;
                    Close();
                };

                StackButtons.Children.Add(button);
                if (option.IsDefault)
                {
                    button.Focus();
                }
            }

            ShowDialog();
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
