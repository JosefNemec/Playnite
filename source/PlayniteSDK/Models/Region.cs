using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK.Models
{
    /// <summary>
    /// Describes Region object.
    /// </summary>
    public class Region : DatabaseObject
    {
        private string specificationId;
        /// <summary>
        /// Gets or sets specification identifier.
        /// </summary>
        public string SpecificationId
        {
            get => specificationId;
            set
            {
                specificationId = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Creates new instance of <see cref="Region"/>.
        /// </summary>
        public Region() : base()
        {
        }

        /// <summary>
        /// Creates new instance of <see cref="Region"/>.
        /// </summary>
        /// <param name="name"></param>
        public Region(string name) : base()
        {
            Name = name;
        }

        /// <summary>
        /// Gets empty region.
        /// </summary>
        public static readonly Region Empty = new Region { Id = Guid.Empty, Name = string.Empty };

        /// <inheritdoc/>
        public override void CopyDiffTo(object target)
        {
            base.CopyDiffTo(target);

            if (target is Region tro)
            {
                if (!string.Equals(SpecificationId, tro.SpecificationId, StringComparison.Ordinal))
                {
                    tro.SpecificationId = SpecificationId;
                }
            }
            else
            {
                throw new ArgumentException($"Target object has to be of type {GetType().Name}");
            }
        }
    }
}
