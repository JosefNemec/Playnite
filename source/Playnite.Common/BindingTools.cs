using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Playnite.Common
{
    public class BindingTools
    {
        public static BindingExpressionBase SetBinding(DependencyObject target, DependencyProperty dp, BindingBase binding)
        {
            return BindingOperations.SetBinding(target, dp, binding);
        }

        public static BindingExpressionBase SetBinding(
            DependencyObject target,
            DependencyProperty dp,
            object source,
            string path,
            BindingMode mode = BindingMode.OneWay,
            UpdateSourceTrigger trigger = UpdateSourceTrigger.Default,
            IValueConverter converter = null,
            object converterParameter = null,
            string stringFormat = null,
            object fallBackValue = null)
        {
            var binding = new Binding
            {
                Source = source,
                Path = new PropertyPath(path),
                Mode = mode,
                UpdateSourceTrigger = trigger,
                Converter = converter,
                ConverterParameter = converterParameter,
                StringFormat = stringFormat,
                FallbackValue = fallBackValue
            };

            return BindingOperations.SetBinding(target, dp, binding);
        }
    }
}
