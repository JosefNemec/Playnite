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
    /// <summary>
    ///
    /// </summary>
    public class EmulatorDefinitionProfile
    {
        /// <summary>
        ///
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///
        /// </summary>
        public List<string> Platforms { get; set; }

        /// <summary>
        ///
        /// </summary>
        public List<string> ImageExtensions { get; set; }

        /// <summary>
        ///
        /// </summary>
        public List<string> ProfileFiles { get; set; }

        /// <summary>
        ///
        /// </summary>
        public string InstallationFile { get; set; }

        /// <summary>
        ///
        /// </summary>
        public string StartupArguments { get; set; }

        /// <summary>
        ///
        /// </summary>
        public string StartupExecutable { get; set; }

        /// <summary>
        ///
        /// </summary>
        public bool ScriptStartup { get; set; }

        /// <summary>
        ///
        /// </summary>
        public bool ScriptGameImport { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Name;
        }
    }

    /// <summary>
    ///
    /// </summary>
    public class EmulatorDefinition
    {
        internal string DirectoryName { get; set; }

        /// <summary>
        ///
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        ///
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///
        /// </summary>
        public string Website { get; set; }

        /// <summary>
        ///
        /// </summary>
        public List<EmulatorDefinitionProfile> Profiles { get; set; }

        /// <inheritdoc/>
        public override string ToString()
        {
            return Name;
        }
    }

    /// <summary>
    /// Represents built-in region definition.
    /// </summary>
    public class EmulatedRegion : IEquatable<EmulatedRegion>
    {
        /// <summary>
        /// Gets region id.
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Gets region name.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Gets value indicating whether the region should be imported into new libraries.
        /// </summary>
        public bool DefaultImport { get; set; }
        /// <summary>
        /// Gets ID of the region on IGDB database.
        /// </summary>
        public ulong IgdbId { get; set; }
        /// <summary>
        /// Gets region codes.
        /// </summary>
        public List<string> Codes { get; set; }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return Id?.GetHashCode() ?? 0;
        }

        /// <inheritdoc/>
        public bool Equals(EmulatedRegion other)
        {
            return other.Id == Id;
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public override string ToString()
        {
            return Name;
        }
    }

    /// <summary>
    /// Represents built-in platform definition.
    /// </summary>
    public class EmulatedPlatform : IEquatable<EmulatedPlatform>
    {
        /// <summary>
        /// Gets ID of the platform on IGDB database.
        /// </summary>
        public ulong IgdbId { get; set; }
        /// <summary>
        /// Gets platform name.
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// Gets platform id.
        /// </summary>
        public string Id { get; set; }
        /// <summary>
        /// Gets list of platform ROM database ids.
        /// </summary>
        public List<string> Databases { get; set; }
        /// <summary>
        /// Gets list of emulator IDs supporting this platform.
        /// </summary>
        public List<string> Emulators { get; set; }

        /// <inheritdoc/>
        public override int GetHashCode()
        {
            return Id?.GetHashCode() ?? 0;
        }

        /// <inheritdoc/>
        public bool Equals(EmulatedPlatform other)
        {
            return other.Id == Id;
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public override string ToString()
        {
            return Name;
        }
    }

    /// <summary>
    ///
    /// </summary>
    public abstract class EmulatorProfile : ObservableObject
    {
        private string id;
        /// <summary>
        /// Gets emulator profile ID.
        /// </summary>
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
        /// <summary>
        /// Gets profile name.
        /// </summary>
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
        /// <summary>
        /// Gets pre-execution script.
        /// </summary>
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
        /// <summary>
        /// Gets post-execution script.
        /// </summary>
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
        /// <summary>
        /// Gets exit-execution script.
        /// </summary>
        public string ExitScript
        {
            get => exitScript;
            set
            {
                exitScript = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Gets emulator profile object type.
        /// </summary>
        [DontSerialize]
        public Type Type => GetType();

        /// <inheritdoc/>
        public override string ToString()
        {
            return Name;
        }
    }

    /// <summary>
    /// Represents built-in emulator profile.
    /// </summary>
    public class BuiltInEmulatorProfile : EmulatorProfile, IEquatable<BuiltInEmulatorProfile>
    {
        internal static readonly string ProfilePrefix = "#builtin_";

        private string builtInProfileName;
        /// <summary>
        /// Gets name of built-in profile represented by this definition.
        /// </summary>
        public string BuiltInProfileName
        {
            get => builtInProfileName;
            set
            {
                builtInProfileName = value;
                OnPropertyChanged();
            }
        }

        private bool overrideDefaultArgs;
        /// <summary>
        /// Gets or sets value indicating whether built-in arguments should be overriden.
        /// </summary>
        public bool OverrideDefaultArgs
        {
            get => overrideDefaultArgs;
            set
            {
                overrideDefaultArgs = value;
                OnPropertyChanged();
            }
        }

        private string customArguments;
        /// <summary>
        /// Gets or set custom emulator arguments.
        /// </summary>
        public string CustomArguments
        {
            get => customArguments;
            set
            {
                customArguments = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Creates new instance of <see cref="BuiltInEmulatorProfile"/>.
        /// </summary>
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

            if (!string.Equals(CustomArguments, other.CustomArguments, StringComparison.Ordinal))
            {
                return false;
            }

            if (OverrideDefaultArgs != other.OverrideDefaultArgs)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public BuiltInEmulatorProfile GetCopy()
        {
            return new BuiltInEmulatorProfile
            {
                Id = Id,
                BuiltInProfileName = BuiltInProfileName,
                Name = Name,
                ExitScript = ExitScript,
                PostScript = PostScript,
                PreScript = PreScript,
                CustomArguments = CustomArguments,
                OverrideDefaultArgs = OverrideDefaultArgs,
            };
        }
    }

    /// <summary>
    /// Represents emulator profile.
    /// </summary>
    public class CustomEmulatorProfile : EmulatorProfile, IEquatable<CustomEmulatorProfile>
    {
        internal static readonly string ProfilePrefix = "#custom_";

        private string startupScript;
        /// <summary>
        /// Gets startup script.
        /// </summary>
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

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public CustomEmulatorProfile GetCopy()
        {
            return new CustomEmulatorProfile
            {
                Id = Id,
                Name = Name,
                Platforms = Platforms?.ToList(),
                ImageExtensions = ImageExtensions?.ToList(),
                Executable = Executable,
                Arguments = Arguments,
                WorkingDirectory = WorkingDirectory,
                ExitScript = ExitScript,
                PostScript = PostScript,
                PreScript = PreScript,
                StartupScript = StartupScript
            };
        }
    }

    /// <summary>
    /// Represents system emulator.
    /// </summary>
    public class Emulator : DatabaseObject
    {
        private string builtInConfigId;
        /// <summary>
        /// Gets id of built-in emulator profile.
        /// </summary>
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
        /// <summary>
        /// Gets emulator installation directory.
        /// </summary>
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

        /// <summary>
        /// Gets list of all profiles including option for profile auto-select.
        /// </summary>
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

        /// <summary>
        /// Gets list of all profiles.
        /// </summary>
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

        /// <summary>
        /// Gets profile by id.
        /// </summary>
        /// <param name="profileId"></param>
        /// <returns></returns>
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

        /// <summary>
        ///
        /// </summary>
        /// <returns></returns>
        public Emulator GetCopy()
        {
            return new Emulator
            {
                Id = Id,
                Name = Name,
                CustomProfiles = CustomProfiles?.Select(a => a.GetCopy()).ToObservable(),
                BuiltinProfiles = BuiltinProfiles?.Select(a => a.GetCopy()).ToObservable(),
                BuiltInConfigId = BuiltInConfigId,
                InstallDir = InstallDir
            };
        }
    }
}
