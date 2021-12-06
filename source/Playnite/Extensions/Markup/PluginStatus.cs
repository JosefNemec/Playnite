using Playnite.Plugins;
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
    public class PluginStatus : BindingExtension
    {
        public string Status { get; set; }
        public string Plugin { get; set; }

        public PluginStatus() : this(null)
        {
        }

        public PluginStatus(string path) : base(path)
        {
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (ServiceProvider.IsTargetTemplate(serviceProvider))
            {
                return this;
            }

            Source = PlayniteApplication.Current;
            Path = $"{nameof(PlayniteApplication.ExtensionsStatusBinder)}[{Plugin}].{nameof(ExtensionsStatusBinder.Status.IsInstalled)}";
            return base.ProvideValue(serviceProvider);
        }
    }
}
