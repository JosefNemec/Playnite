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
using Playnite.Common;

namespace Playnite.Scripting.IronPython
{
    public class IronPythonScript : PlayniteScript
    {
        private static ILogger logger = LogManager.GetLogger();
        private readonly List<ApplicationEvent> supportedEvents;

        public IronPythonRuntime Runtime
        {
            get; private set;
        }

        public IronPythonScript(string path) : base(path)
        {
            Runtime = new IronPythonRuntime();
            Runtime.ExecuteFile(path);
            supportedEvents = GetSupportedEvents();
        }

        private List<ApplicationEvent> GetSupportedEvents()
        {
            var events = new List<ApplicationEvent>();

            if (Runtime.GetFunctionExits("on_application_started"))
            {
                events.Add(ApplicationEvent.OnApplicationStarted);
            }

            if (Runtime.GetFunctionExits("on_application_stopped"))
            {
                events.Add(ApplicationEvent.OnApplicationStopped);
            }

            if (Runtime.GetFunctionExits("on_library_updated"))
            {
                events.Add(ApplicationEvent.OnLibraryUpdated);
            }

            if (Runtime.GetFunctionExits("on_game_starting"))
            {
                events.Add(ApplicationEvent.OnGameStarting);
            }

            if (Runtime.GetFunctionExits("on_game_started"))
            {
                events.Add(ApplicationEvent.OnGameStarted);
            }

            if (Runtime.GetFunctionExits("on_game_stopped"))
            {
                events.Add(ApplicationEvent.OnGameStopped);
            }

            if (Runtime.GetFunctionExits("on_game_installed"))
            {
                events.Add(ApplicationEvent.OnGameInstalled);
            }

            if (Runtime.GetFunctionExits("on_game_uninstalled"))
            {
                events.Add(ApplicationEvent.OnGameUninstalled);
            }

            if (Runtime.GetFunctionExits("on_game_selected"))
            {
                events.Add(ApplicationEvent.OnGameSelected);
            }

            return events;
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
            if (supportedEvents.Contains(ApplicationEvent.OnApplicationStarted))
            {
                Runtime.Execute("on_application_started()");
            }
        }

        public override void OnApplicationStopped()
        {
            if (supportedEvents.Contains(ApplicationEvent.OnApplicationStopped))
            {
                Runtime.Execute("on_application_stopped()");
            }
        }

        public override void OnLibraryUpdated()
        {
            if (supportedEvents.Contains(ApplicationEvent.OnLibraryUpdated))
            {
                Runtime.Execute("on_library_updated()");
            }
        }

        public override void OnGameStarting(Game game)
        {
            if (supportedEvents.Contains(ApplicationEvent.OnGameStarting))
            {
                Runtime.Execute("on_game_starting(__game)", new Dictionary<string, object>()
                {
                    { "__game", game }
                });
            }
        }

        public override void OnGameStarted(Game game)
        {
            if (supportedEvents.Contains(ApplicationEvent.OnGameStarted))
            {
                Runtime.Execute("on_game_started(__game)", new Dictionary<string, object>()
                {
                    { "__game", game }
                });
            }
        }

        public override void OnGameStopped(Game game, long ellapsedSeconds)
        {
            if (supportedEvents.Contains(ApplicationEvent.OnGameStopped))
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
            if (supportedEvents.Contains(ApplicationEvent.OnGameInstalled))
            {
                Runtime.Execute("on_game_installed(__game)", new Dictionary<string, object>()
                {
                    { "__game", game }
                });
            }
        }

        public override void OnGameUninstalled(Game game)
        {
            if (supportedEvents.Contains(ApplicationEvent.OnGameUninstalled))
            {
                Runtime.Execute("on_game_uninstalled(__game)", new Dictionary<string, object>()
                {
                    { "__game", game }
                });
            }
        }

        public override void OnGameSelected(GameSelectionEventArgs args)
        {
            if (supportedEvents.Contains(ApplicationEvent.OnGameSelected))
            {
                Runtime.Execute("on_game_selected(__event_args)", new Dictionary<string, object>()
                {
                    { "__event_args", args }
                });
            }
        }
    }
}
