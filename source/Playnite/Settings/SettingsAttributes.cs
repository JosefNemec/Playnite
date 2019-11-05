using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite
{
    [AttributeUsage(AttributeTargets.Property)]
    public class RequiresRestartAttribute : Attribute
    {        
    }
}
