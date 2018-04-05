using IronPython.Runtime;
using Playnite.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Playnite.SDK.Models;

namespace Playnite.Scripting.IronPython
{
    public class IronPythonScript : PlayniteScript
    {
        public IronPythonRuntime Runtime
        {
            get; private set;
        }

        public IronPythonScript(string path) : base(path, ScriptLanguage.IronPython)
        {
            Runtime = new IronPythonRuntime();
            Runtime.ExecuteFile(path);

            var attributes = Runtime.GetVariable("__attributes");
            if (attributes != null)
            {
                Attributes = ((PythonDictionary)attributes).Cast<KeyValuePair<object, object>>().ToDictionary(a => a.Key.ToString(), b => b.Value.ToString());
            }

            var exports = Runtime.GetVariable("__exports");
            if (exports != null)
            {
                FunctionExports = new List<ScriptFunctionExport>();
                var dict = (PythonDictionary)exports;
                foreach (var key in dict.Keys)
                {
                    var functionProp = (PythonDictionary)dict[key];
                    FunctionExports.Add(new ScriptFunctionExport(key.ToString(), functionProp["Function"].ToString(), this));
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
            Runtime.Execute(function.FunctionName + "()");
        }

        public override void SetVariable(string name, object value)
        {
            Runtime.SetVariable(name, value);
        }

        public override void OnScriptLoaded()
        {
            if (Runtime.GetFunctionExits("on_script_loaded"))
            {
                Runtime.Execute("on_script_loaded()");
            }
        }

        public override void OnGameStarted(IGame game)
        {
            if (Runtime.GetFunctionExits("on_game_started"))
            {
                Runtime.Execute("on_game_started(__game)", new Dictionary<string, object>()
                {
                    { "__game", game }
                });
            }
        }

        public override void OnGameStopped(IGame game, long ellapsedSeconds)
        {
            if (Runtime.GetFunctionExits("on_game_stopped"))
            {
                Runtime.Execute("on_game_stopped(__game, __ellapsed_seconds)", new Dictionary<string, object>()
                {
                    { "__game", game },
                    { "__ellapsed_seconds", ellapsedSeconds }
                });
            }
        }

        public override void OnGameInstalled(IGame game)
        {
            if (Runtime.GetFunctionExits("on_game_installed"))
            {
                Runtime.Execute("on_game_installed(__game)", new Dictionary<string, object>()
                {
                    { "__game", game }
                });
            }
        }

        public override void OnGameUninstalled(IGame game)
        {
            if (Runtime.GetFunctionExits("on_game_uninstalled"))
            {
                Runtime.Execute("on_game_uninstalled(__game)", new Dictionary<string, object>()
                {
                    { "__game", game }
                });
            }
        }
    }
}
