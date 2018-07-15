using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK
{
    /// <summary>
    /// Represents executable function.
    /// </summary>
    public class ExtensionFunction
    {
        /// <summary>
        /// Gets or sets function name.
        /// </summary>
        public string Name
        {
            get; set;
        }

        private Action func;

        /// <summary>
        /// Creates new instance of ExtensionFunction with specified name.
        /// </summary>
        /// <param name="name">Function name.</param>
        public ExtensionFunction(string name)
        {
            Name = name;
        }

        /// <summary>
        /// Creates new instance of ExtensionFunction with specified name and method to execute.
        /// </summary>
        /// <param name="name">Function name.</param>
        /// <param name="func">Method to be executed.</param>
        public ExtensionFunction(string name, Action func) : this(name)
        {
            this.func = func;
        }

        /// <summary>
        /// Invokes function.
        /// </summary>
        public virtual void Invoke()
        {
            func?.Invoke();
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Name;
        }
    }
}
