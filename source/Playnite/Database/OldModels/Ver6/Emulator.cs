using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Database.OldModels.Ver6
{
    /// <summary>
    /// Represents emulator profile.
    /// </summary>
    public class EmulatorProfile : DatabaseObject
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
                OnPropertyChanged("Platforms");
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
                OnPropertyChanged("ImageExtensions");
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
                OnPropertyChanged("Executable");
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
                OnPropertyChanged("Arguments");
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
                OnPropertyChanged("WorkingDirectory");
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
        public EmulatorProfile()
        {
            Id = Guid.NewGuid();
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
                OnPropertyChanged("Profiles");
            }
        }

        /// <summary>
        /// Creates new instance of Emulator.
        /// </summary>
        public Emulator()
        {
            Id = Guid.NewGuid();
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
    }
}
