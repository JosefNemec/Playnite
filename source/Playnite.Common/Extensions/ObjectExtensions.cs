using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System
{
    public static class ObjectExtensions
    {
        public static T CrateInstance<T>(this Type type, params object[] parameters)
        {
            return (T)Activator.CreateInstance(type, parameters);
        }
    }
}
