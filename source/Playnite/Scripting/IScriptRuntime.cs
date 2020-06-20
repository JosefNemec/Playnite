using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Scripting
{
    public interface IScriptRuntime : IDisposable
    {
        object Execute(string script, string workDir = null);

        object ExecuteFile(string scriptPath, string workDir = null);

        object Execute(string script, Dictionary<string, object> variables, string workDir = null);
    }
}
