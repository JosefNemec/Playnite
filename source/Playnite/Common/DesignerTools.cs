using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Common
{
    public class DesignerTools
    {
        public static bool IsInDesignMode => DesignerProperties.GetIsInDesignMode(new System.Windows.DependencyObject());

    }
}
