using Playnite.Models;
using Playnite.SDK.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Scripting.PowerShell
{
    public class PowerShellScript : PlayniteScript
    {
        public PowerShellRuntime Runtime
        {
            get; private set;
        }

        public PowerShellScript(string path) : base(path, ScriptLanguage.PowerShell)
        {
            Runtime = new PowerShellRuntime();
            Runtime.ExecuteFile(path);
            var attributes = Runtime.GetVariable("__attributes");
            if (attributes != null)
            {
                Attributes = ((Hashtable)attributes).Cast<DictionaryEntry>().ToDictionary(kvp => (string)kvp.Key, kvp => (string)kvp.Value);
            }

            var exports = Runtime.GetVariable("__exports");
            if (exports != null)
            {
                FunctionExports = new List<ScriptFunctionExport>();
                var funcs = (IEnumerable<object>)exports;
                foreach (Hashtable func in funcs)
                {
                    FunctionExports.Add(new ScriptFunctionExport(func["Name"].ToString(), func["Function"].ToString(), this));
                }
            }
        }

        public override void Dispose()
        {
            base.Dispose();
            Runtime.Dispose();
        }

        public override void InvokeExportedFunction(ScriptFunctionExport function)
        {
            Runtime.Execute(function.FunctionName);
        }

        public override void SetVariable(string name, object value)
        {
            Runtime.SetVariable(name, value);
        }

        public override void OnScriptLoaded()
        {
            if (Runtime.GetFunctionExits("OnScriptLoaded"))
            {
                Runtime.Execute("OnScriptLoaded");
            }
        }

        public override void OnGameStarted(Game game)
        {
            if (Runtime.GetFunctionExits("OnGameStarted"))
            {
                Runtime.Execute("OnGameStarted $__game", new Dictionary<string, object>()
                {
                    { "__game", game }
                });
            }
        }

        public override void OnGameStopped(Game game, long ellapsedSeconds)
        {
            if (Runtime.GetFunctionExits("OnGameStopped"))
            {
                Runtime.Execute("OnGameStopped $__game $__ellapsedSeconds", new Dictionary<string, object>()
                {
                    { "__game", game },
                    { "__ellapsedSeconds", ellapsedSeconds }
                });
            }
        }

        public override void OnGameInstalled(Game game)
        {
            if (Runtime.GetFunctionExits("OnGameInstalled"))
            {
                Runtime.Execute("OnGameInstalled $__game", new Dictionary<string, object>()
                {
                    { "__game", game }
                });
            }
        }

        public override void OnGameUninstalled(Game game)
        {
            if (Runtime.GetFunctionExits("OnGameUninstalled"))
            {
                Runtime.Execute("OnGameUninstalled $__game", new Dictionary<string, object>()
                {
                    { "__game", game }
                });
            }
        }
    }
}
