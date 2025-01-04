using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Common
{
    public static class Constants
    {
        public const string DefaultDateTimeFormat = "d";
        public const string DefaultPartialReleaseDateTimeFormat = "y";

        public static string DateUiFormat
        {
            get;
        } = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;

        public static string TimeUiFormat
        {
            get;
        } = CultureInfo.CurrentCulture.DateTimeFormat.ShortTimePattern;

        public static char[] ListSeparators
        {
            get;
        } = new char[] { ListSeparator };

        public const char ListSeparator = ',';

        public static readonly Guid MaxGuidVal = new Guid("FFFFFFFF-FFFF-FFFF-FFFF-FFFFFFFFFFFF");
    }
}
