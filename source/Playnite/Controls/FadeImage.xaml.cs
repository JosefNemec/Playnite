using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
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
        private enum CurrentImage
        {
            Image1,
            Image2,
            None
        }

        private CurrentImage currentImage = CurrentImage.None;

        internal Storyboard Image1FadeIn;
        internal Storyboard Image2FadeIn;
        internal Storyboard Image1FadeOut;
        internal Storyboard Image2FadeOut;
        internal Storyboard stateAnim;
        internal Storyboard BorderDarkenFadeOut;

        #region Source

        public static readonly DependencyProperty SourceProperty = DependencyProperty.Register(
            nameof(Source),
            typeof(string),
            typeof(FadeImage),
            new PropertyMetadata(null, SourceChanged));

        public string Source
        {
            get { return (string)GetValue(SourceProperty); }
            set { SetValue(SourceProperty, value); }
        }

        #endregion Source

        #region ImageOpacityMask

        public static readonly DependencyProperty ImageOpacityMaskProperty = DependencyProperty.Register(
            nameof(ImageOpacityMask),
            typeof(Brush),
            typeof(FadeImage),
            new PropertyMetadata());

        public Brush ImageOpacityMask
        {
            get { return (Brush)GetValue(ImageOpacityMaskProperty); }
            set { SetValue(ImageOpacityMaskProperty, value); }
        }

        #endregion ImageOpacityMask

        #region ImageDarkeningBrush

        public static readonly DependencyProperty ImageDarkeningBrushProperty = DependencyProperty.Register(
            nameof(ImageDarkeningBrush),
            typeof(Brush),
            typeof(FadeImage),
            new PropertyMetadata());

        public Brush ImageDarkeningBrush
        {
            get { return (Brush)GetValue(ImageDarkeningBrushProperty); }
            set { SetValue(ImageDarkeningBrushProperty, value); }
        }

        #endregion ImageDarkeningBrush

        #region Stretch

        public static readonly DependencyProperty StretchProperty = DependencyProperty.Register(
            nameof(Stretch),
            typeof(Stretch),
            typeof(FadeImage),
            new PropertyMetadata(Stretch.UniformToFill));

        public Stretch Stretch
        {
            get { return (Stretch)GetValue(StretchProperty); }
            set { SetValue(StretchProperty, value); }
        }

        #endregion Strech
        
        #region IsBlurEnabled

        public static readonly DependencyProperty IsBlurEnabledProperty = DependencyProperty.Register(
            nameof(IsBlurEnabled),
            typeof(bool),
            typeof(FadeImage),
            new PropertyMetadata(false, BlurSettingChanged));

        public bool IsBlurEnabled
        {
            get { return (bool)GetValue(IsBlurEnabledProperty); }
            set { SetValue(IsBlurEnabledProperty, value); }
        }

        #endregion IsBlurEnabled

        #region BlurAmount

        public static readonly DependencyProperty BlurAmountProperty = DependencyProperty.Register(
            nameof(BlurAmount),
            typeof(int),
            typeof(FadeImage),
            new PropertyMetadata(10, BlurSettingChanged));

        public int BlurAmount
        {
            get { return (int)GetValue(BlurAmountProperty); }
            set { SetValue(BlurAmountProperty, value); }
        }

        #endregion IsBlurEnabled

        public FadeImage()
        {
            InitializeComponent();
            Image1FadeIn = (Storyboard)TryFindResource("Image1FadeIn");
            Image2FadeIn = (Storyboard)TryFindResource("Image2FadeIn");
            Image1FadeOut = (Storyboard)TryFindResource("Image1FadeOut");
            Image2FadeOut = (Storyboard)TryFindResource("Image2FadeOut");
            BorderDarkenFadeOut = (Storyboard)TryFindResource("BorderDarkenFadeOut");
            Image1FadeOut.Completed += Image1FadeOut_Completed;
            Image2FadeOut.Completed += Image2FadeOut_Completed;
            BorderDarkenFadeOut.Completed += BorderDarkenOut_Completed;
        }

        private void Image1FadeOut_Completed(object sender, EventArgs e)
        {
            Image1.Source = null;
        }

        private void Image2FadeOut_Completed(object sender, EventArgs e)
        {
            Image2.Source = null;
        }

        private void BorderDarkenOut_Completed(object sender, EventArgs e)
        {
            BorderDarken.Opacity = 0;
        }

        private static async void BlurSettingChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (FadeImage)obj;
            if (control.Source.IsNullOrEmpty())
            {
                return;
            }

            var blurAmount = control.BlurAmount;
            var blurEnabled = control.IsBlurEnabled;
            var source = control.Source;
            var image = await Task.Factory.StartNew(() =>
            {
                var tmp = ImageSourceManager.GetImage(control.Source, false);
                if (tmp == null)
                {
                    return null;
                }

                if (blurEnabled)
                {
                    tmp = new GaussianBlur(tmp.ToBitmap()).Process(blurAmount).ToBitmapImage();
                    // GaussianBlur uses quite of lot of memory that's not immediately released.
                    GC.Collect();
                }

                return tmp;
            });

            if (control.currentImage == CurrentImage.Image1)
            {
                control.Image1.Source = image;
            }
            else if (control.currentImage == CurrentImage.Image2)
            {
                control.Image2.Source = image;
            }
        }

        private static void SourceChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            var control = (FadeImage)obj;
            control.LoadNewSource((string)args.NewValue, (string)args.OldValue);
        }

        private async void LoadNewSource(string newSource, string oldSource)
        {
            BitmapImage image = null;
            if (!newSource.IsNullOrEmpty())
            {
                var blurAmount = BlurAmount;
                var blurEnabled = IsBlurEnabled;
                image = await Task.Factory.StartNew(() =>
                {
                    var tmp = ImageSourceManager.GetImage(newSource, false);
                    if (tmp == null)
                    {
                        return null;
                    }

                    if (blurEnabled)
                    {
                        tmp = new GaussianBlur(tmp.ToBitmap()).Process(blurAmount).ToBitmapImage();
                        // GaussianBlur uses quite of lot of memory that's not immediately released.
                        GC.Collect();
                    }

                    return tmp;
                });
            }

            if (image == null)
            {
                if (currentImage == CurrentImage.None)
                {
                    return;
                }

                if (currentImage == CurrentImage.Image1)
                {
                    Image1FadeOut.Begin();
                    BorderDarkenFadeOut.Begin();
                }
                else if (currentImage == CurrentImage.Image2)
                {
                    Image2FadeOut.Begin();
                    BorderDarkenFadeOut.Begin();
                }

                currentImage = CurrentImage.None;
            }
            else
            {
                if (currentImage == CurrentImage.None)
                {
                    Image1FadeOut.Stop();
                    Image1.Source = image;
                    Image1FadeIn.Begin();
                    BorderDarken.Opacity = 1;
                    BorderDarkenFadeOut.Stop();
                    currentImage = CurrentImage.Image1;
                }
                else if (currentImage == CurrentImage.Image1)
                {
                    Image2FadeOut.Stop();
                    Image2.Source = image;
                    Image2FadeIn.Begin();
                    Image1FadeOut.Begin();
                    BorderDarken.Opacity = 1;
                    BorderDarkenFadeOut.Stop();
                    currentImage = CurrentImage.Image2;
                }
                else if (currentImage == CurrentImage.Image2)
                {
                    Image1FadeOut.Stop();
                    Image1.Source = image;
                    Image1FadeIn.Begin();
                    Image2FadeOut.Begin();
                    BorderDarken.Opacity = 1;
                    BorderDarkenFadeOut.Stop();
                    currentImage = CurrentImage.Image1;
                }
            }
        }
    }
}
