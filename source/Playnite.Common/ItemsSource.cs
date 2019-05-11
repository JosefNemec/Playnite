using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Common
{
    public class ItemsSource
    {
        public class EnumItem
        {
            public string Name { get; set; }
            public Enum Value { get; set; }

            public EnumItem(string name, Enum value)
            {
                Name = name;
                Value = value;
            }
        }

        public static IEnumerable<EnumItem> GetEnumSources(Type enumType)
        {
            foreach (Enum type in Enum.GetValues(enumType))
            {
                yield return new EnumItem(type.GetDescription(), type);
            }
        }
    }
}
