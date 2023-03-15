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
    public enum ScannerConfigPlayActionSettings
    {
        /// <summary>
        ///
        /// </summary>
        [Description("LOCScannerConfigPlayActionSettingsScanner")]
        ScannerSettings,
        /// <summary>
        ///
        /// </summary>
        [Description("LOCScannerConfigPlayActionSettingsSelectProfile")]
        SelectProfiteOnStart,
        /// <summary>
        ///
        /// </summary>
        [Description("LOCScannerConfigPlayActionSettingsSelectEmulator")]
        SelectEmulatorOnStart
    }

    /// <summary>
    /// Represents emulated game scanner configuration.
    /// </summary>
    public class GameScannerConfig : DatabaseObject
    {
        private Guid emulatorId;
        /// <summary>
        /// Gets or sets assigned emulator id.
        /// </summary>
        public Guid EmulatorId
        {
            get => emulatorId;
            set
            {
                emulatorId = value;
                OnPropertyChanged();
            }
        }

        private List<string> crcExcludeFileTypes;
        /// <summary>
        /// Gets or sets list of file extensions that should be excluded from CRC check.
        /// </summary>
        public List<string> CrcExcludeFileTypes
        {
            get => crcExcludeFileTypes;
            set
            {
                crcExcludeFileTypes = value;
                OnPropertyChanged();
            }
        }

        private string emulatorProfileId;
        /// <summary>
        /// Gets or sets assigned emulator profile id.
        /// </summary>
        public string EmulatorProfileId
        {
            get => emulatorProfileId;
            set
            {
                emulatorProfileId = value;
                OnPropertyChanged();
            }
        }

        private string directory;
        /// <summary>
        /// Gets or sets directory to scan.
        /// </summary>
        public string Directory
        {
            get => directory;
            set
            {
                directory = value;
                OnPropertyChanged();
            }
        }

        private bool inGlobalUpdate = true;
        /// <summary>
        /// Gets or sets value indicating whether this config should be included in global library update.
        /// </summary>
        public bool InGlobalUpdate
        {
            get => inGlobalUpdate;
            set
            {
                inGlobalUpdate = value;
                OnPropertyChanged();
            }
        }

        private bool excludeOnlineFiles = false;
        /// <summary>
        /// Gets or sets value indicating whether from cloud storage services should be scanned if not downloaded to a device.
        /// </summary>
        public bool ExcludeOnlineFiles
        {
            get => excludeOnlineFiles;
            set
            {
                excludeOnlineFiles = value;
                OnPropertyChanged();
            }
        }

        private bool useSimplifiedOnlineFileScan = false;
        /// <summary>
        /// Gets or sets value indicating whether online only cloud files should still be scanned without reading a file.
        /// </summary>
        public bool UseSimplifiedOnlineFileScan
        {
            get => useSimplifiedOnlineFileScan;
            set
            {
                useSimplifiedOnlineFileScan = value;
                OnPropertyChanged();
            }
        }

        private bool importWithRelativePaths = true;
        /// <summary>
        /// Gets or sets value indicating whether game ROMs should be imported under relative paths if possible.
        /// </summary>
        public bool ImportWithRelativePaths
        {
            get => importWithRelativePaths;
            set
            {
                importWithRelativePaths = value;
                OnPropertyChanged();
            }
        }

        private bool scanSubfolders = true;
        /// <summary>
        /// Gets or sets value indicating whether subfolders should be scanned.
        /// </summary>
        public bool ScanSubfolders
        {
            get => scanSubfolders;
            set
            {
                scanSubfolders = value;
                OnPropertyChanged();
            }
        }

        private bool scanInsideArchives = true;
        /// <summary>
        /// Gets or sets value indicating whether file archives should be scanned for content.
        /// </summary>
        public bool ScanInsideArchives
        {
            get => scanInsideArchives;
            set
            {
                scanInsideArchives = value;
                OnPropertyChanged();
            }
        }

        private List<string> excludedFiles;
        /// <summary>
        /// Gets or sets list of files excluded from scan.
        /// </summary>
        public List<string> ExcludedFiles
        {
            get => excludedFiles;
            set
            {
                excludedFiles = value;
                OnPropertyChanged();
            }
        }

        private List<string> excludedDirectories;
        /// <summary>
        /// Gets or sets list of folders excluded from scan.
        /// </summary>
        public List<string> ExcludedDirectories
        {
            get => excludedDirectories;
            set
            {
                excludedDirectories = value;
                OnPropertyChanged();
            }
        }

        private Guid overridePlatformId;
        /// <summary>
        /// Gets or sets id of default platform to be assigned if auto detection fails.
        /// </summary>
        public Guid OverridePlatformId
        {
            get => overridePlatformId;
            set
            {
                overridePlatformId = value;
                OnPropertyChanged();
            }
        }

        private ScannerConfigPlayActionSettings playActionSettings = ScannerConfigPlayActionSettings.ScannerSettings;
        /// <summary>
        /// Gets or sets play action settings for imported games.
        /// </summary>
        public ScannerConfigPlayActionSettings PlayActionSettings
        {
            get => playActionSettings;
            set
            {
                playActionSettings = value;
                OnPropertyChanged();
            }
        }

        private bool mergeRelatedFiles = true;
        /// <summary>
        /// Gets or sets value indicating whether related files, like individual game discs, should be merged under one game entry.
        /// </summary>
        public bool MergeRelatedFiles
        {
            get => mergeRelatedFiles;
            set
            {
                mergeRelatedFiles = value;
                OnPropertyChanged();
            }
        }

        /// <inheritdoc/>
        public override void CopyDiffTo(object target)
        {
            base.CopyDiffTo(target);
            if (target is GameScannerConfig tro)
            {
                if (EmulatorId != tro.EmulatorId)
                {
                    tro.EmulatorId = EmulatorId;
                }

                if (InGlobalUpdate != tro.InGlobalUpdate)
                {
                    tro.InGlobalUpdate = InGlobalUpdate;
                }

                if (!string.Equals(EmulatorProfileId, tro.EmulatorProfileId, StringComparison.Ordinal))
                {
                    tro.EmulatorProfileId = EmulatorProfileId;
                }

                if (!string.Equals(Directory, tro.Directory, StringComparison.Ordinal))
                {
                    tro.Directory = Directory;
                }

                if (!CrcExcludeFileTypes.IsListEqual(tro.CrcExcludeFileTypes))
                {
                    tro.CrcExcludeFileTypes = CrcExcludeFileTypes;
                }

                if (ExcludeOnlineFiles != tro.ExcludeOnlineFiles)
                {
                    tro.ExcludeOnlineFiles = ExcludeOnlineFiles;
                }

                if (UseSimplifiedOnlineFileScan != tro.UseSimplifiedOnlineFileScan)
                {
                    tro.UseSimplifiedOnlineFileScan = UseSimplifiedOnlineFileScan;
                }

                if (ImportWithRelativePaths != tro.ImportWithRelativePaths)
                {
                    tro.ImportWithRelativePaths = ImportWithRelativePaths;
                }

                if (ScanSubfolders != tro.ScanSubfolders)
                {
                    tro.ScanSubfolders = ScanSubfolders;
                }

                if (ScanInsideArchives != tro.ScanInsideArchives)
                {
                    tro.ScanInsideArchives = ScanInsideArchives;
                }

                if (!ExcludedFiles.IsListEqual(tro.ExcludedFiles))
                {
                    tro.ExcludedFiles = ExcludedFiles;
                }

                if (!ExcludedDirectories.IsListEqual(tro.ExcludedDirectories))
                {
                    tro.ExcludedDirectories = ExcludedDirectories;
                }

                if (OverridePlatformId != tro.OverridePlatformId)
                {
                    tro.OverridePlatformId = OverridePlatformId;
                }

                if (PlayActionSettings != tro.PlayActionSettings)
                {
                    tro.PlayActionSettings = PlayActionSettings;
                }

                if (MergeRelatedFiles != tro.MergeRelatedFiles)
                {
                    tro.MergeRelatedFiles = MergeRelatedFiles;
                }
            }
            else
            {
                throw new ArgumentException($"Target object has to be of type {GetType().Name}");
            }
        }
    }
}
