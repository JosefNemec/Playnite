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
using Playnite.FullscreenApp.Controls;

namespace Playnite.FullscreenApp.Windows
{
    public partial class TextInputWindow : WindowBase
    {
        private MessageBoxResult result;
        private bool capsEnabled = false;

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

        public RelayCommand<object> ToggleCapsCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                ToggleCaps();
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

        public RelayCommand<object> AddSpaceCommand
        {
            get => new RelayCommand<object>((a) =>
            {
                AddSpace();
            });
        }

        public RelayCommand<KeyEventArgs> PreviewKeyUpDownCommand
        {
            get => new RelayCommand<KeyEventArgs>((a) =>
            {
                ProcessPreviewKeyUpDown(a);
            });
        }

        public RelayCommand<TextCompositionEventArgs> PreviewTextInputCommand
        {
            get => new RelayCommand<TextCompositionEventArgs>((a) =>
            {
                ProcessTextInput(a);
            });
        }

        private List<MessageBoxToggle> toggleOptions;
        public List<MessageBoxToggle> ToggleOptions
        {
            get => toggleOptions;
            set
            {
                toggleOptions = value;
                OnPropertyChanged(nameof(ToggleOptions));
            }
        }

        public TextInputWindow() : base()
        {
            InitializeComponent();
            WindowTools.ConfigureChildWindow(this);
            DataContext = this;
        }

        public StringSelectionDialogResult ShowInput(
            Window owner,
            string messageBoxText,
            string caption,
            string defaultInput,
            List<MessageBoxToggle> options = null)
        {
            if (owner == null)
            {
                WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }

            if (this != owner)
            {
                Owner = owner;
            }

            Button1.Focus();
            Text = messageBoxText;
            InputText = defaultInput ?? string.Empty;
            ToggleOptions = options;
            if (options.HasItems())
            {
                ItemsToggleOptions.Visibility = Visibility.Visible;
            }

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

        private void TextInputText_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up)
            {
                TextInputText.MoveFocus(new TraversalRequest(FocusNavigationDirection.Up));
                e.Handled = true;
                return;
            }
            else if (e.Key == Key.Down)
            {
                TextInputText.MoveFocus(new TraversalRequest(FocusNavigationDirection.Down));
                e.Handled = true;
                return;
            }
            else if (e.Key == Key.Return)
            {
                e.Handled = true;
                Confirm();
            }
        }

        private void ToggleCaps()
        {
            foreach (var child in GridInput.Children)
            {
                if (child is ButtonEx button)
                {
                    var cont = button.Content?.ToString();
                    if (cont.IsNullOrEmpty() || cont.Length > 1)
                    {
                        continue;
                    }

                    button.Content = capsEnabled ? cont.ToLower() : cont.ToUpper();
                }
            }

            capsEnabled = !capsEnabled;
        }

        private void AddSpace()
        {
            InputText += " ";
        }

        private void ProcessPreviewKeyUpDown(KeyEventArgs a)
        {
            if (a.Key == Key.Space)
            {
                a.Handled = true;
                AddSpace();
            }
        }

        private void ProcessTextInput(TextCompositionEventArgs a)
        {
            if (a.Text == "\b")
            {
                BackSpace();
            }
            else if (a.Text == "\t")
            {
            }
            else
            {
                InputText += a.Text;
            }
        }
    }
}
