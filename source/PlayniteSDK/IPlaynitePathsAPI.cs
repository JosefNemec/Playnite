using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK
{
    public interface IPlaynitePathsAPI
    {
        bool IsPortable { get; }
        string ApplicationPath { get; }
        string ConfigurationPath { get; }
    }
}
