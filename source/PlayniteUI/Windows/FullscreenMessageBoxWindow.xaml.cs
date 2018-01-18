using PlayniteUI.Controls;
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

namespace PlayniteUI.Windows
{
    public partial class FullscreenMessageBoxWindow : WindowBase, INotifyPropertyChanged
    {
        private MessageBoxResult result;

        public event PropertyChangedEventHandler PropertyChanged;

        private string text = string.Empty;
        public string Text
        {
            get => text;
            set
            {
                text = value;
                OnPropertyChanged("Text");
            }
        }

        private string caption = string.Empty;
        public string Caption
        {
            get => caption;
            set
            {
                caption = value;
                OnPropertyChanged("Caption");
            }
        }

        private bool showOKButton = false;
        public bool ShowOKButton
        {
            get => showOKButton;
            set
            {
                showOKButton = value;
                OnPropertyChanged("ShowOKButton");
            }
        }

        private bool showYesButton = false;
        public bool ShowYesButton
        {
            get => showYesButton;
            set
            {
                showYesButton = value;
                OnPropertyChanged("ShowYesButton");
            }
        }

        private bool showNoButton = false;
        public bool ShowNoButton
        {
            get => showNoButton;
            set
            {
                showNoButton = value;
                OnPropertyChanged("ShowNoButton");
            }
        }

        private bool showCancelButton = false;
        public bool ShowCancelButton
        {
            get => showCancelButton;
            set
            {
                showCancelButton = value;
                OnPropertyChanged("ShowCancelButton");
            }
        }


        private bool showInputField = false;
        public bool ShowInputField
        {
            get => showInputField;
            set
            {
                showInputField = value;
                OnPropertyChanged("ShowInputField");
            }
        }

        private string inputText = string.Empty;
        public string InputText
        {
            get => inputText;
            set
            {
                inputText = value;
                OnPropertyChanged("InputText");
            }
        }

        private MessageBoxImage displayIcon;
        public MessageBoxImage DisplayIcon
        {
            get => displayIcon;
            set
            {
                displayIcon = value;
                OnPropertyChanged("DisplayIcon");
            }
        }

        public FullscreenMessageBoxWindow()
        {
            InitializeComponent();
        }

        public void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public MessageBoxResult Show(Window owner, string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult, MessageBoxOptions options)
        {
            if (owner == null)
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }

            MaxHeight = owner.Height;
            MaxWidth = owner.Width;
            result = defaultResult;
            Owner = owner;
            Text = messageBoxText;
            Caption = caption;
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
