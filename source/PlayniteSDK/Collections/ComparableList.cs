using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace System.Collections.Generic
{
    /// <summary>
    /// Represent comparable database item collection.
    /// </summary>
    /// <typeparam name="T">Database object type.</typeparam>
    public class ComparableDbItemList<T> : List<T>, IComparable where T : DatabaseObject
    {
        /// <summary>
        /// Creates new instance of ComparableDbItemList.
        /// </summary>
        /// <param name="collection">Intial collection.</param>
        public ComparableDbItemList(IEnumerable<T> collection) : base(collection ?? new List<T>())
        {
        }

        /// <inheritdoc/>
        public int CompareTo(object obj)
        {
            if (obj == null)
            {
                return -1;
            }

            var list2 = (List<T>)obj;
            if (Count == 0 && list2.Count == 0)
            {
                return 0;
            }

            if (list2.Any() == false && Count > 0)
            {
                return -1;
            }

            if (this.Any() == false && list2.Count > 0)
            {
                return 1;
            }

            var str1 = string.Join(", ", this);
            var str2 = string.Join(", ", list2);
            return str1.CompareTo(str2);
        }
    }

    /// <summary>
    /// Highly unoptimized, should not be used anywhere anymore in new code.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ComparableList<T>: List<T>, IComparable
    {
        /// <summary>
        ///
        /// </summary>
        public ComparableList()
        {
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="count"></param>
        public ComparableList(int count) : base(count)
        {
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="collection"></param>
        public ComparableList(IEnumerable<T> collection) : base(collection ?? new List<T>())
        {
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public int CompareTo(object obj)
        {
            if (obj == null)
            {
                return -1;
            }

            var list2 = obj as List<T>;
            var str1 = string.Join(", ", this);
            var str2 = string.Join(", ", list2);
            return str1.CompareTo(str2);
        }
    }
}
