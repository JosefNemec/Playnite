using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK.Models
{
    /// <summary>
    /// Represents web link.
    /// </summary>
    public class Link : ObservableObject
    {
        private string name;
        /// <summary>
        /// Gets or sets name of the link.
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

        private string url;
        /// <summary>
        /// Gets or sets web based URL.
        /// </summary>
        public string Url
        {
            get => url;
            set
            {
                url = value;
                OnPropertyChanged("Url");
            }
        }

        /// <summary>
        /// Creates new instance of Link.
        /// </summary>
        public Link()
        {
        }

        /// <summary>
        /// Creates new instance of Link with specific values.
        /// </summary>
        /// <param name="name">Link name.</param>
        /// <param name="url">Link URL.</param>
        public Link(string name, string url)
        {
            Name = name;
            Url = url;
        }
    }
}
