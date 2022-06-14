using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Collections.Generic
{
    /// <summary>
    ///
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class RangeObservableCollection<T> : ObservableCollection<T>
    {
        private bool suppressNotification = false;

        /// <summary>
        ///
        /// </summary>
        /// <param name="e"></param>
        public void OnCollectionChangedPublic(NotifyCollectionChangedEventArgs e)
        {
            OnCollectionChanged(e);
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="e"></param>
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            if (!suppressNotification)
            {
                base.OnCollectionChanged(e);
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="obj"></param>
        /// <param name="index"></param>
        /// <param name="oldIndex"></param>
        public void OnItemMoved(object obj, int index, int oldIndex)
        {
            if (!suppressNotification)
            {
                base.OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, obj, index, oldIndex));
            }
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="list"></param>
        public void AddRange(IEnumerable<T> list)
        {
            if (list == null)
            {
                return;
            }

            suppressNotification = true;
            foreach (T item in list)
            {
                Add(item);
            }

            suppressNotification = false;
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="list"></param>
        public void RemoveRange(IEnumerable<T> list)
        {
            if (list == null)
            {
                return;
            }

            suppressNotification = true;
            foreach (T item in list)
            {
                Remove(item);
            }

            suppressNotification = false;
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }
    }
}
