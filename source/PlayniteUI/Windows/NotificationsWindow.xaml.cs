using PlayniteUI.Controls;
using System;
using System.ComponentModel;
using System.Windows.Data;

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

    public class NotificationsWindowFactory : WindowFactory
    {
        public static NotificationsWindowFactory Instance
        {
            get => new NotificationsWindowFactory();
        }

        public override WindowBase CreateNewWindowInstance()
        {
            return new NotificationsWindow();
        }
    }

    /// <summary>
    /// Interaction logic for NotificationsWindow.xaml
    /// </summary>
    public partial class NotificationsWindow : WindowBase
    { 
        public NotificationsWindow()
        {
            InitializeComponent();
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Visibility = System.Windows.Visibility.Hidden;
            e.Cancel = true;
        }
    }
}
