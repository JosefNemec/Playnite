using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Playnite.Extensions.Markup
{
    public class MainViewModel<TAppViewModel, TDesignViewModel, TApp> : BindingExtension
    {
        public bool DirectValue { get; set; } = false;

        public MainViewModel() : this(null)
        {
        }

        public MainViewModel(string path) : base(path)
        {
            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                Source = typeof(TDesignViewModel).CrateInstance<TDesignViewModel>();
                PathRoot = null;
            }
            else
            {
                Source = PlayniteApplication.Current;
                PathRoot = "MainModel";
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
                    return typeof(TAppViewModel).GetProperty(Path).GetValue(Source, null);
                }
                else
                {
                    var src = typeof(TApp).GetProperty("MainModel").GetValue(PlayniteApplication.Current, null);
                    return typeof(TAppViewModel).GetProperty(Path).GetValue(src, null);
                }
            }
            else
            {
                return base.ProvideValue(serviceProvider);
            }
        }
    }
}
