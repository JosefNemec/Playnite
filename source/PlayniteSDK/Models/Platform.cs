using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK.Models
{
    /// <summary>
    /// Represents game's platform.
    /// </summary>
    public class Platform : DatabaseObject
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

        private string icon;
        /// <summary>
        /// Gets or sets platform icon.
        /// </summary>
        public string Icon
        {
            get => icon;
            set
            {
                icon = value;
                OnPropertyChanged();
            }
        }

        private string cover;
        /// <summary>
        /// Gets or sets default game cover.
        /// </summary>
        public string Cover
        {
            get => cover;
            set
            {
                cover = value;
                OnPropertyChanged();
            }
        }

        private string background;
        /// <summary>
        /// Gets or sets default game background image.
        /// </summary>
        public string Background
        {
            get => background;
            set
            {
                background = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Creates new instance of Platform.
        /// </summary>
        public Platform() : base()
        {
        }

        /// <summary>
        /// Creates new instance of Platform with specific name.
        /// </summary>
        /// <param name="name">Platform name.</param>
        public Platform(string name) : this()
        {
            Name = name;
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        /// Gets empty platform.
        /// </summary>
        public static readonly Platform Empty = new Platform { Id = Guid.Empty, Name = string.Empty };

        /// <inheritdoc/>
        public override void CopyDiffTo(object target)
        {
            base.CopyDiffTo(target);

            if (target is Platform tro)
            {
                if (!string.Equals(Icon, tro.Icon, StringComparison.Ordinal))
                {
                    tro.Icon = Icon;
                }

                if (!string.Equals(Cover, tro.Cover, StringComparison.Ordinal))
                {
                    tro.Cover = Cover;
                }

                if (!string.Equals(Background, tro.Background, StringComparison.Ordinal))
                {
                    tro.Background = Background;
                }

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
