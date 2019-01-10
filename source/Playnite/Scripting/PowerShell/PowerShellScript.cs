using Playnite.SDK;
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
        private static ILogger logger = LogManager.GetLogger();

        public PowerShellRuntime Runtime
        {
            get; private set;
        }

        public PowerShellScript(string path, IDictionary<string, object> initialVariables) : base(path)
        {
            Runtime = new PowerShellRuntime();
            if (initialVariables != null)
            {
                foreach (var kvp in initialVariables)
                {
                    Runtime.SetVariable(kvp.Key, kvp.Value);
                }
            }
            Runtime.ExecuteFile(path);
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

        public override void OnApplicationStarted()
        {
            if (Runtime.GetFunctionExits("OnApplicationStarted"))
            {
                Runtime.Execute("OnApplicationStarted");
            }
        }

        public override void OnGameStarting(Game game)
        {
            if (Runtime.GetFunctionExits("OnGameStarting"))
            {
                Runtime.Execute("OnGameStarting $__game", new Dictionary<string, object>()
                {
                    { "__game", game }
                });
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
