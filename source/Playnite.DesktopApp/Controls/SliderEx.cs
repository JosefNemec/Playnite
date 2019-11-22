using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Playnite.DesktopApp.Controls
{
    public class SliderEx : Slider
    {
        internal bool IgnoreChanges { get; set; } = false;

        public SliderEx() : base()
        {
        }

        public double FinalValue
        {
            get { return (double)GetValue(FinalValueProperty); }
            set { SetValue(FinalValueProperty, value); }
        }

        public static readonly DependencyProperty FinalValueProperty =
            DependencyProperty.Register(
                nameof(FinalValue),
                typeof(double),
                typeof(SliderEx),
                new FrameworkPropertyMetadata(0d,

                    FinalValueChangedCallback));

        protected override void OnThumbDragCompleted(DragCompletedEventArgs e)
        {
            base.OnThumbDragCompleted(e);
            IgnoreChanges = false;
            FinalValue = Value;
            IgnoreChanges = true;
        }

        private static void FinalValueChangedCallback(DependencyObject sender, DependencyPropertyChangedEventArgs e)
        {
            var obj = sender as SliderEx;
            if (!obj.IgnoreChanges)
            {
                var value = (double)e.NewValue;
                obj.Value = value;
                obj.FinalValue = value;
            }
        }

    }
}
