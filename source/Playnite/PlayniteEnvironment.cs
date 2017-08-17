using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite
{
    public class PlayniteEnvironment
    {
#if DEBUG
        public static bool ThrowAllErrors = true;
#else
        public static bool ThrowAllErrors = false;
#endif
    }
}
