using Playnite.Controls;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
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

namespace Playnite.FullscreenApp.Windows
{
    public partial class MessageBoxWindow : WindowBase
    {
        private MessageBoxResult result;

        private string text = string.Empty;
        public string Text
        {
            get => text;
            set
            {
                text = value;
                OnPropertyChanged();
            }
        }

        private string caption = string.Empty;
        public string Caption
        {
            get => caption;
            set
            {
                caption = value;
                OnPropertyChanged();
            }
        }

        private bool showOKButton = false;
        public bool ShowOKButton
        {
            get => showOKButton;
            set
            {
                showOKButton = value;
                OnPropertyChanged();
            }
        }

        private bool showYesButton = false;
        public bool ShowYesButton
        {
            get => showYesButton;
            set
            {
                showYesButton = value;
                OnPropertyChanged();
            }
        }

        private bool showNoButton = false;
        public bool ShowNoButton
        {
            get => showNoButton;
            set
            {
                showNoButton = value;
                OnPropertyChanged();
            }
        }

        private bool showCancelButton = false;
        public bool ShowCancelButton
        {
            get => showCancelButton;
            set
            {
                showCancelButton = value;
                OnPropertyChanged();
            }
        }

        private bool showInputField = false;
        public bool ShowInputField
        {
            get => showInputField;
            set
            {
                showInputField = value;
                OnPropertyChanged();
            }
        }

        private string inputText = string.Empty;
        public string InputText
        {
            get => inputText;
            set
            {
                inputText = value;
                OnPropertyChanged();
            }
        }

        private MessageBoxImage displayIcon;
        public MessageBoxImage DisplayIcon
        {
            get => displayIcon;
            set
            {
                displayIcon = value;
                OnPropertyChanged();
            }
        }

        public MessageBoxWindow() : base()
        {
            InitializeComponent();
        }

        public MessageBoxResult Show(Window owner, string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult, MessageBoxOptions options)
        {
            if (owner == null)
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }

            if (this != owner)
            {
                Owner = owner;
            }

            Height = owner.Height;
            Width = owner.Width;
            result = defaultResult;
            SetStrings(messageBoxText, caption);
            DisplayIcon = icon;

            switch (button)
            {
                case MessageBoxButton.OK:
                    ButtonOK.Focus();
                    ShowOKButton = true;
                    break;
                case MessageBoxButton.OKCancel:
                    ButtonOK.Focus();
                    ShowOKButton = true;
                    ShowCancelButton = true;
                    break;
                case MessageBoxButton.YesNoCancel:
                    ButtonYes.Focus();
                    ShowYesButton = true;
                    ShowNoButton = true;
                    ShowCancelButton = true;
                    break;
                case MessageBoxButton.YesNo:
                    ButtonYes.Focus();
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
            return result;
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
