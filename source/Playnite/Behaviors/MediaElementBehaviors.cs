using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace Playnite.Behaviors
{
    public class MediaElementBehaviors
    {
        private static readonly DependencyProperty RepeatProperty =
            DependencyProperty.RegisterAttached(
            "Repeat",
            typeof(bool),
            typeof(MediaElementBehaviors),
            new PropertyMetadata(new PropertyChangedCallback(RepeatPropertyChanged)));

        public static bool GetRepeat(DependencyObject obj)
        {
            return (bool)obj.GetValue(RepeatProperty);
        }

        public static void SetRepeat(DependencyObject obj, bool value)
        {
            obj.SetValue(RepeatProperty, value);
        }

        private static void RepeatPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            if (DesignerProperties.GetIsInDesignMode(obj))
            {
                return;
            }

            var media = (MediaElement)obj;
            if ((bool)args.NewValue)
            {
                media.UnloadedBehavior = MediaState.Manual;
                media.MediaEnded += Control_MediaEnded;
            }
            else
            {
                media.MediaEnded -= Control_MediaEnded;
            }
        }

        private static void Control_MediaEnded(object sender, RoutedEventArgs e)
        {
            Console.WriteLine("restert");
            var media = (MediaElement)sender;
            media.Position = TimeSpan.Zero;
            media.Play();
        }
    }
}
