using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    public static class LongExtensions
    {
        public static DateTime ToDateFromUnixMs(this long value)
        {            
            return DateTimeOffset.FromUnixTimeMilliseconds(value).DateTime;
        }

        public static DateTime ToDateFromUnixSeconds(this long value)
        {
            return DateTimeOffset.FromUnixTimeSeconds(value).DateTime;
        }
    }
}
