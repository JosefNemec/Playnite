using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Playnite.SDK.Plugins
{
    /// <summary>
    /// Describes Playnite plugin base.
    /// </summary>
    public interface IPlugin : IDisposable, IIdentifiable
    {
        /// <summary>
        /// Gets plugin settings view or null if plugin doesn't provide settings view.
        /// </summary>
        UserControl GetSettingsView(bool firstRunView);

        /// <summary>
        /// Gets plugin settings or null if plugin doesn't provide any settings.
        /// </summary>
        ISettings GetSettings(bool firstRunSettings);        
    }
}
