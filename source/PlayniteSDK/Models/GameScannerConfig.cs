using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK.Models
{
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
        /// Gets or sets value indicating whether this configu should be included in global library update.
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
        /// Gets or sets value indicating whether game roms should be imported under relative paths if possible.
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
            }
            else
            {
                throw new ArgumentException($"Target object has to be of type {GetType().Name}");
            }
        }
    }
}
