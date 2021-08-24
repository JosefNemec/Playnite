using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;

namespace Playnite.Extensions.Markup
{
    public class PluginStatus : MarkupExtension
    {
        public string Status { get; set; }
        public string Plugin { get; set; }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (ServiceProvider.IsTargetTemplate(serviceProvider) || Plugin.IsNullOrEmpty() || Status.IsNullOrEmpty())
            {
                return this;
            }

            var plugin = PlayniteApplication.Current?.Extensions.Plugins.FirstOrDefault(a => a.Value.Description.Id == Plugin).Value;
            var installed = plugin != null;
            var provider = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;
            var targetType = IProvideValueTargetExtensions.GetTargetType(provider);
            if (targetType == typeof(Visibility))
            {
                return installed ? Visibility.Visible : Visibility.Collapsed;
            }
            else if (targetType == typeof(string))
            {
                return installed.ToString();
            }
            else
            {
                return installed;
            }
        }
    }
}
