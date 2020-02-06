using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK.Models
{
    /// <summary>
    /// Represents emulator profile.
    /// </summary>
    public class EmulatorProfile : DatabaseObject, IEquatable<EmulatorProfile>
    {
        private List<Guid> platforms;
        /// <summary>
        /// Gets or sets platforms supported by profile.
        /// </summary>
        public List<Guid> Platforms
        {
            get => platforms;
            set
            {
                platforms = value;
                OnPropertyChanged();
            }
        }

        private List<string> imageExtensions;
        /// <summary>
        /// Gets or sets file extension supported by profile.
        /// </summary>
        public List<string> ImageExtensions
        {
            get => imageExtensions;
            set
            {
                imageExtensions = value;
                OnPropertyChanged();
            }
        }

        private string executable;
        /// <summary>
        /// Gets or sets executable path used to launch emulator.
        /// </summary>
        public string Executable
        {
            get => executable;
            set
            {
                executable = value;
                OnPropertyChanged();
            }
        }

        private string arguments;
        /// <summary>
        /// Gets or sets arguments for emulator executable.
        /// </summary>
        public string Arguments
        {
            get => arguments;
            set
            {
                arguments = value;
                OnPropertyChanged();
            }
        }

        private string workingDirectory;
        /// <summary>
        /// Gets or sets working directory of emulator process.
        /// </summary>
        public string WorkingDirectory
        {
            get => workingDirectory;
            set
            {
                workingDirectory = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        /// Creates new instance of EmulatorProfile.
        /// </summary>
        public EmulatorProfile() : base()
        {
        }

        /// <inheritdoc/>
        public bool Equals(EmulatorProfile other)
        {
            if (other is null)
            {
                return false;
            }

            if (!Platforms.IsListEqual(other.Platforms))
            {
                return false;
            }

            if (!ImageExtensions.IsListEqual(other.ImageExtensions))
            {
                return false;
            }

            if (!string.Equals(Executable, other.Executable, StringComparison.Ordinal))
            {
                return false;
            }

            if (!string.Equals(Arguments, other.Arguments, StringComparison.Ordinal))
            {
                return false;
            }

            if (!string.Equals(WorkingDirectory, other.WorkingDirectory, StringComparison.Ordinal))
            {
                return false;
            }

            if (!string.Equals(Name, other.Name, StringComparison.Ordinal))
            {
                return false;
            }

            return true;
        }
    }

    /// <summary>
    /// Represents system emulator.
    /// </summary>
    public class Emulator : DatabaseObject
    {
        private ObservableCollection<EmulatorProfile> profile;
        /// <summary>
        /// Gets or sets list of emulator profiles.
        /// </summary>
        public ObservableCollection<EmulatorProfile> Profiles
        {
            get => profile;
            set
            {
                profile = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Creates new instance of Emulator.
        /// </summary>
        public Emulator() : base()
        {
        }

        /// <summary>
        /// Creates new instance of Emulator with specific name.
        /// </summary>
        /// <param name="name">Emulator name.</param>
        public Emulator(string name) : this()
        {
            Name = name;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return Name;
        }

        /// <inheritdoc/>
        public override void CopyDiffTo(object target)
        {
            base.CopyDiffTo(target);

            if (target is Emulator tro)
            {
                if (!Profiles.IsListEqualExact(tro.Profiles))
                {
                    tro.Profiles = Profiles;
                }
            }
            else
            {
                throw new ArgumentException($"Target object has to be of type {GetType().Name}");
            }
        }
    }
}
