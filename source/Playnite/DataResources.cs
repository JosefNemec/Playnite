using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Playnite
{
    public class DataResources
    {
        public static string ReadFileFromResource(string resource)
        {
            using (var stream = Assembly.GetCallingAssembly().GetManifestResourceStream(resource))
            {
                var tr = new StreamReader(stream);
                return tr.ReadToEnd();
            }
        }
    }
}
