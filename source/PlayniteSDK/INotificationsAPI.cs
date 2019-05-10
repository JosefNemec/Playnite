using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Playnite.SDK
{
    /// <summary>
    /// Represents notification message;
    /// </summary>
    public class NotificationMessage
    {
        /// <summary>
        /// Gets command executed when user activates notification.
        /// </summary>
        public ICommand ActivationCommand
        {
            get;
        }

        /// <summary>
        /// Gets notification id.
        /// </summary>
        public string Id
        {
            get;
        }

        /// <summary>
        /// Gets notification content text.
        /// </summary>
        public string Text
        {
            get;
        }

        /// <summary>
        /// Gets notification type.
        /// </summary>
        public NotificationType Type
        {
            get;
        }

        /// <summary>
        /// Gets action to be invoked when notification is activated.
        /// </summary>
        public Action InvokeAction
        {
            get;
        }

        /// <summary>
        /// Creates new instance of <see cref="NotificationMessage"/>.
        /// </summary>
        /// <param name="id">Notification id.</param>
        /// <param name="text">Notification text.</param>
        /// <param name="type">Notification type.</param>
        public NotificationMessage(string id, string text, NotificationType type) : this(id, text, type, null)
        {
        }

        /// <summary>
        /// Creates new instance of <see cref="NotificationMessage"/>.
        /// </summary>
        /// <param name="id">Notification id.</param>
        /// <param name="text">Notification text.</param>
        /// <param name="type">Notification type.</param>
        /// <param name="command">Action to be invoked when notification is activated.</param>
        public NotificationMessage(string id, string text, NotificationType type, ICommand command)
        {
            Id = id;
            Text = text;
            Type = type;
            ActivationCommand = command;
        }
    }

    /// <summary>
    /// Notification.
    /// </summary>
    public enum NotificationType
    {
        /// <summary>
        /// Info severity.
        /// </summary>
        Info,
        /// <summary>
        /// Error serverity
        /// </summary>
        Error
    }

    /// <summary>
    /// Describes notification API.
    /// </summary>
    public interface INotificationsAPI
    {
        /// <summary>
        /// Gets list of all notification messages.
        /// </summary>
        ObservableCollection<NotificationMessage> Messages { get; }

        /// <summary>
        /// Gets notification count.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Adds new notification message.
        /// </summary>
        /// <param name="message">Notification message</param>
        void Add(NotificationMessage message);

        /// <summary>
        /// Adds new notification message.
        /// </summary>
        /// <param name="id">Notification id.</param>
        /// <param name="text">Notification text.</param>
        /// <param name="type">Notification type.</param>
        void Add(string id, string text, NotificationType type);

        /// <summary>
        /// Removes specific notification.
        /// </summary>
        /// <param name="id">Notification id.</param>
        void Remove(string id);

        /// <summary>
        /// Removes all notifications.
        /// </summary>
        void RemoveAll();
    }
}
