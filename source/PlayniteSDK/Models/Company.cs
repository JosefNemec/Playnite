using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK.Models
{
    /// <summary>
    /// Describes Company object.
    /// </summary>
    public class Company : DatabaseObject
    {
        /// <summary>
        /// Creates new instance of <see cref="Company"/>.
        /// </summary>
        public Company() : base()
        {
        }

        /// <summary>
        /// Creates new instance of <see cref="Company"/>.
        /// </summary>
        /// <param name="name"></param>
        public Company(string name) : base()
        {
            Name = name;
        }

        /// <summary>
        /// 
        /// </summary>
        public static readonly Company Empty = new Company { Id = Guid.Empty, Name = string.Empty };
    }

    /// <summary>
    /// Describes Developer object.
    /// </summary>
    public class Developer : Company
    {
        /// <summary>
        /// Creates new instance of <see cref="Developer"/>.
        /// </summary>
        public Developer() : base()
        {
        }

        /// <summary>
        /// Creates new instance of <see cref="Developer"/>.
        /// </summary>
        /// <param name="name"></param>
        public Developer(string name) : base()
        {

        }
    }

    /// <summary>
    /// Describes Publisher object.
    /// </summary>
    public class Publisher : Company
    {
        /// <summary>
        /// Creates new instance of <see cref="Publisher"/>.
        /// </summary>
        public Publisher() : base()
        {
        }

        /// <summary>
        /// Creates new instance of <see cref="Publisher"/>.
        /// </summary>
        /// <param name="name"></param>
        public Publisher(string name) : base()
        {

        }
    }
}
