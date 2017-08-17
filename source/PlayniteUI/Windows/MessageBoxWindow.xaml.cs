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
    /// <summary>
    /// Interaction logic for MessageBoxWindow.xaml
    /// </summary>
    public partial class MessageBoxWindow : WindowBase, INotifyPropertyChanged
    {
        private MessageBoxResult result;

        public event PropertyChangedEventHandler PropertyChanged;

        private string image = null;
        public string Image
        {
            get => image;
            set
            {
                image = value;
                OnPropertyChanged("Image");
            }
        }

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

        public MessageBoxWindow()
        {
            InitializeComponent();
        }

        public void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public MessageBoxResult Show(Window owner, string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult, MessageBoxOptions options)
        {
            result = defaultResult;
            Owner = owner;
            Text = messageBoxText;
            Caption = caption;

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

            switch (icon)
            {
                case MessageBoxImage.None:
                    Image = null;
                    break;
                case MessageBoxImage.Stop:
                    Image = @"/Images/MessageBox/error.png";
                    break;
                case MessageBoxImage.Question:
                    Image = @"/Images/MessageBox/question.png";
                    break;
                case MessageBoxImage.Warning:
                    Image = @"/Images/MessageBox/warning.png";
                    break;
                case MessageBoxImage.Information:
                    Image = @"/Images/MessageBox/information.png";
                    break;
                default:
                    Image = null;
                    break;
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
