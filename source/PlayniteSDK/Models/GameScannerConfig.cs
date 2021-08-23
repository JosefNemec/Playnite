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
            }
            else
            {
                throw new ArgumentException($"Target object has to be of type {GetType().Name}");
            }
        }
    }
}
