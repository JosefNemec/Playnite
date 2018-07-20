using LiteDB;
using Newtonsoft.Json;
using Playnite.SDK.Converters;
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
    public class GameAction : ObservableObject
    {
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
                OnPropertyChanged("Type");
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
                OnPropertyChanged("Arguments");
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
                OnPropertyChanged("AdditionalArguments");
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
                OnPropertyChanged("OverrideDefaultArgs");
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
                OnPropertyChanged("Path");
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
                OnPropertyChanged("WorkingDir");
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
                OnPropertyChanged("Name");
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
                OnPropertyChanged("IsHandledByPlugin");
            }
        }

        private ObjectId emulatorId;
        /// <summary>
        /// Gets or sets emulator id for Emulator action type execution.
        /// </summary>
        [JsonConverter(typeof(ObjectIdJsonConverter))]       
        public ObjectId EmulatorId
        {
            get => emulatorId;
            set
            {
                emulatorId = value;
                OnPropertyChanged("EmulatorId");
            }
        }

        private ObjectId emulatorProfileId;
        /// <summary>
        /// Gets or sets emulator profile id for Emulator action type execution.
        /// </summary>
        [JsonConverter(typeof(ObjectIdJsonConverter))]
        public ObjectId EmulatorProfileId
        {
            get => emulatorProfileId;
            set
            {
                emulatorProfileId = value;
                OnPropertyChanged("EmulatorProfileId");
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
    }
}
