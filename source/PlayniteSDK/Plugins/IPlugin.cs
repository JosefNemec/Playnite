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
    public interface IPlugin : IDisposable
    {
        /// <summary>
        /// Gets plugin settings view or null if plugin doesn't provide settings view.
        /// </summary>
        UserControl SettingsView { get; }

        /// <summary>
        /// Gets plugin settings or null if plugin doesn't provide any settings.
        /// </summary>
        ISettings Settings { get; }

        /// <summary>
        /// Gets unique plugin id.
        /// </summary>
        Guid Id { get; }
    }
}
