using Playnite.API.DesignData;
using Playnite.Converters;
using Playnite.DesktopApp.ViewModels;
using Playnite.Extensions;
using Playnite.Extensions.Markup;
using Playnite.ViewModels.Desktop.DesignData;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Markup;

namespace Playnite.DesktopApp.Markup
{
    public class MainViewModel : BindingExtension
    {
        public bool DirectValue { get; set; } = false;

        public MainViewModel() : this(null)
        {
        }

        public MainViewModel(string path) : base(path)
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                Source = new DesignMainViewModel();
                PathRoot = null;
            }
            else
            {
                Source = DesktopApplication.Current;
                PathRoot = nameof(DesktopApplication.MainModel);
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
                    return typeof(DesktopAppViewModel).GetProperty(Path).GetValue(Source, null);
                }
                else
                {
                    var src = ((DesktopApplication)Source).MainModel;
                    return typeof(DesktopAppViewModel).GetProperty(Path).GetValue(src, null);
                }
            }
            else
            {
                return base.ProvideValue(serviceProvider);
            }
        }
    }
}
