using Playnite.Extensions.Markup;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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
using TheArtOfDev.HtmlRenderer.WPF;

namespace Playnite.Controls
{
    public class GridEx : Grid
    {
        public GridEx() : base()
        {
        }

        public static readonly DependencyProperty AutoLayoutColumnsProperty =
            DependencyProperty.Register(
                "AutoLayoutColumns", typeof(int), typeof(GridEx),
                new PropertyMetadata(-1, AutoLayoutColumnsChanged));

        public static int GetAutoLayoutColumns(DependencyObject obj)
        {
            return (int)obj.GetValue(AutoLayoutColumnsProperty);
        }

        public static void SetAutoLayoutColumns(DependencyObject obj, int value)
        {
            obj.SetValue(AutoLayoutColumnsProperty, value);
        }

        public static void AutoLayoutColumnsChanged(
            DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {            
            ((GridEx)obj).ArrangeChildren();
        }

        internal void ArrangeChildren()
        {
            var columns = GetAutoLayoutColumns(this);
            if (columns == -1)
            {
                return;
            }

            for (int i = 0; i < Children.Count; i++)
            {
                var elem = Children[i];
                SetColumn(elem, i % columns);
                SetRow(elem, i / columns);
            }
        }

        protected override Size MeasureOverride(Size constraint)
        {
            ArrangeChildren();
            return base.MeasureOverride(constraint);
        }
    }
}
