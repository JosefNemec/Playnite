using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace Playnite
{
    public class ComparableList<T>: List<T>, IComparable, IEnumerable<T>
    {
        public ComparableList()
        {
        }

        public ComparableList(int count) : base(count)
        {
        }

        public ComparableList(IEnumerable<T> collection) : base(collection ?? new List<T>())
        {
        }

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
