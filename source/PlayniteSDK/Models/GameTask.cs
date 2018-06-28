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
    /// Represents game task type.
    /// </summary>
    public enum GameTaskType : int
    {
        /// <summary>
        /// Game task executes a file.
        /// </summary>
        File = 0,
        /// <summary>
        /// Game task navigates to a web based URL.
        /// </summary>
        URL = 1,
        /// <summary>
        /// Game task starts an emulator.
        /// </summary>
        Emulator = 2
    }

    /// <summary>
    /// Represents executable game task.
    /// </summary>
    public class GameTask : ObservableObject
    {
        private GameTaskType type;
        /// <summary>
        /// Gets or sets task type.
        /// </summary>
        public GameTaskType Type
        {
            get => type;
            set
            {
                type = value;
                OnPropertyChanged("Type");
            }
        }

        private bool isPrimary;
        /// <summary>
        /// Gets or sets value indicating wheter a task is used to launch a game.
        /// Used only during game import to generate Play task.
        /// </summary>
        public bool IsPrimary
        {
            get => isPrimary;
            set
            {
                isPrimary = value;
                OnPropertyChanged("IsPrimary");
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
        /// Gets or sets additional executable arguments used for Emulator task type.
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
        /// Gets or sets value indicating wheter emulator arguments should be completely overwritten with task arguments.
        /// Applies only to Emulator task type.
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
        /// Gets or sets executable path for File task type or URL for URL task type.
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
        /// Gets or sets working directory for File task type executable.
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
        /// Gets or sets task name.
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

        private bool isBuiltIn;
        /// <summary>
        /// Gets or sets value indicating wheter a task is from original library provider.
        /// </summary>        
        public bool IsBuiltIn
        {
            get => isBuiltIn;
            set
            {
                isBuiltIn = value;
                OnPropertyChanged("IsBuiltIn");
            }
        }

        private ObjectId emulatorId;
        /// <summary>
        /// Gets or sets emulator id for Emulator task type execution.
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
        /// Gets or sets emulator profile id for Emulator task type execution.
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
                case GameTaskType.File:
                    return $"File: {Path}, {Arguments}, {WorkingDir}";
                case GameTaskType.URL:
                    return $"Url: {Path}";
                case GameTaskType.Emulator:
                    return $"Emulator: {EmulatorId}, {EmulatorProfileId}, {OverrideDefaultArgs}, {AdditionalArguments}";
                default:
                    return Path;
            }
        }
    }
}
