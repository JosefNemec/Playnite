using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK
{
    /// <summary>
    /// Represents object with definitions of available expandable game variables.
    /// </summary>
    public class ExpandableVariables
    {
        /// <summary>
        /// Gets variable name for game's installation directory path.
        /// </summary>
        public const string InstallationDirectory = "{InstallDir}";

        /// <summary>
        /// Gets variable name for game's installation directory name.
        /// </summary>
        public const string InstallationDirName = "{InstallDirName}";

        /// <summary>
        /// Gets variable name for game's image path.
        /// </summary>
        public const string ImagePath = "{ImagePath}";

        /// <summary>
        /// Gets variable name for game's image file name without extension.
        /// </summary>
        public const string ImageNameNoExtension = "{ImageNameNoExt}";

        /// <summary>
        /// Gets variable name for game's image file name.
        /// </summary>
        public const string ImageName = "{ImageName}";

        /// <summary>
        /// Gets variable name for Playnite's installation directory path.
        /// </summary>
        public const string PlayniteDirectory = "{PlayniteDir}";

        /// <summary>
        /// Gets variable for game's name.
        /// </summary>
        public const string Name = "{Name}";

        /// <summary>
        /// Gets variable for game's platform.
        /// </summary>
        public const string Platform = "{Platform}";

        /// <summary>
        /// Gets variable for game's ID.
        /// </summary>
        public const string GameId = "{GameId}";

        /// <summary>
        /// Gets variable for game's database ID.
        /// </summary>
        public const string DatabaseId = "{DatabaseId}";

        /// <summary>
        /// Gets variable for game's library plugin ID.
        /// </summary>
        public const string PluginId = "{PluginId}";

        /// <summary>
        /// Gets variable for game's version.
        /// </summary>
        public const string Version = "{Version}";
    }
}
