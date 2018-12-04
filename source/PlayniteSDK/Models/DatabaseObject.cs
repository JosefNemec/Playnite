﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK.Models
{
    /// <summary>
    /// Represents base database object item.
    /// </summary>
    public class DatabaseObject : ObservableObject
    {
        /// <summary>
        /// Gets or sets identifier of database object.
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// Creates new instance of <see cref="DatabaseObject"/>.
        /// </summary>
        public DatabaseObject()
        {
            Id = Guid.NewGuid();
        }
    }
}
