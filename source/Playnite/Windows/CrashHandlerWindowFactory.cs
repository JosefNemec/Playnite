using Playnite.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Windows
{
    public class CrashHandlerWindowFactory : WindowFactory
    {
        public static Type WindowType { get; private set; }

        public static void SetWindowType(Type windowType)
        {
            WindowType = windowType;
        }

        public override WindowBase CreateNewWindowInstance()
        {
            return WindowType.CrateInstance<WindowBase>();
        }
    }
}
