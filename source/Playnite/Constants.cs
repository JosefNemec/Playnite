using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite
{
    public static class Constants
    {
        public static string DateUiFormat
        {
            get
            {
                return CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;
            }
        }
    }
}
