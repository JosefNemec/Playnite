using Playnite.API.DesignData;
using Playnite.Converters;
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
    public class Api : BindingExtension
    {
        public Api() : this(null)
        {
        }

        public Api(string path) : base(path)
        {

            if (DesignerProperties.GetIsInDesignMode(new DependencyObject()))
            {
                Source = new DesignPlayniteAPI();
                PathRoot = null;
            }
            else
            {
                Source = PlayniteApplication.Current;
                PathRoot = nameof(PlayniteApplication.Api);
            }

            if (!path.IsNullOrEmpty())
            {
                PathRoot += ".";
            }
        }
    }
}
