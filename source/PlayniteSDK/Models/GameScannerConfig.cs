using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK.Models
{
    public class GameScannerConfig : DatabaseObject
    {
        private Guid emulatorId;
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
        public bool InGlobalUpdate
        {
            get => inGlobalUpdate;
            set
            {
                inGlobalUpdate = value;
                OnPropertyChanged();
            }
        }

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
