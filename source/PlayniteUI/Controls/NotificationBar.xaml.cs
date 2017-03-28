using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PlayniteUI
{
    public class NotificationIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            var icon = (NotificationType)value;

            switch (icon)
            {
                case NotificationType.Info:
                    return @"/Images/Icons/info.png";
                case NotificationType.Error:
                    return @"/Images/Icons/warn.png";
                default:
                    return null;
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class NotificationMessage
    {
        public int Id
        {
            get;set;
        }

        public string Text
        {
            get; set;
        }

        public NotificationType Type
        {
            get;set;
        }

        public delegate void ClickDelegate();
        public ClickDelegate ClickAction
        {
            get;set;
        }

        public NotificationMessage(int id, string text, NotificationType type, ClickDelegate action)
        {
            Id = id;
            Text = text;
            Type = type;
            ClickAction = action;
        }
    }

    public enum NotificationType
    {
        Info,
        Error
    }

    /// <summary>
    /// Interaction logic for NotificationBar.xaml
    /// </summary>
    public partial class NotificationBar : UserControl, INotifyPropertyChanged
    {
        private bool autoVisible;
        public bool AutoVisible
        {
            get
            {
                return autoVisible;
            }

            set
            {
                if (value != autoVisible)
                {
                    autoVisible = value;
                    OnPropertyChanged("AutoVisible");
                }
            }
        }

        private NotificationType notificationIcon;
        public NotificationType NotificationIcon
        {
            get
            {
                return notificationIcon;
            }

            set
            {
                if (notificationIcon != value)
                {
                    notificationIcon = value;
                    OnPropertyChanged("NotificationIcon");
                }
            }
        }

        private string text;
        public string Text
        {
            get
            {
                return text;
            }

            set
            {
                if (text != value)
                {
                    text = value;
                    OnPropertyChanged("Text");
                }
            }
        }

        private ObservableCollection<NotificationMessage> Messages;
        private static object messagesLock = new object();

        public event PropertyChangedEventHandler PropertyChanged;

        public NotificationBar()
        {
            InitializeComponent();
            Messages = new ObservableCollection<NotificationMessage>();
            Messages.CollectionChanged += Messages_CollectionChanged;
            BindingOperations.EnableCollectionSynchronization(Messages, messagesLock);
            ItemsNotifications.ItemsSource = Messages;
        }

        private void NotifItem_MouseUp(object sender, MouseButtonEventArgs e)
        {
            var obj = (FrameworkElement)sender;
            var notifItem = (NotificationMessage)obj.DataContext;
            notifItem.ClickAction?.Invoke();
        }

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            Visibility = Visibility.Collapsed;
        }

        private void MainBar_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (Messages.Count == 1)
            {
                Messages[0].ClickAction?.Invoke();
            }
            else
            {
                if (ItemsNotifications.Visibility == Visibility.Visible)
                {
                    ItemsNotifications.Visibility = Visibility.Collapsed;
                }
                else
                {
                    ItemsNotifications.Visibility = Visibility.Visible;
                }
            }
        }

        private void Messages_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (Messages.Count > 1)
            {
                Text = "There are two or more new messages (click to show)";
                var warn = Messages.FirstOrDefault(a => a.Type == NotificationType.Error);
                if (warn != null)
                {
                    NotificationIcon = NotificationType.Error;
                }
                else
                {
                    NotificationIcon = NotificationType.Info;
                }
            }
            else if (Messages.Count == 0)
            {
                Text = string.Empty;
                NotificationIcon = NotificationType.Info;
            }
            else
            {
                Text = Messages[0].Text;
                NotificationIcon = Messages[0].Type;
            }
        }

        public void AddMessage(NotificationMessage message)
        {
            if (Messages.FirstOrDefault(a => a.Id == message.Id) == null)
            {
                Messages.Add(message);
            }

            if (autoVisible)
            {
                Dispatcher.Invoke(() => Visibility = Visibility.Visible);
            }
        }

        public void RemoveMessage(int id)
        {
            var message = Messages.FirstOrDefault(a => a.Id == id);
            if (message != null)
            {
                Messages.Remove(message);
            }

            if (autoVisible)
            {
                if (Messages.Count == 0)
                {
                    Dispatcher.Invoke(() => Visibility = Visibility.Collapsed);
                }
            }
        }

        public void ClearMessages()
        {
            Messages.Clear();
        }

        public void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
