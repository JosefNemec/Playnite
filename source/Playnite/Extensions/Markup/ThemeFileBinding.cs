using Playnite.API.DesignData;
using Playnite.Converters;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Playnite.Extensions.Markup
{
    public class ThemeFileBinding : BindingExtension
    {
        public string PathFormat { get; set; }

        public ThemeFileBinding() : this(null)
        {
        }

        public ThemeFileBinding(string path) : base(path)
        {
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (ServiceProvider.IsTargetTemplate(serviceProvider))
            {
                return this;
            }

            if (!PathFormat.IsNullOrEmpty() && !(Converter is GenericTypeConverter))
            {
                if (Converter != null)
                {
                    Converter = new GenericTypeConverter
                    {
                        StringFormat = ThemeFile.GetFilePath(PathFormat, false),
                        TestAsFilePath = true,
                        CustomConverter = Converter
                    };
                }
                else
                {
                    Converter = new GenericTypeConverter
                    {
                        StringFormat = ThemeFile.GetFilePath(PathFormat, false),
                        TestAsFilePath = true
                    };
                }
            }

            return base.ProvideValue(serviceProvider);
        }
    }
}
