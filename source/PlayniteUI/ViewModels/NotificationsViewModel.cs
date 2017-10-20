using Playnite;
using PlayniteUI.Commands;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Data;

namespace PlayniteUI.ViewModels
{
    public class NotificationsViewModel : ObservableObject
    {
        private ObservableCollection<NotificationMessage> messages;
        public ObservableCollection<NotificationMessage> Messages
        {
            get => messages;
            set
            {
                messages = value;

                BindingOperations.EnableCollectionSynchronization(messages, messagesLock);
                OnPropertyChanged("Messages");
            }
        }

        private bool autoShow = true;
        public bool AutoShow
        {
            get => autoShow;
            set
            {
                autoShow = value;
                OnPropertyChanged("AutoShow");
            }
        }

        public RelayCommand<NotificationMessage> InvokeActionCommand
        {
            get => new RelayCommand<NotificationMessage>((message) =>
            {
                InvokeMessageAction(message);
            });
        }

        private static object messagesLock = new object();
        private IWindowFactory window;

        public NotificationsViewModel(IWindowFactory window)
        {            
            this.window = window;
            Messages = new ObservableCollection<NotificationMessage>();
        }

        public void AddMessage(NotificationMessage message)
        {
            if (!Messages.Any(a => a.Id == message.Id))
            {
                Messages.Add(message);
            }

            if (AutoShow)
            {
                window.Show(this);
                window.BringToForeground();
            }
        }

        public void RemoveMessage(int id)
        {
            var message = Messages.FirstOrDefault(a => a.Id == id);
            if (message != null)
            {
                Messages.Remove(message);
            }
        }

        public void ClearMessages()
        {
            Messages.Clear();
        }

        public void InvokeMessageAction(NotificationMessage message)
        {
            message.ClickAction?.Invoke();
        }
    }
}
