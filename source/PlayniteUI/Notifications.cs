using PlayniteUI.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PlayniteUI
{
    public enum NotificationType
    {
        Info,
        Error
    }

    public class NotificationMessage
    {
        public string Id
        {
            get; set;
        }

        public string Text
        {
            get; set;
        }

        public NotificationType Type
        {
            get; set;
        }

        public delegate void ClickDelegate();
        public ClickDelegate ClickAction
        {
            get; set;
        }

        public NotificationMessage(string id, string text, NotificationType type, ClickDelegate action)
        {
            Id = id;
            Text = text;
            Type = type;
            ClickAction = action;
        }
    }
}
