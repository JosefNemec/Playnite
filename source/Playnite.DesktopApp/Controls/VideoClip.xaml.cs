using Playnite.SDK;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Playnite.DesktopApp.Controls
{
    /// <summary>
    /// Interaction logic for VideoClip.xaml
    /// </summary>
    public partial class VideoClip : UserControl, INotifyPropertyChanged
    {
        public VideoClip()
        {
            InitializeComponent();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private bool isPlaying;
        public bool IsPlaying
        {
            get => isPlaying;
            private set
            {
                if (isPlaying != value)
                {
                    isPlaying = value;
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsPlaying)));
                }
            }
        }

        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
            nameof(Source), typeof(Uri), typeof(VideoClip), new PropertyMetadata(OnSourceChanged));

        private static void OnSourceChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            if (sender is VideoClip videoClip)
            {
                videoClip.PropertyChanged?.Invoke(videoClip, new PropertyChangedEventArgs(nameof(IsValid)));
            }
        }

        public Uri Source
        {
            get => (Uri)GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }

        public bool IsValid => Source != null;

        public ICommand PlayCommand => new RelayCommand(Play);
        public ICommand PauseCommand => new RelayCommand(Pause);
        public ICommand StopCommand => new RelayCommand(Stop);

        private void Stop()
        {
            Media.Stop();
            IsPlaying = false;
        }

        private void Pause()
        {
            Media.Pause();
            IsPlaying = false;
        }

        private void Play()
        {
            Media.Play();
            IsPlaying = true;
        }
        private void OnMediaLoaded(object sender, RoutedEventArgs e)
        {
            Pause();
        }

        private void OnMediaEnded(object sender, RoutedEventArgs e)
        {
            Stop();
        }

        private void OnMediaFailed(object sender, ExceptionRoutedEventArgs e)
        {
            Source = null;
        }
    }
}
