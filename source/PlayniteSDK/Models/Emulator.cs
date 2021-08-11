using Playnite.SDK.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK.Models
{
    public class EmulatedRegion : IEquatable<EmulatedRegion>
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public bool DefaultImport { get; set; }
        public ulong IgdbId { get; set; }
        public List<string> Codes { get; set; }

        public override int GetHashCode()
        {
            return Id?.GetHashCode() ?? 0;
        }

        public bool Equals(EmulatedRegion other)
        {
            return other.Id == Id;
        }

        public override bool Equals(object obj)
        {
            if (obj is EmulatedRegion region)
            {
                return Equals(region);
            }
            else
            {
                return false;
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public class EmulatedPlatform : IEquatable<EmulatedPlatform>
    {
        public ulong IgdbId { get; set; }
        public string Name { get; set; }
        public string Id { get; set; }
        public List<string> Databases { get; set; }
        public List<string> Emulators { get; set; }

        public override int GetHashCode()
        {
            return Id?.GetHashCode() ?? 0;
        }

        public bool Equals(EmulatedPlatform other)
        {
            return other.Id == Id;
        }

        public override bool Equals(object obj)
        {
            if (obj is EmulatedPlatform platform)
            {
                return Equals(platform);
            }
            else
            {
                return false;
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }

    public abstract class EmulatorProfile : ObservableObject
    {
        private string id;
        public string Id
        {
            get => id;
            set
            {
                id = value;
                OnPropertyChanged();
            }
        }

        private string name;
        public string Name
        {
            get => name;
            set
            {
                name = value;
                OnPropertyChanged();
            }
        }

        private string preScript;
        public string PreScript
        {
            get => preScript;
            set
            {
                preScript = value;
                OnPropertyChanged();
            }
        }

        private string postScript;
        public string PostScript
        {
            get => postScript;
            set
            {
                postScript = value;
                OnPropertyChanged();
            }
        }

        private string exitScript;
        public string ExitScript
        {
            get => exitScript;
            set
            {
                exitScript = value;
                OnPropertyChanged();
            }
        }

        [DontSerialize]
        public Type Type => GetType();

        public override string ToString()
        {
            return Name;
        }
    }

    public class BuiltInEmulatorProfile : EmulatorProfile, IEquatable<BuiltInEmulatorProfile>
    {
        public static readonly string ProfilePrefix = "#builtin_";

        private string builtInProfileName;
        public string BuiltInProfileName
        {
            get => builtInProfileName;
            set
            {
                builtInProfileName = value;
                OnPropertyChanged();
            }
        }

        public BuiltInEmulatorProfile() : base()
        {
            Id = ProfilePrefix + Guid.NewGuid();
        }

        /// <inheritdoc/>
        public bool Equals(BuiltInEmulatorProfile other)
        {
            if (other is null)
            {
                return false;
            }

            if (!string.Equals(BuiltInProfileName, other.BuiltInProfileName, StringComparison.Ordinal))
            {
                return false;
            }

            if (!string.Equals(Name, other.Name, StringComparison.Ordinal))
            {
                return false;
            }

            if (!string.Equals(ExitScript, other.ExitScript, StringComparison.Ordinal))
            {
                return false;
            }

            if (!string.Equals(PostScript, other.PostScript, StringComparison.Ordinal))
            {
                return false;
            }

            if (!string.Equals(PreScript, other.PreScript, StringComparison.Ordinal))
            {
                return false;
            }

            return true;
        }
    }

    /// <summary>
    /// Represents emulator profile.
    /// </summary>
    public class CustomEmulatorProfile : EmulatorProfile, IEquatable<CustomEmulatorProfile>
    {
        public static readonly string ProfilePrefix = "#custom_";

        private string startupScript;
        public string StartupScript
        {
            get => startupScript;
            set
            {
                startupScript = value;
                OnPropertyChanged();
            }
        }

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
        public CustomEmulatorProfile() : base()
        {
            Id = ProfilePrefix + Guid.NewGuid();
        }

        /// <inheritdoc/>
        public bool Equals(CustomEmulatorProfile other)
        {
            if (other is null)
            {
                return false;
            }

            if (!string.Equals(Id, other.Id, StringComparison.Ordinal))
            {
                return false;
            }

            if (!string.Equals(Name, other.Name, StringComparison.Ordinal))
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

            if (!string.Equals(ExitScript, other.ExitScript, StringComparison.Ordinal))
            {
                return false;
            }

            if (!string.Equals(PostScript, other.PostScript, StringComparison.Ordinal))
            {
                return false;
            }

            if (!string.Equals(PreScript, other.PreScript, StringComparison.Ordinal))
            {
                return false;
            }

            if (!string.Equals(StartupScript, other.StartupScript, StringComparison.Ordinal))
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
        private string builtInConfigId;
        public string BuiltInConfigId
        {
            get => builtInConfigId;
            set
            {
                builtInConfigId = value;
                OnPropertyChanged();
            }
        }

        private string installDir;
        public string InstallDir
        {
            get => installDir;
            set
            {
                installDir = value;
                OnPropertyChanged();
            }
        }

        private ObservableCollection<BuiltInEmulatorProfile> builtinProfiles;
        /// <summary>
        /// Gets or sets list of emulator profiles.
        /// </summary>
        public ObservableCollection<BuiltInEmulatorProfile> BuiltinProfiles
        {
            get => builtinProfiles;
            set
            {
                if (builtinProfiles != null)
                {
                    builtinProfiles.CollectionChanged -= Profiles_CollectionChanged;
                }

                builtinProfiles = value;
                if (builtinProfiles != null)
                {
                    builtinProfiles.CollectionChanged += Profiles_CollectionChanged;
                }

                OnPropertyChanged();
                OnPropertyChanged(nameof(AllProfiles));
                OnPropertyChanged(nameof(SelectableProfiles));
            }
        }

        private ObservableCollection<CustomEmulatorProfile> customProfiles;
        /// <summary>
        /// Gets or sets list of emulator profiles.
        /// </summary>
        public ObservableCollection<CustomEmulatorProfile> CustomProfiles
        {
            get => customProfiles;
            set
            {
                if (customProfiles != null)
                {
                    customProfiles.CollectionChanged -= Profiles_CollectionChanged;
                }

                customProfiles = value;
                if (customProfiles != null)
                {
                    customProfiles.CollectionChanged += Profiles_CollectionChanged;
                }

                OnPropertyChanged();
                OnPropertyChanged(nameof(AllProfiles));
                OnPropertyChanged(nameof(SelectableProfiles));
            }
        }

        private void Profiles_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(nameof(AllProfiles));
            OnPropertyChanged(nameof(SelectableProfiles));
        }

        [DontSerialize]
        public List<EmulatorProfile> SelectableProfiles
        {
            get
            {
                var selProfiles = new List<EmulatorProfile>();
                if (BuiltinProfiles.HasItems())
                {
                    selProfiles.AddRange(BuiltinProfiles.OrderBy(a => a.Name));
                }

                if (CustomProfiles.HasItems())
                {
                    selProfiles.AddRange(CustomProfiles.OrderBy(a => a.Name));
                }

                selProfiles.Insert(0, new CustomEmulatorProfile { Id = null, Name = ResourceProvider.GetString("LOCGameActionSelectOnStart") });
                return selProfiles;
            }
        }

        [DontSerialize]
        public List<EmulatorProfile> AllProfiles
        {
            get
            {
                var selProfiles = new List<EmulatorProfile>();
                if (BuiltinProfiles.HasItems())
                {
                    selProfiles.AddRange(BuiltinProfiles.OrderBy(a => a.Name));
                }

                if (CustomProfiles.HasItems())
                {
                    selProfiles.AddRange(CustomProfiles.OrderBy(a => a.Name));
                }

                return selProfiles;
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

        public EmulatorProfile GetProfile(string profileId)
        {
            var cus = CustomProfiles?.FirstOrDefault(a => a.Id == profileId);
            if (cus != null)
            {
                return cus;
            }

            return BuiltinProfiles?.FirstOrDefault(a => a.Id == profileId);
        }

        /// <inheritdoc/>
        public override void CopyDiffTo(object target)
        {
            base.CopyDiffTo(target);

            if (target is Emulator tro)
            {
                if (!CustomProfiles.IsListEqualExact(tro.CustomProfiles))
                {
                    tro.CustomProfiles = CustomProfiles;
                }

                if (!BuiltinProfiles.IsListEqualExact(tro.BuiltinProfiles))
                {
                    tro.BuiltinProfiles = BuiltinProfiles;
                }

                if (!string.Equals(BuiltInConfigId, tro.BuiltInConfigId, StringComparison.Ordinal))
                {
                    tro.BuiltInConfigId = BuiltInConfigId;
                }

                if (!string.Equals(InstallDir, tro.InstallDir, StringComparison.Ordinal))
                {
                    tro.InstallDir = InstallDir;
                }
            }
            else
            {
                throw new ArgumentException($"Target object has to be of type {GetType().Name}");
            }
        }
    }
}
