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
using System.Windows.Shapes;

namespace PlayniteUI.Windows
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
    /// Interaction logic for NotificationsWindow.xaml
    /// </summary>
    public partial class NotificationsWindow : Window, INotifyPropertyChanged
    {
        private bool autoOpen;
        public bool AutoOpen
        {
            get
            {
                return autoOpen;
            }

            set
            {
                autoOpen = value;
                OnPropertyChanged("AutoOpen");
            }
        }

        private ObservableCollection<NotificationMessage> Messages;
        private static object messagesLock = new object();

        public event PropertyChangedEventHandler PropertyChanged;

        public NotificationsWindow()
        {
            InitializeComponent();
            Messages = new ObservableCollection<NotificationMessage>();
            Messages.CollectionChanged += Messages_CollectionChanged;
            BindingOperations.EnableCollectionSynchronization(Messages, messagesLock);
            ListNotifications.ItemsSource = Messages;
        }

        public void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        private void Messages_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (e.NewItems != null && AutoOpen)
            {
                Dispatcher.Invoke(() => Show());
            }
        }

        public void AddMessage(NotificationMessage message)
        {
            if (Messages.FirstOrDefault(a => a.Id == message.Id) == null)
            {
                Messages.Add(message);
            }

            if (AutoOpen)
            {
                Dispatcher.Invoke(() => Show());
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

        private void ListNotifications_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            if (ListNotifications.SelectedItem != null)
            {
                var notifItem = (NotificationMessage)ListNotifications.SelectedItem;
                notifItem.ClickAction?.Invoke();
            }
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Visibility = Visibility.Hidden;
            e.Cancel = true;
        }
    }
}
