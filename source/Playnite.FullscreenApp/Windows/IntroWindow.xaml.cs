using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
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

namespace Playnite.FullscreenApp.Windows
{
    /// <summary>
    /// Interaction logic for IntroWindow.xaml
    /// </summary>
    public partial class IntroWindow : Window
    {
        public event EventHandler IntroEnded;
        public IntroWindow()
        {
            Loaded += OnLoaded;
            InitializeComponent();
        }

        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
           nameof(Source), typeof(Uri), typeof(IntroWindow));


        public Uri Source
        {
            get => (Uri)GetValue(SourceProperty);
            set => SetValue(SourceProperty, value);
        }

        private void Media_MediaEnded(object sender, RoutedEventArgs e)
        {
            IntroEnded?.Invoke(this, EventArgs.Empty);
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            //This is a small hack to prevent an annoying white flash when opening a WPF window.
            //Basically start the window in minimized state then maximize it after it is loaded.
            Activate();
            WindowState = WindowState.Maximized;
        }
    }
}
