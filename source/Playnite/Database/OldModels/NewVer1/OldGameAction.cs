using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Database.OldModels.NewVer1
{
    /// <summary>
    /// Represents game action type.
    /// </summary>
    public enum OldGameActionType : int
    {
        /// <summary>
        /// Game action executes a file.
        /// </summary>
        File = 0,
        /// <summary>
        /// Game action navigates to a web based URL.
        /// </summary>
        URL = 1,
        /// <summary>
        /// Game action starts an emulator.
        /// </summary>
        Emulator = 2
    }

    /// <summary>
    /// Represents executable game action.
    /// </summary>
    public class OldGameAction : ObservableObject
    {
        private OldGameActionType type;
        /// <summary>
        /// Gets or sets task type.
        /// </summary>
        public OldGameActionType Type
        {
            get => type;
            set
            {
                type = value;
                OnPropertyChanged();
            }
        }

        private string arguments;
        /// <summary>
        /// Gets or sets executable arguments for File type tasks.
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

        private string additionalArguments;
        /// <summary>
        /// Gets or sets additional executable arguments used for Emulator action type.
        /// </summary>
        public string AdditionalArguments
        {
            get => additionalArguments;
            set
            {
                additionalArguments = value;
                OnPropertyChanged();
            }
        }

        private bool overrideDefaultArgs;
        /// <summary>
        /// Gets or sets value indicating wheter emulator arguments should be completely overwritten with action arguments.
        /// Applies only to Emulator action type.
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

        private string path;
        /// <summary>
        /// Gets or sets executable path for File action type or URL for URL action type.
        /// </summary>
        public string Path
        {
            get => path;
            set
            {
                path = value;
                OnPropertyChanged();
            }
        }

        private string workingDir;
        /// <summary>
        /// Gets or sets working directory for File action type executable.
        /// </summary>
        public string WorkingDir
        {
            get => workingDir;
            set
            {
                workingDir = value;
                OnPropertyChanged();
            }
        }

        private string name;
        /// <summary>
        /// Gets or sets action name.
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

        private bool isHandledByPlugin;
        /// <summary>
        /// Gets or sets value indicating wheter a action's execution should be handled by a plugin.
        /// </summary>        
        public bool IsHandledByPlugin
        {
            get => isHandledByPlugin;
            set
            {
                isHandledByPlugin = value;
                OnPropertyChanged();
            }
        }

        private Guid emulatorId;
        /// <summary>
        /// Gets or sets emulator id for Emulator action type execution.
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

        private Guid emulatorProfileId;
        /// <summary>
        /// Gets or sets emulator profile id for Emulator action type execution.
        /// </summary>
        public Guid EmulatorProfileId
        {
            get => emulatorProfileId;
            set
            {
                emulatorProfileId = value;
                OnPropertyChanged();
            }
        }
        
        /// <inheritdoc/>
        public override string ToString()
        {
            switch (Type)
            {
                case OldGameActionType.File:
                    return $"File: {Path}, {Arguments}, {WorkingDir}";
                case OldGameActionType.URL:
                    return $"Url: {Path}";
                case OldGameActionType.Emulator:
                    return $"Emulator: {EmulatorId}, {EmulatorProfileId}, {OverrideDefaultArgs}, {AdditionalArguments}";
                default:
                    return Path;
            }
        }
    }
}
