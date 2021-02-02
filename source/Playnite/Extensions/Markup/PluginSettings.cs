using Playnite.Plugins;
using Playnite.SDK.Plugins;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Playnite.Extensions.Markup
{
    public class PluginSettings : BindingExtension
    {
        public string Plugin { get; set; }
        public PluginUiElementSupport PluginSource { get; set; }

        public PluginSettings() : this(null)
        {
        }

        public PluginSettings(string path) : base(path)
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                return;
            }
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (ServiceProvider.IsTargetTemplate(serviceProvider))
            {
                return this;
            }

            PluginSource = PlayniteApplication.Current?.Extensions.CustomElementList.FirstOrDefault(a => a.SourceName == Plugin);
            if (PluginSource == null || PluginSource.SettingsRoot.IsNullOrEmpty())
            {
                return null;
            }

            Source = PluginSource.Source;
            PathRoot = PluginSource.SettingsRoot + '.';
            return base.ProvideValue(serviceProvider);
        }
    }
}
