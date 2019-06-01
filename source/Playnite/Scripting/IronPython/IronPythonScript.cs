using IronPython.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Playnite.SDK.Models;
using Playnite.SDK;

namespace Playnite.Scripting.IronPython
{
    public class IronPythonScript : PlayniteScript
    {
        private static ILogger logger = LogManager.GetLogger();

        public IronPythonRuntime Runtime
        {
            get; private set;
        }

        public IronPythonScript(string name, string path, IDictionary<string, object> initialVariables) : base(path)
        {
            Runtime = new IronPythonRuntime();
            if (initialVariables != null)
            {
                foreach (var kvp in initialVariables)
                {
                    Runtime.SetVariable(kvp.Key, kvp.Value);
                }
            }
            Runtime.ImportModule(path);
        }

        public override void Dispose()
        {
            base.Dispose();
            Runtime.Dispose();
        }

        public override void InvokeExportedFunction(ScriptFunctionExport function)
        {
            Runtime.CallFunction(function.FunctionName);
        }

        public override void SetVariable(string name, object value)
        {
            Runtime.SetVariable(name, value);
        }

        public override void OnApplicationStarted()
        {
            if (Runtime.GetFunctionExits("on_application_started"))
            {
                Runtime.CallFunction("on_application_started");
            }
        }

        public override void OnGameStarting(Game game)
        {
            if (Runtime.GetFunctionExits("on_game_starting"))
            {
                Runtime.CallFunction("on_game_starting", game);
            }
        }

        public override void OnGameStarted(Game game)
        {
            if (Runtime.GetFunctionExits("on_game_started"))
            {
                Runtime.CallFunction("on_game_started", game);
            }
        }

        public override void OnGameStopped(Game game, long ellapsedSeconds)
        {
            if (Runtime.GetFunctionExits("on_game_stopped"))
            {
                Runtime.CallFunction("on_game_stopped", game, ellapsedSeconds);
            }
        }

        public override void OnGameInstalled(Game game)
        {
            if (Runtime.GetFunctionExits("on_game_installed"))
            {
                Runtime.CallFunction("on_game_installed", game);
            }
        }

        public override void OnGameUninstalled(Game game)
        {
            if (Runtime.GetFunctionExits("on_game_uninstalled"))
            {
                Runtime.CallFunction("on_game_uninstalled", game);
            }
        }
    }
}
