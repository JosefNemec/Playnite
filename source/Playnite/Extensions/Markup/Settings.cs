using Playnite.Converters;
using Playnite.Settings;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Playnite.Extensions.Markup
{
    public class Settings : BindingExtension
    {
        public bool DirectValue { get; set; } = false;

        public Settings() : this(null)
        {
        }

        public Settings(string path) : base(path)
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                Source = new PlayniteSettings();
                PathRoot = null;
            }
            else
            {
                Source = PlayniteApplication.Current;
                PathRoot = nameof(PlayniteApplication.AppSettings);
            }

            if (!path.IsNullOrEmpty())
            {
                PathRoot += ".";
            }
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (ServiceProvider.IsTargetTemplate(serviceProvider))
            {
                return this;
            }

            if (DirectValue)
            {
                if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
                {
                    return typeof(PlayniteSettings).GetProperty(Path).GetValue(Source, null);
                }
                else
                {
                    var src = ((PlayniteApplication)Source).AppSettings;
                    return typeof(PlayniteSettings).GetProperty(Path).GetValue(src, null);
                }
            }
            else
            {
                return base.ProvideValue(serviceProvider);
            }
        }
    }
}
