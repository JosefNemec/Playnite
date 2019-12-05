using IronPython.Runtime;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Playnite.SDK.Models;
using Playnite.SDK;
using Playnite.SDK.Events;

namespace Playnite.Scripting.IronPython
{
    public class IronPythonScript : PlayniteScript
    {
        private static ILogger logger = LogManager.GetLogger();

        public IronPythonRuntime Runtime
        {
            get; private set;
        }

        public IronPythonScript(string path) : base(path)
        {
            Runtime = new IronPythonRuntime();
            Runtime.ExecuteFile(path);
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

        public override void OnApplicationStarted()
        {
            if (Runtime.GetFunctionExits("on_application_started"))
            {
                Runtime.Execute("on_application_started()");
            }
        }

        public override void OnGameStarting(Game game)
        {
            if (Runtime.GetFunctionExits("on_game_starting"))
            {
                Runtime.Execute("on_game_starting(__game)", new Dictionary<string, object>()
                {
                    { "__game", game }
                });
            }
        }

        public override void OnGameStarted(Game game)
        {
            if (Runtime.GetFunctionExits("on_game_started"))
            {
                Runtime.Execute("on_game_started(__game)", new Dictionary<string, object>()
                {
                    { "__game", game }
                });
            }
        }

        public override void OnGameStopped(Game game, long ellapsedSeconds)
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

        public override void OnGameInstalled(Game game)
        {
            if (Runtime.GetFunctionExits("on_game_installed"))
            {
                Runtime.Execute("on_game_installed(__game)", new Dictionary<string, object>()
                {
                    { "__game", game }
                });
            }
        }

        public override void OnGameUninstalled(Game game)
        {
            if (Runtime.GetFunctionExits("on_game_uninstalled"))
            {
                Runtime.Execute("on_game_uninstalled(__game)", new Dictionary<string, object>()
                {
                    { "__game", game }
                });
            }
        }

        public override void OnGameSelected(GameSelectionEventArgs args)
        {
            if (Runtime.GetFunctionExits("on_game_selected"))
            {
                Runtime.Execute("on_game_selected(__event_args)", new Dictionary<string, object>()
                {
                    { "__event_args", args }
                });
            }
        }
    }
}
