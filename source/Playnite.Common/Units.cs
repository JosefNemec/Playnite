using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Common
{
    public static class Units
    {
        public static long MegaBytesToBytes(long megaBytes)
        {
            return megaBytes * 1024 * 1024;
        }
    }
}
