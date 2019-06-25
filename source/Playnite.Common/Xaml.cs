using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace Playnite.Common
{
    public class Xaml
    {
        public static object FromFile(string path)
        {
            using (var stream = new StreamReader(path))
            {
                return XamlReader.Load(stream.BaseStream);
            }
        }

        public static T FromFile<T>(string path)
        {
            using (var stream = new StreamReader(path))
            {
                return (T)XamlReader.Load(stream.BaseStream);
            }
        }

        public static T FromString<T>(string xaml)
        {
            return (T)XamlReader.Parse(xaml);
        }
    }
}
