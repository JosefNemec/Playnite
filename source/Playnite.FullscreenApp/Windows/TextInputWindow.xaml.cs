using Playnite.SDK;
using Playnite.Commands;
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
using System.Runtime.CompilerServices;

namespace Playnite.FullscreenApp.Windows
{
    public partial class TextInputWindow : WindowBase
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

        public RelayCommand<object> BackSpaceCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                BackSpace();
            });
        }

        public RelayCommand<object> ClearTextCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                ClearText();
            });
        }

        public RelayCommand<object> ConfirmCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                Confirm();
            });
        }

        public RelayCommand<object> CancelCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                Cancel();
            });
        }

        public TextInputWindow() : base()
        {
            InitializeComponent();
            DataContext = this;
        }

        public StringSelectionDialogResult ShowInput(Window owner, string messageBoxText, string caption, string defaultInput)
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
            Button1.Focus();
            Text = messageBoxText;
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

        public void Confirm()
        {
            result = MessageBoxResult.OK;
            Close();
        }

        public void Cancel()
        {
            result = MessageBoxResult.Cancel;
            Close();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var button = (Button)sender;
            if (button.Command == null)
            {
                var text = button.Content?.ToString();
                InputText += text;
            }
        }

        public void BackSpace()
        {
            if (!string.IsNullOrEmpty(InputText))
            {
                InputText = InputText.Substring(0, InputText.Length - 1);
            }
        }

        public void ClearText()
        {
            InputText = string.Empty;
        }
    }
}
