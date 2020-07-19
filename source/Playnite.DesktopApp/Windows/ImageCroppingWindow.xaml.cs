using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using Playnite.Controls;
using Playnite.Windows;

/*
 * Original code from: https://github.com/JackWangCUMT/CroppingAdorner
 */

namespace Playnite.DesktopApp.Windows
{
    public class ImageCroppingWindowFactory : WindowFactory
    {
        public override WindowBase CreateNewWindowInstance()
        {
            return new ImageCroppingWindow();
        }
    }

    public partial class ImageCroppingWindow 
    {
        public ImageCroppingWindow()
        {
            InitializeComponent();
        }

        private CroppingAdorner clp;
        private FrameworkElement frameworkElement;
        private bool isDragging;
        private Point anchorPoint;

        private void FrameLayer_OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            anchorPoint.X = e.GetPosition(SelectionBox).X;
            anchorPoint.Y = e.GetPosition(SelectionBox).Y;
            isDragging = true;
        }

        private void FrameLayer_OnMouseMove(object sender, MouseEventArgs e)
        {
            if (!isDragging)
                return;

            var x = e.GetPosition(SelectionBox).X;
            var y = e.GetPosition(SelectionBox).Y;

            Rect.SetValue(Canvas.LeftProperty, Math.Min(x, anchorPoint.X));
            Rect.SetValue(Canvas.TopProperty, Math.Min(y, anchorPoint.Y));

            Rect.Width = Math.Abs(x - anchorPoint.X);
            Rect.Height = Math.Abs(y - anchorPoint.Y);

            if (Rect.Visibility != Visibility.Visible)
                Rect.Visibility = Visibility.Visible;
        }

        private void RemoveCropFromCur()
        {
            var aly = AdornerLayer.GetAdornerLayer(frameworkElement);
            aly?.Remove(clp);
        }

        private void AddCropToElement(FrameworkElement fel)
        {
            if (frameworkElement != null)
            {
                RemoveCropFromCur();
            }
            var rcInterior = new Rect(
                (double)Rect.GetValue(Canvas.LeftProperty),
                (double)Rect.GetValue(Canvas.TopProperty),
                Rect.Width,
                Rect.Height);
            var aly = AdornerLayer.GetAdornerLayer(fel);
            clp = new CroppingAdorner(fel, rcInterior);
            if (aly == null)
                return;

            aly.Add(clp);
            CroppedImage.Source = clp.BpsCrop();
            //imgCrop.Source = _clp.BpsCrop();
            clp.CropChanged += CropChanged;
            frameworkElement = fel;
            SetClipColorGrey();
        }

        private void SetClipColorGrey()
        {
            if (clp == null) return;

            var clr = Colors.Black;
            clr.A = 110;
            clp.Fill = new SolidColorBrush(clr);
        }

        private void CropChanged(object sender, RoutedEventArgs rea)
        {
            RefreshCropImage();
        }

        private void RefreshCropImage()
        {
            if (clp == null) return;
            var rc = clp.ClippingRectangle;
            CroppedImage.Source = clp.BpsCrop();
            //imgCrop.Source = _clp.BpsCrop();
        }

        private void FrameLayer_OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Rect.Visibility = Visibility.Collapsed;
            isDragging = false;

            AddCropToElement(Image);
        }
    }
}
