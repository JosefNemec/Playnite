using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK
{
    /// <summary>
    /// Describes settings object.
    /// </summary>
    public interface ISettings : IEditableObject
    {
        /// <summary>
        /// Verifies settings configuration.
        /// </summary>
        /// <param name="errors">List of validation errors.</param>
        /// <returns>true if validation passes, otherwise false.</returns>
        bool VerifySettings(out List<string> errors);
    }
}
