using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Database.OldModels.NewVer1
{
    /// <summary>
    /// Represents base database object item.
    /// </summary>
    public class OldDatabaseObject : ObservableObject
    {
        /// <summary>
        /// Gets or sets identifier of database object.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Creates new instance of <see cref="OldDatabaseObject"/>.
        /// </summary>
        public OldDatabaseObject()
        {
            Id = Guid.NewGuid();
        }
    }
}
