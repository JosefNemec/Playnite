using Playnite.Plugins;
using Playnite.SDK.Plugins;
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
    public class PluginSettings : BindingExtension
    {
        public string Plugin { get; set; }

        public PluginSettings() : this(null)
        {
        }

        public PluginSettings(string path) : base(path)
        {
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            if (ServiceProvider.IsTargetTemplate(serviceProvider))
            {
                return this;
            }

            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                Source = "";
                PathRoot = "";
                return base.ProvideValue(serviceProvider);
            }

            var pSource = PlayniteApplication.Current.Extensions?.SettingsSupportList.FirstOrDefault(a => a.SourceName == Plugin);
            if (pSource == null || pSource.SettingsRoot.IsNullOrEmpty())
            {
                PlayniteApplication.Current.ExtensionsLoaded += Current_ExtensionsLoaded;
                Source = "";
                PathRoot = "";
                return base.ProvideValue(serviceProvider);
            }
            else
            {
                Source = pSource.Source;
                PathRoot = $"{pSource.SettingsRoot}.";
                return base.ProvideValue(serviceProvider);
            }
        }

        private void Current_ExtensionsLoaded(object sender, EventArgs e)
        {
            PlayniteApplication.Current.ExtensionsLoaded -= Current_ExtensionsLoaded;
            var pSource = PlayniteApplication.Current?.Extensions?.SettingsSupportList.FirstOrDefault(a => a.SourceName == Plugin);
            if (pSource != null)
            {
                PathRoot = $"{pSource.SettingsRoot}.";
                binding.Path = new PropertyPath(PathRoot + Path);
                binding.Source = pSource.Source;
            }
        }
    }
}
