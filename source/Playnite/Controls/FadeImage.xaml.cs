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

namespace Playnite.Controls
{
    /// <summary>
    /// Interaction logic for FadeImage.xaml
    /// </summary>
    public partial class FadeImage : UserControl
    {
        internal Storyboard mainAnim;
        internal Storyboard stateAnim;
        internal Storyboard borderDarkenOut;

        // TODO rewrite all of this to something more sane

        public FadeImage()
        {
            InitializeComponent();
            mainAnim = (Storyboard)TryFindResource("Anim");            
            stateAnim = (Storyboard)TryFindResource("Anim2");
            borderDarkenOut = (Storyboard)TryFindResource("BorderDarkenOut");
            borderDarkenOut.Completed += BorderDarkenOut_Completed;
            stateAnim.Completed += stateAnim_Completed;
        }

        private void BorderDarkenOut_Completed(object sender, EventArgs e)
        {
            BorderDarken.Opacity = 0;
        }

        private void stateAnim_Completed(object sender, EventArgs e)
        {
            if (MainImage.Source == null)
            {
                borderDarkenOut.Begin();                
            }
        }

        public static readonly DependencyProperty StretchProperty =
            DependencyProperty.Register(nameof(Stretch), typeof(Stretch), typeof(FadeImage),
        new PropertyMetadata(Stretch.UniformToFill));

        public Stretch Stretch
        {
            get { return (Stretch)GetValue(StretchProperty); }
            set { SetValue(StretchProperty, value); }
        }

        public static readonly DependencyProperty SourceProperty =
            DependencyProperty.Register(nameof(Source), typeof(ImageSource), typeof(FadeImage),
                new PropertyMetadata(default(ImageSource), SourceChanged));

        public ImageSource Source
        {
            get { return (ImageSource)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        public static readonly DependencyProperty ImageOpacityMaskProperty =
                DependencyProperty.Register(nameof(ImageOpacityMask), typeof(Brush), typeof(FadeImage),
            new PropertyMetadata());

        public Brush ImageOpacityMask
        {
            get { return (Brush)GetValue(ImageOpacityMaskProperty); }
            set { SetValue(ImageOpacityMaskProperty, value); }
        }

        public static readonly DependencyProperty ImageDarkeningBrushProperty =
                DependencyProperty.Register(nameof(ImageDarkeningBrush), typeof(Brush), typeof(FadeImage),
            new PropertyMetadata());

        public Brush ImageDarkeningBrush
        {
            get { return (Brush)GetValue(ImageDarkeningBrushProperty); }
            set { SetValue(ImageDarkeningBrushProperty, value); }
        }

        private static void SourceChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (FadeImage)obj;
            var newSource = (ImageSource)args.NewValue;
            var oldSource = (ImageSource)args.OldValue;

            control.StagingImage.Visibility = Visibility.Visible;
            control.BorderDarken.Opacity = 1;
            if (oldSource != null)
            {
                control.StagingImage.Source = oldSource;
            }

            control.MainImage.Source = newSource;
            control.mainAnim.Begin();

            if (oldSource != null)
            {
                control.stateAnim.Begin();
            }
        }
    }
}
