using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace System.Collections.Generic
{
    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ComparableList<T>: List<T>, IComparable, IEnumerable<T>
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
            var str1 = string.Join(", ", ToArray());
            var str2 = string.Join(", ", list2.ToArray());

            return str1.CompareTo(str2);
        }
    }
}
