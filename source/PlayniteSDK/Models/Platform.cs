using LiteDB;
using Newtonsoft.Json;
using Playnite.SDK.Converters;
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
    public class Platform : ObservableObject
    {
        private ObjectId id;
        /// <summary>
        /// Gets or sets platform database id.
        /// </summary>
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
        }

        /// <summary>
        /// Creates new instance of Platform with specific name.
        /// </summary>
        /// <param name="name">Platform name.</param>
        public Platform(string name)
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
