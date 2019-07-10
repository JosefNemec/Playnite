using Playnite.Commands;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.API.DesignData
{
    public class DesignNotificationsAPI : ObservableObject, INotificationsAPI
    {
        public RelayCommand<object> RemoveAllCommands
        {
            get => new RelayCommand<object>((a) =>
            {
                RemoveAll();
            });
        }

        public ObservableCollection<NotificationMessage> Messages { get; set; }

        public int Count { get; }

        public DesignNotificationsAPI()
        {
            Count = 3;
            Messages = new ObservableCollection<NotificationMessage>
            {
                new NotificationMessage("design1", "Design notification message 1", NotificationType.Error),
                new NotificationMessage("design2", "Design message 2", NotificationType.Info),
                new NotificationMessage("design3", "Design notification message 3, long message that does to multiple lies. Long message that does to multiple lies.", NotificationType.Error)
            };
        }

        public void Add(NotificationMessage message)
        {
            throw new NotImplementedException();
        }

        public void Add(string id, string text, NotificationType type)
        {
            throw new NotImplementedException();
        }

        public void Remove(string id)
        {
            throw new NotImplementedException();
        }

        public void RemoveAll()
        {
            Messages.Clear();
        }
    }
}
