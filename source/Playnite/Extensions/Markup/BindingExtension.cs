using Playnite.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Playnite.Extensions.Markup
{
    public class BindingExtension : MarkupExtension
    {
        internal Binding binding;
        public string PathRoot { get; set; }
        public object Source { get; set; }
        public string Path { get; set; }
        public object TargetNullValue { get; set; }
        public object FallbackValue { get; set; }
        public int Delay { get; set; }
        public BindingMode Mode { get; set; } = BindingMode.OneWay;
        public IValueConverter Converter { get; set; }
        public object ConverterParameter { get; set; }
        public string StringFormat { get; set; }

        public BindingExtension()
        {
        }

        public BindingExtension(string path)
        {
            Path = path;
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            binding = new Binding()
            {
                Path = new PropertyPath(PathRoot + Path),
                Delay = Delay,
                Mode = Mode
            };

            if (Source != null)
            {
                binding.Source = Source;
            }

            if (TargetNullValue != null)
            {
                binding.TargetNullValue = TargetNullValue;
            }

            if (FallbackValue != null)
            {
                binding.FallbackValue = FallbackValue;
            }

            if (Converter != null)
            {
                binding.Converter = Converter;
            }

            if (ConverterParameter != null)
            {
                binding.ConverterParameter = ConverterParameter;
            }

            if (!StringFormat.IsNullOrEmpty())
            {
                binding.StringFormat = StringFormat;
            }

            var provider = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;
            if (ServiceProvider.IsTargetTemplate(serviceProvider))
            {
                return this;
            }
            else
            {
                var targetType = IProvideValueTargetExtensions.GetTargetType(provider);
                if (targetType == typeof(Visibility) && Converter == null)
                {
                    binding.Converter = new BooleanToVisibilityConverter();
                }

                if (provider.TargetProperty == null)
                {
                    return binding;
                }
                else if (provider.TargetProperty.GetType() == typeof(DependencyProperty))
                {
                    return BindingOperations.SetBinding(
                        provider.TargetObject as DependencyObject,
                        provider.TargetProperty as DependencyProperty,
                        binding);
                }
                else
                {
                    if (targetType == typeof(BindingBase))
                    {
                        return binding;
                    }
                }

                return this;
            }
        }
    }
}
