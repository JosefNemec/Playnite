using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Playnite.DesktopApp.Windows
{
    /*
 * Original code from: https://github.com/JackWangCUMT/CroppingAdorner
 */

	public class CroppingAdorner : Adorner
	{
		#region Private variables
		// Width of the thumbs.  I know these really aren't "pixels", but px
		// is still a good mnemonic.
		private const int _cpxThumbWidth = 6;

		//DPS: Ancho del cuadro de movimiento
		private const int _cpxThumbMoveWidth = 10;

		// PuncturedRect to hold the "Cropping" portion of the adorner
		private PuncturedRect _prCropMask;

		// Canvas to hold the thumbs so they can be moved in response to the user
		private Canvas _cnvThumbs;

		// Cropping adorner uses Thumbs for visual elements.  
		// The Thumbs have built-in mouse input handling.
		private CropThumb _crtTopLeft, _crtTopRight, _crtBottomLeft, _crtBottomRight;
		private CropThumb _crtTop, _crtLeft, _crtBottom, _crtRight;
		//DPS: Move
		private CropThumb _crtMove;

		// To store and manage the adorner's visual children.
		private VisualCollection _vc;

		// DPI for screen
		private static double s_dpiX, s_dpiY;
		#endregion

		#region Properties
		public Rect ClippingRectangle => _prCropMask.RectInterior;

        #endregion

		#region Routed Events
		public static readonly RoutedEvent CropChangedEvent = EventManager.RegisterRoutedEvent(
			"CropChanged",
			RoutingStrategy.Bubble,
			typeof(RoutedEventHandler),
			typeof(CroppingAdorner));

		public event RoutedEventHandler CropChanged
		{
			add => AddHandler(CropChangedEvent, value);
            remove => RemoveHandler(CropChangedEvent, value);
        }
		#endregion

		#region Dependency Properties
		public static DependencyProperty FillProperty = Shape.FillProperty.AddOwner(typeof(CroppingAdorner));

		public Brush Fill
		{
			get => (Brush)GetValue(FillProperty);
            set => SetValue(FillProperty, value);
        }

		private static void FillPropChanged(DependencyObject d, DependencyPropertyChangedEventArgs args)
		{
			var crp = d as CroppingAdorner;

			if (crp != null)
			{
				crp._prCropMask.Fill = (Brush)args.NewValue;
			}
		}
		#endregion

		#region Constructor
		static CroppingAdorner()
		{
			var clr = Colors.Red;
			var g = System.Drawing.Graphics.FromHwnd((IntPtr)0);

			s_dpiX = g.DpiX;
			s_dpiY = g.DpiY;
			clr.A = 80;
			FillProperty.OverrideMetadata(typeof(CroppingAdorner),
				new PropertyMetadata(
					new SolidColorBrush(clr),
					FillPropChanged));
		}

		public CroppingAdorner(UIElement adornedElement, Rect rcInit)
			: base(adornedElement)
		{
			_vc = new VisualCollection(this);
            _prCropMask = new PuncturedRect {IsHitTestVisible = false, RectInterior = rcInit, Fill = Fill};
            _vc.Add(_prCropMask);
            _cnvThumbs = new Canvas
            {
                HorizontalAlignment = HorizontalAlignment.Stretch, VerticalAlignment = VerticalAlignment.Stretch
            };

            _vc.Add(_cnvThumbs);
			BuildCorner(ref _crtTop, Cursors.SizeNS);
			BuildCorner(ref _crtBottom, Cursors.SizeNS);
			BuildCorner(ref _crtLeft, Cursors.SizeWE);
			BuildCorner(ref _crtRight, Cursors.SizeWE);
			BuildCorner(ref _crtTopLeft, Cursors.SizeNWSE);
			BuildCorner(ref _crtTopRight, Cursors.SizeNESW);
			BuildCorner(ref _crtBottomLeft, Cursors.SizeNESW);
			BuildCorner(ref _crtBottomRight, Cursors.SizeNWSE);
			//DPS: Move
			BuildMove(ref _crtMove, Cursors.Hand);

			// Add handlers for Cropping.
			_crtBottomLeft.DragDelta += HandleBottomLeft;
			_crtBottomRight.DragDelta += HandleBottomRight;
			_crtTopLeft.DragDelta += HandleTopLeft;
			_crtTopRight.DragDelta += HandleTopRight;
			_crtTop.DragDelta += HandleTop;
			_crtBottom.DragDelta += HandleBottom;
			_crtRight.DragDelta += HandleRight;
			_crtLeft.DragDelta += HandleLeft;

			//DPS: Manejador para el movimiento
			_crtMove.DragDelta += HandleMove;

			// We have to keep the clipping interior withing the bounds of the adorned element
			// so we have to track it's size to guarantee that...

            if (adornedElement is FrameworkElement fel)
			{
				fel.SizeChanged += AdornedElement_SizeChanged;
			}
		}
		#endregion

		#region Thumb handlers
		// Generic handler for Cropping
		private void HandleThumb(
			double drcL,
			double drcT,
			double drcW,
			double drcH,
			double dx,
			double dy)
		{
			var rcInterior = _prCropMask.RectInterior;

			if (rcInterior.Width + drcW * dx < 0)
			{
				dx = -rcInterior.Width / drcW;
			}

			if (rcInterior.Height + drcH * dy < 0)
			{
				dy = -rcInterior.Height / drcH;
			}

			rcInterior = new Rect(
				rcInterior.Left + drcL * dx,
				rcInterior.Top + drcT * dy,
				rcInterior.Width + drcW * dx,
				rcInterior.Height + drcH * dy);

			_prCropMask.RectInterior = rcInterior;
			SetThumbs(_prCropMask.RectInterior);
			RaiseEvent(new RoutedEventArgs(CropChangedEvent, this));
		}

		// Handler for Cropping from the bottom-left.
		private void HandleBottomLeft(object sender, DragDeltaEventArgs args)
		{
			if (sender is CropThumb)
			{
				HandleThumb(
					1, 0, -1, 1,
					args.HorizontalChange,
					args.VerticalChange);
			}
		}

		// Handler for Cropping from the bottom-right.
		private void HandleBottomRight(object sender, DragDeltaEventArgs args)
		{
			if (sender is CropThumb)
			{
				HandleThumb(
					0, 0, 1, 1,
					args.HorizontalChange,
					args.VerticalChange);
			}
		}

		// Handler for Cropping from the top-right.
		private void HandleTopRight(object sender, DragDeltaEventArgs args)
		{
			if (sender is CropThumb)
			{
				HandleThumb(
					0, 1, 1, -1,
					args.HorizontalChange,
					args.VerticalChange);
			}
		}

		// Handler for Cropping from the top-left.
		private void HandleTopLeft(object sender, DragDeltaEventArgs args)
		{
			if (sender is CropThumb)
			{
				HandleThumb(
					1, 1, -1, -1,
					args.HorizontalChange,
					args.VerticalChange);
			}
		}

		// Handler for Cropping from the top.
		private void HandleTop(object sender, DragDeltaEventArgs args)
		{
			if (sender is CropThumb)
			{
				HandleThumb(
					0, 1, 0, -1,
					args.HorizontalChange,
					args.VerticalChange);
			}
		}

		// Handler for Cropping from the left.
		private void HandleLeft(object sender, DragDeltaEventArgs args)
		{
			if (sender is CropThumb)
			{
				HandleThumb(
					1, 0, -1, 0,
					args.HorizontalChange,
					args.VerticalChange);
			}
		}

		// Handler for Cropping from the right.
		private void HandleRight(object sender, DragDeltaEventArgs args)
		{
			if (sender is CropThumb)
			{
				HandleThumb(
					0, 0, 1, 0,
					args.HorizontalChange,
					args.VerticalChange);
			}
		}

		// Handler for Cropping from the bottom.
		private void HandleBottom(object sender, DragDeltaEventArgs args)
		{
			if (sender is CropThumb)
			{
				HandleThumb(
					0, 0, 0, 1,
					args.HorizontalChange,
					args.VerticalChange);
			}
		}

		//DPS: Manejador para el movimiento
		private void HandleMove(object sender, DragDeltaEventArgs args)
		{
			if (sender is CropThumb)
			{
				HandleThumb(
					1, 1, 0, 0,
					args.HorizontalChange,
					args.VerticalChange);
			}
		}
		#endregion

		#region Other handlers
		private void AdornedElement_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			var fel = sender as FrameworkElement;
			var rcInterior = _prCropMask.RectInterior;
			var fFixupRequired = false;
			double
				intLeft = rcInterior.Left,
				intTop = rcInterior.Top,
				intWidth = rcInterior.Width,
				intHeight = rcInterior.Height;

            if (fel != null)
            {
                if (rcInterior.Left > fel.RenderSize.Width)
                {
                    intLeft = fel.RenderSize.Width;
                    intWidth = 0;
                    fFixupRequired = true;
                }

                if (rcInterior.Top > fel.RenderSize.Height)
                {
                    intTop = fel.RenderSize.Height;
                    intHeight = 0;
                    fFixupRequired = true;
                }

                if (rcInterior.Right > fel.RenderSize.Width)
                {
                    intWidth = Math.Max(0, fel.RenderSize.Width - intLeft);
                    fFixupRequired = true;
                }

                if (rcInterior.Bottom > fel.RenderSize.Height)
                {
                    intHeight = Math.Max(0, fel.RenderSize.Height - intTop);
                    fFixupRequired = true;
                }
			}

			if (fFixupRequired)
			{
				_prCropMask.RectInterior = new Rect(intLeft, intTop, intWidth, intHeight);
			}
		}
		#endregion

		#region Arranging/positioning
		private void SetThumbs(Rect rc)
		{
			_crtBottomRight.SetPos(rc.Right, rc.Bottom);
			_crtTopLeft.SetPos(rc.Left, rc.Top);
			_crtTopRight.SetPos(rc.Right, rc.Top);
			_crtBottomLeft.SetPos(rc.Left, rc.Bottom);
			_crtTop.SetPos(rc.Left + rc.Width / 2, rc.Top);
			_crtBottom.SetPos(rc.Left + rc.Width / 2, rc.Bottom);
			_crtLeft.SetPos(rc.Left, rc.Top + rc.Height / 2);
			_crtRight.SetPos(rc.Right, rc.Top + rc.Height / 2);

			//DPS: Move
			_crtMove.SetPos(rc.Left + 10, rc.Top);
		}

		// Arrange the Adorners.
		protected override Size ArrangeOverride(Size finalSize)
		{
			var rcExterior = new Rect(0, 0, AdornedElement.RenderSize.Width, AdornedElement.RenderSize.Height);
			_prCropMask.RectExterior = rcExterior;
			var rcInterior = _prCropMask.RectInterior;
			_prCropMask.Arrange(rcExterior);

			SetThumbs(rcInterior);
			_cnvThumbs.Arrange(rcExterior);
			return finalSize;
		}
		#endregion

		#region Public interface
		public BitmapSource BpsCrop()
		{
			var margin = AdornerMargin();
			var rcInterior = _prCropMask.RectInterior;

			var pxFromSize = UnitsToPx(rcInterior.Width, rcInterior.Height);

			// It appears that CroppedBitmap indexes from the upper left of the margin whereas RenderTargetBitmap renders the
			// control exclusive of the margin.  Hence our need to take the margins into account here...

			var pxFromPos = UnitsToPx(rcInterior.Left + margin.Left, rcInterior.Top + margin.Top);
			var pxWhole = UnitsToPx(AdornedElement.RenderSize.Width + margin.Left, AdornedElement.RenderSize.Height + margin.Left);
			pxFromSize.X = Math.Max(Math.Min(pxWhole.X - pxFromPos.X, pxFromSize.X), 0);
			pxFromSize.Y = Math.Max(Math.Min(pxWhole.Y - pxFromPos.Y, pxFromSize.Y), 0);
			if (Math.Abs(pxFromSize.X) < double.Epsilon || Math.Abs(pxFromSize.Y) < double.Epsilon)
			{
				return null;
			}
			var rcFrom = new Int32Rect((int) pxFromPos.X, (int)pxFromPos.Y, (int)pxFromSize.X, (int)pxFromSize.Y);

			var rtb = new RenderTargetBitmap((int)pxWhole.X, (int)pxWhole.Y, s_dpiX, s_dpiY, PixelFormats.Default);
			rtb.Render(AdornedElement);
			return new CroppedBitmap(rtb, rcFrom);
		}
		#endregion

		#region Helper functions
		private Thickness AdornerMargin()
		{
			var thick = new Thickness(0);
			if (AdornedElement is FrameworkElement element)
			{
				thick = element.Margin;
			}
			return thick;
		}

		private void BuildCorner(ref CropThumb crt, Cursor crs)
		{
			if (crt != null) return;

			crt = new CropThumb(_cpxThumbWidth);

			// Set some arbitrary visual characteristics.
			crt.Cursor = crs;

			_cnvThumbs.Children.Add(crt);
		}
		/// <summary>
		/// Thumb para movimiento
		/// </summary>
		/// <param name="crt"></param>
		/// <param name="crs"></param>
		/// <remarks>DPS</remarks>
		private void BuildMove(ref CropThumb crt, Cursor crs)
		{
			if (crt != null) return;

			crt = new CropThumb(_cpxThumbMoveWidth);

			// Set some arbitrary visual characteristics.
			crt.Cursor = crs;

			_cnvThumbs.Children.Add(crt);
		}

		private static Point UnitsToPx(double x, double y)
		{
			return new Point((int)(x * s_dpiX / 96), (int)(y * s_dpiY / 96));
		}
		#endregion

		#region Visual tree overrides
		// Override the VisualChildrenCount and GetVisualChild properties to interface with 
		// the adorner's visual collection.
		protected override int VisualChildrenCount => _vc.Count;
        protected override Visual GetVisualChild(int index) { return _vc[index]; }
		#endregion

		#region Internal Classes
		class CropThumb : Thumb
		{
			#region Private variables
			int _cpx;
			#endregion

			#region Constructor
			internal CropThumb(int cpx)
            {
				_cpx = cpx;
			}
			#endregion

			#region Overrides
			protected override Visual GetVisualChild(int index)
			{
				return null;
			}

			protected override void OnRender(DrawingContext drawingContext)
			{
				drawingContext.DrawRoundedRectangle(Brushes.White, new Pen(Brushes.Black, 1), new Rect(new Size(_cpx, _cpx)), 1, 1);
			}
			#endregion

			#region Positioning
			internal void SetPos(double x, double y)
			{
				Canvas.SetTop(this, y - _cpx / 2);
				Canvas.SetLeft(this, x - _cpx / 2);
			}
			#endregion
		}
		#endregion
	}
}
