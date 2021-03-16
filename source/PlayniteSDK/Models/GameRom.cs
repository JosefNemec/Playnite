using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK.Models
{
    public class GameRom : ObservableObject, IEquatable<GameRom>
    {
        private string name;
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

        public GameRom()
        {
        }

        public GameRom(string name, string path)
        {
            Name = name;
            Path = path;
        }

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
    }
}
