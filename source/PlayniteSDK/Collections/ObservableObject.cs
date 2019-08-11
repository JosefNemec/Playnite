using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace System.Collections.Generic
{
    /// <summary>
    /// Represents object implementing INotifyPropertyChanged.
    /// </summary>
    public abstract class ObservableObject : INotifyPropertyChanged
    {
        /// <summary>
        /// If set to <c>true</c> no <see cref="PropertyChanged"/> events will be fired.
        /// </summary>
        [JsonIgnore]
        public bool SuppressNotifications
        {
            get; set;
        } = false;

        /// <summary>
        /// Occurs when a property value changes
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Invokes PropertyChanged events.
        /// </summary>
        /// <param name="name">Name of property that changed.</param>
        public void OnPropertyChanged([CallerMemberName]string name = null)
        {
            if (!SuppressNotifications)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }
        }
    }
}
