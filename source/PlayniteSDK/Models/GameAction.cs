using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK.Models
{
    /// <summary>
    /// Represents game action type.
    /// </summary>
    public enum GameActionType : int
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
    public class GameAction : ObservableObject, IEquatable<GameAction>
    {
        private readonly Guid id = Guid.NewGuid();

        private GameActionType type;
        /// <summary>
        /// Gets or sets task type.
        /// </summary>
        public GameActionType Type
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
                case GameActionType.File:
                    return $"File: {Path}, {Arguments}, {WorkingDir}";
                case GameActionType.URL:
                    return $"Url: {Path}";
                case GameActionType.Emulator:
                    return $"Emulator: {EmulatorId}, {EmulatorProfileId}, {OverrideDefaultArgs}, {AdditionalArguments}";
                default:
                    return Path;
            }
        }

        /// <summary>
        /// Compares two <see cref="GameAction"/> objects for equality.
        /// </summary>
        /// <param name="obj1"></param>
        /// <param name="obj2"></param>
        /// <returns></returns>
        public static bool Equals(GameAction obj1, GameAction obj2)
        {
            if (obj1 == null && obj2 == null)
            {
                return true;
            }
            else
            {
                return obj1?.Equals(obj2) == true;
            }
        }

        /// <inheritdoc/>
        public bool Equals(GameAction other)
        {
            if (other is null)
            {
                return false;
            }

            if (Type != other.Type)
            {
                return false;
            }

            if (!string.Equals(Arguments, other.Arguments, StringComparison.Ordinal))
            {
                return false;
            }

            if (!string.Equals(AdditionalArguments, other.AdditionalArguments, StringComparison.Ordinal))
            {
                return false;
            }

            if (!string.Equals(Path, other.Path, StringComparison.Ordinal))
            {
                return false;
            }

            if (!string.Equals(WorkingDir, other.WorkingDir, StringComparison.Ordinal))
            {
                return false;
            }

            if (!string.Equals(Name, other.Name, StringComparison.Ordinal))
            {
                return false;
            }

            if (IsHandledByPlugin != other.IsHandledByPlugin)
            {
                return false;
            }

            if (EmulatorId != other.EmulatorId)
            {
                return false;
            }

            if (EmulatorProfileId != other.EmulatorProfileId)
            {
                return false;
            }

            if (OverrideDefaultArgs != other.OverrideDefaultArgs)
            {
                return false;
            }

            return true;
        }
    }
}
