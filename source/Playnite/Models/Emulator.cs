using LiteDB;
using Newtonsoft.Json;
using Playnite.Converters;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Models
{
    public class EmulatorProfile : ObservableObject
    {
        private ObjectId id;
        [JsonConverter(typeof(ObjectIdJsonConverter))]
        public ObjectId Id
        {
            get => id;
            set
            {
                id = value;
                OnPropertyChanged("Id");
            }
        }

        private string name;
        public string Name
        {
            get => name;
            set
            {
                name = value;
                OnPropertyChanged("Name");
            }
        }

        private List<ObjectId> platforms;
        public List<ObjectId> Platforms
        {
            get => platforms;
            set
            {
                platforms = value;
                OnPropertyChanged("Platforms");
            }
        }

        private List<string> imageExtensions;
        public List<string> ImageExtensions
        {
            get => imageExtensions;
            set
            {
                imageExtensions = value;
                OnPropertyChanged("ImageExtensions");
            }
        }

        private string executable;
        public string Executable
        {
            get => executable;
            set
            {
                executable = value;
                OnPropertyChanged("Executable");
            }
        }

        private string arguments;
        public string Arguments
        {
            get => arguments;
            set
            {
                arguments = value;
                OnPropertyChanged("Arguments");
            }
        }

        private string workingDirectory;
        public string WorkingDirectory
        {
            get => workingDirectory;
            set
            {
                workingDirectory = value;
                OnPropertyChanged("WorkingDirectory");
            }
        }

        public override string ToString()
        {
            return Name;
        }

        public EmulatorProfile()
        {
            Id = ObjectId.NewObjectId();
        }
    }

    public class Emulator : ObservableObject
    {
        private ObjectId id;
        [BsonId]
        [JsonConverter(typeof(ObjectIdJsonConverter))]
        public ObjectId Id
        {
            get => id;
            set
            {
                id = value;
                OnPropertyChanged("Id");
            }
        }

        private string name;
        public string Name
        {
            get => name;
            set
            {
                name = value;
                OnPropertyChanged("Name");
            }
        }

        private ObservableCollection<EmulatorProfile> profile;
        public ObservableCollection<EmulatorProfile> Profiles
        {
            get => profile;
            set
            {
                profile = value;
                OnPropertyChanged("Profiles");
            }
        }

        public Emulator()
        {
        }

        public Emulator(string name)
        {
            Name = name;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
