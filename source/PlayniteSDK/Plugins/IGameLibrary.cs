using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Playnite.SDK.Plugins
{
    public interface IGameLibrary : IPlugin
    {
        UserControl SettingsView { get; }

        IEditableObject Settings { get; }

        string Name { get; }
    }
}
