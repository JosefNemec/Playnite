using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Playnite.SDK.Plugins
{
    public interface IPlugin : IDisposable
    {
        UserControl SettingsView { get; }

        ISettings Settings { get; }

        Guid Id { get; }
    }
}
