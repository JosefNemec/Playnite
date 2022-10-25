using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK.Models
{
    /// <summary>
    /// Represents game disk image.
    /// </summary>
    public class GameRom : ObservableObject, IEquatable<GameRom>
    {
        private string name;
        /// <summary>
        /// Gets ROM name.
        /// </summary>
        public string Name
        {
            get
            {
                return name;
            }

            set
            {
                name = value;
                OnPropertyChanged();
            }
        }

        private string path;
        /// <summary>
        /// Gets ROM path.
        /// </summary>
        public string Path
        {
            get
            {
                return path;
            }

            set
            {
                path = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Creates new instance of <see cref="GameRom"/>.
        /// </summary>
        public GameRom()
        {
        }

        /// <summary>
        /// Creates new instance of <see cref="GameRom"/>.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="path"></param>
        public GameRom(string name, string path)
        {
            Name = name;
            Path = path;
        }

        /// <inheritdoc/>
        public bool Equals(GameRom other)
        {
            if (other is null)
            {
                return false;
            }

            if (!string.Equals(Name, other.Name, StringComparison.Ordinal))
            {
                return false;
            }

            if (!string.Equals(Path, other.Path, StringComparison.Ordinal))
            {
                return false;
            }

            return true;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public GameRom GetCopy()
        {
            return new GameRom
            {
                Name = Name,
                Path = Path
            };
        }
    }
}
