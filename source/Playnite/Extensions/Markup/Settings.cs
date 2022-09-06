using Playnite.Converters;
using Playnite.SDK;
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
    public class SettingsBinding : Binding
    {
        public SettingsBinding() : this(null)
        {
        }

        public SettingsBinding(string path) : base(path)
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                Source = new PlayniteSettings();
            }
            else
            {
                Source = PlayniteApplication.Current.AppSettings;
            }
        }
    }

    public class Settings : BindingExtension
    {
        private static ILogger logger = LogManager.GetLogger();
        private static readonly Dictionary<string, PropertyInfo> directValuePropCache = new Dictionary<string, PropertyInfo>();
        public bool DirectValue { get; set; } = false;

        public Settings() : this(null)
        {
        }

        public Settings(string path) : base(path)
        {
            if (DesignerTools.IsInDesignMode)
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
                // Doesn't support nested properties!
                var src = Source;
                if (!DesignerTools.IsInDesignMode)
                {
                    src = PlayniteApplication.Current.AppSettings;
                }

                if (directValuePropCache.TryGetValue(Path, out var prop))
                {
                    return prop?.GetValue(src, null);
                }
                else
                {
                    var newProp = typeof(PlayniteSettings).GetProperty(Path);
                    directValuePropCache.Add(Path, newProp);
                    if (newProp != null)
                    {
                        return newProp.GetValue(src, null);
                    }
                    else
                    {
                        logger.Error($"Failed to get value of \"{Path}\" path from app settings.");
                        return null;
                    }
                }
            }
            else
            {
                return base.ProvideValue(serviceProvider);
            }
        }
    }
}
