using Playnite.Commands;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Playnite.API
{
    public class NotificationsAPI : ObservableObject, INotificationsAPI
    {
        private readonly SynchronizationContext context;

        public RelayCommand<object> RemoveAllCommands
        {
            get => new RelayCommand<object>((a) =>
            {
                RemoveAll();
            });
        }

        public ObservableCollection<NotificationMessage> Messages
        {
            get;
        }

        public int Count
        {
            get => Messages.Count;
        }

        public NotificationsAPI()
        {
            context = SynchronizationContext.Current;
            Messages = new ObservableCollection<NotificationMessage>();
        }

        public void Add(NotificationMessage message)
        {
            context.Send((c =>
            {
                if (!Messages.Any(a => a.Id == message.Id))
                {
                    Messages.Add(message);
                    OnPropertyChanged(nameof(Count));
                }
            }), null);
        }

        public void Add(string id, string text, NotificationType type)
        {
            context.Send((c =>
            {
                if (!Messages.Any(a => a.Id == id))
                {
                    Messages.Add(new NotificationMessage(id, text, type, null));
                    OnPropertyChanged(nameof(Count));
                }
            }), null);
        }

        public void Remove(string id)
        {
            context.Send((c =>
            {
                var message = Messages.FirstOrDefault(a => a.Id == id);
                if (message != null)
                {
                    Messages.Remove(message);
                    OnPropertyChanged(nameof(Count));
                }
            }), null);
        }

        public void RemoveAll()
        {
            context.Send((c =>
            {
                Messages.Clear();
                OnPropertyChanged(nameof(Count));
            }), null);
        }
    }
}
