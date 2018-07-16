using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK.Plugins
{
    public interface IGameLibrary : IDisposable
    {
        IEditableObject Settings { get; }

        Guid LibraryId { get; }
    }
}
