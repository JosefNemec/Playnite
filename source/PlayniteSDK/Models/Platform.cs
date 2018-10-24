using LiteDB;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK.Models
{
    /// <summary>
    /// Represents game's platfom.
    /// </summary>
    public class Platform : DatabaseObject
    {
        private string name;
        /// <summary>
        /// Gets or sets platform name.
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

        private string icon;
        /// <summary>
        /// Gets or sets platform icon.
        /// </summary>
        public string Icon
        {
            get => icon;
            set
            {
                icon = value;
                OnPropertyChanged("Icon");
            }
        }

        private string cover;
        /// <summary>
        /// Gets or sets default game cover.
        /// </summary>
        public string Cover
        {
            get => cover;
            set
            {
                cover = value;
                OnPropertyChanged("Cover");
            }
        }

        /// <summary>
        /// Creates new instance of Platform.
        /// </summary>
        public Platform()
        {
            Id = Guid.NewGuid();
        }

        /// <summary>
        /// Creates new instance of Platform with specific name.
        /// </summary>
        /// <param name="name">Platform name.</param>
        public Platform(string name) : this()
        {
            Name = name;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {            
            return Name;
        }
    }
}
