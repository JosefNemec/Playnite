using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    public static class StrExtensions
    {
        public static string GetLocalized(this string source)
        {
            return source.StartsWith("LOC", StringComparison.Ordinal) ? ResourceProvider.GetString(source) : source;
        }
    }
}
