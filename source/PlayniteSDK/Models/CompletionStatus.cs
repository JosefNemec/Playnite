using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK.Models
{
    /// <summary>
    /// Represents game completion status.
    /// </summary>
    public class CompletionStatus : DatabaseObject
    {
        /// <summary>
        /// Creates new instance of <see cref="CompletionStatus"/>.
        /// </summary>
        public CompletionStatus() : base()
        {
        }

        /// <summary>
        /// Creates new instance of <see cref="CompletionStatus"/>.
        /// </summary>
        /// <param name="name"></param>
        public CompletionStatus(string name) : base()
        {
            Name = name;
        }

        /// <summary>
        /// Gets empty CompletionStatus.
        /// </summary>
        public static readonly CompletionStatus Empty = new CompletionStatus { Id = Guid.Empty, Name = string.Empty };
    }
}
