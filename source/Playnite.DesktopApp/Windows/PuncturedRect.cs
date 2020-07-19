using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Playnite.DesktopApp.Windows
{
    /*
 * Original code from: https://github.com/JackWangCUMT/CroppingAdorner
 */

	public class PuncturedRect : Shape
	{
		#region Dependency properties
		public static readonly DependencyProperty RectInteriorProperty =
			DependencyProperty.Register(
				"RectInterior",
				typeof(Rect),
				typeof(FrameworkElement),
				new FrameworkPropertyMetadata(
					new Rect(0, 0, 0, 0),
					FrameworkPropertyMetadataOptions.AffectsRender,
					null,
					CoerceRectInterior,
					false
				),
				null
			);

		private static object CoerceRectInterior(DependencyObject d, object value)
		{
			var pr = (PuncturedRect)d;
			var rcExterior = pr.RectExterior;
			var rcProposed = (Rect)value;
			var left = Math.Max(rcProposed.Left, rcExterior.Left);
			var top = Math.Max(rcProposed.Top, rcExterior.Top);
			var width = Math.Min(rcProposed.Right, rcExterior.Right) - left;
			var height = Math.Min(rcProposed.Bottom, rcExterior.Bottom) - top;
			rcProposed = new Rect(left, top, width, height);
			return rcProposed;
		}

		public Rect RectInterior
		{
			get => (Rect)GetValue(RectInteriorProperty);
            set => SetValue(RectInteriorProperty, value);
        }


		public static readonly DependencyProperty RectExteriorProperty =
			DependencyProperty.Register(
				"RectExterior",
				typeof(Rect),
				typeof(FrameworkElement),
				new FrameworkPropertyMetadata(
					new Rect(0, 0, double.MaxValue, double.MaxValue),
					FrameworkPropertyMetadataOptions.AffectsMeasure |
					FrameworkPropertyMetadataOptions.AffectsArrange |
					FrameworkPropertyMetadataOptions.AffectsParentMeasure |
					FrameworkPropertyMetadataOptions.AffectsParentArrange |
					FrameworkPropertyMetadataOptions.AffectsRender,
					null,
					null,
					false
				),
				null
			);

		public Rect RectExterior
		{
			get => (Rect)GetValue(RectExteriorProperty);
            set => SetValue(RectExteriorProperty, value);
        }
		#endregion

		#region Constructors
		public PuncturedRect() : this(new Rect(0, 0, double.MaxValue, double.MaxValue), new Rect()) { }

		public PuncturedRect(Rect rectExterior, Rect rectInterior)
		{
			RectInterior = rectInterior;
			RectExterior = rectExterior;
		}
		#endregion

		#region Geometry
		protected override Geometry DefiningGeometry
		{
			get
			{
				var pthgExt = new PathGeometry();
                var pthfExt = new PathFigure {StartPoint = RectExterior.TopLeft};
                pthfExt.Segments.Add(new LineSegment(RectExterior.TopRight, false));
				pthfExt.Segments.Add(new LineSegment(RectExterior.BottomRight, false));
				pthfExt.Segments.Add(new LineSegment(RectExterior.BottomLeft, false));
				pthfExt.Segments.Add(new LineSegment(RectExterior.TopLeft, false));
				pthgExt.Figures.Add(pthfExt);

				var rectIntSect = Rect.Intersect(RectExterior, RectInterior);
				var pthgInt = new PathGeometry();
                var pthfInt = new PathFigure {StartPoint = rectIntSect.TopLeft};
                pthfInt.Segments.Add(new LineSegment(rectIntSect.TopRight, false));
				pthfInt.Segments.Add(new LineSegment(rectIntSect.BottomRight, false));
				pthfInt.Segments.Add(new LineSegment(rectIntSect.BottomLeft, false));
				pthfInt.Segments.Add(new LineSegment(rectIntSect.TopLeft, false));
				pthgInt.Figures.Add(pthfInt);

				var cmbg = new CombinedGeometry(GeometryCombineMode.Exclude, pthgExt, pthgInt);
				return cmbg;
			}
		}
		#endregion
	}
}
