using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ItchioLibrary.Models
{
    /// <summary>
    /// Ask the client to perform a shell launch, ie. open an item with the operating system’s default handler (File explorer).
    /// Sent during Launch.
    /// </summary>
    public class ShellLaunch
    {
        /// <summary>
        /// Absolute path of item to open, e.g. D:\\Games\\Itch\\garden\\README.txt
        /// </summary>
        public string itemPath;
    }
}
