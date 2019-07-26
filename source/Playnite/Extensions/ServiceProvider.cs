using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace Playnite.Extensions
{
    public class ServiceProvider
    {
        public static bool IsTargetTemplate(IServiceProvider serviceProvider)
        {
            var provider = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;
            return provider.TargetObject.GetType().FullName == "System.Windows.SharedDp";
        }
    }
}
