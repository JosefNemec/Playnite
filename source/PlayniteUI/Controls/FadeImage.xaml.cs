using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace PlayniteUI.Controls
{
    /// <summary>
    /// Interaction logic for FadeImage.xaml
    /// </summary>
    public partial class FadeImage : UserControl
    {
        public FadeImage()
        {
            InitializeComponent();
            
        }

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register("Source", typeof(ImageSource), typeof(FadeImage),
                new PropertyMetadata(default(ImageSource), SourceChanged));

        public ImageSource Source
        {
            get { return (ImageSource)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        private static void SourceChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (FadeImage)obj;
            var newSource = (ImageSource)args.NewValue;
            var oldSource = (ImageSource)args.OldValue;

            control.StagingImage.Visibility = Visibility.Visible;
            control.StagingImage.Source = oldSource;
            control.MainImage.Source = newSource;

            var story = (Storyboard)control.TryFindResource("Anim");
            story.Begin();
            story = (Storyboard)control.TryFindResource("Anim2");
            story.Begin();
        }
    }
}
