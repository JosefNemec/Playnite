using LiteDB;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Models.Old1
{
    public class Emulator : INotifyPropertyChanged
    {
        private int id;
        [BsonId]
        public int Id
        {
            get => id;
            set
            {
                id = value;
                OnPropertyChanged("Id");
            }
        }

        private List<int> platforms;
        public List<int> Platforms
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

        public event PropertyChangedEventHandler PropertyChanged;

        public Emulator()
        {
        }

        public Emulator(string name)
        {
            Name = name;
        }

        public void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
