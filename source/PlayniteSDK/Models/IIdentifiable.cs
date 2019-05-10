using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK.Models
{
    /// <summary>
    /// Desribest object identifiable by an id.
    /// </summary>
    public interface IIdentifiable
    {
        /// <summary>
        /// Gets unique object identifier.
        /// </summary>
        Guid Id { get; }
    }
}
