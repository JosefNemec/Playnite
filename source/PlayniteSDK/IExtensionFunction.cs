using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK
{
    public class ExtensionFunction
    {
        public string Name
        {
            get; set;
        }

        private Action func;

        public ExtensionFunction(string name)
        {
            Name = name;
        }

        public ExtensionFunction(string name, Action func) : this(name)
        {
            this.func = func;
        }

        public virtual void Invoke()
        {
            func?.Invoke();
        }
    }
}
