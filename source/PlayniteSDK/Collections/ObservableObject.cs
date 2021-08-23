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
        internal bool SuppressNotifications
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

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="property"></param>
        /// <param name="value"></param>
        /// <param name="propertyName"></param>
        protected void SetValue<T>(ref T property, T value, [CallerMemberName]string propertyName = null)
        {
            property = value;
            OnPropertyChanged(propertyName);
        }

        /// <summary>
        ///
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="property"></param>
        /// <param name="value"></param>
        /// <param name="propertyNames"></param>
        protected void SetValue<T>(ref T property, T value, params string[] propertyNames)
        {
            property = value;
            foreach (var pro in propertyNames)
            {
                OnPropertyChanged(pro);
            }
        }
    }
}
