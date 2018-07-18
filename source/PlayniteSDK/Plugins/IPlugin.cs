using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK.Plugins
{
    public interface IPlugin : IDisposable
    {

        Guid Id { get; }
    }
}
