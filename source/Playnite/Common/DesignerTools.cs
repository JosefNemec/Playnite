using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Windows
{
    public class DesignerTools
    {
        private static bool? inDesignMode = null;

        public static bool IsInDesignMode
        {
            get
            {
                if (inDesignMode == null)
                {
                    inDesignMode = DesignerProperties.GetIsInDesignMode(new System.Windows.DependencyObject());
                }

                return inDesignMode.Value;
            }
        }
    }
}
