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
            object fallBackValue = null,
            int delay = 0,
            bool isAsync = false)
        {
            var binding = new Binding
            {
                Path = new PropertyPath(path),
                Mode = mode,
                UpdateSourceTrigger = trigger
            };

            if (converter != null)
            {
                binding.Converter = converter;
            }

            if (converterParameter != null)
            {
                binding.ConverterParameter = converterParameter;
            }

            if (source != null)
            {
                binding.Source = source;
            }

            if (fallBackValue != null)
            {
                binding.FallbackValue = fallBackValue;
            }

            if (delay > 0)
            {
                binding.Delay = delay;
            }

            if (!stringFormat.IsNullOrEmpty())
            {
                binding.StringFormat = stringFormat;
            }

            if (isAsync)
            {
                binding.IsAsync = true;
            }

            return BindingOperations.SetBinding(target, dp, binding);
        }

        public static BindingExpressionBase SetBinding(
           DependencyObject target,
           DependencyProperty dp,
           string path,
           BindingMode mode = BindingMode.OneWay,
           UpdateSourceTrigger trigger = UpdateSourceTrigger.Default,
           IValueConverter converter = null,
           object converterParameter = null,
           string stringFormat = null,
           object fallBackValue = null,
           int delay = 0,
           bool isAsync = false)
        {
            return SetBinding(
                target,
                dp,
                null,
                path,
                mode,
                trigger,
                converter,
                converterParameter,
                stringFormat,
                fallBackValue,
                delay,
                isAsync);
        }
    }
}
