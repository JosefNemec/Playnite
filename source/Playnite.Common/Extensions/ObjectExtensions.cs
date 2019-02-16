using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Playnite
{
    public static class ObjectExtensions
    {
        public static bool HasMethod(this object obj, string methodName)
        {
            if (obj == null)
            {
                return false;
            }

            try
            {
                return obj.GetType().GetMethod(methodName) != null;
            }
            catch (AmbiguousMatchException)
            {
                // Ambiguous means there is more than one result
                return true;
            }
        }
    }
}
