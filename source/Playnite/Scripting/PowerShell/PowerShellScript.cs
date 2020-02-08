using Playnite.SDK;
using Playnite.SDK.Models;
using Playnite.SDK.Events;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;
using Playnite.Common;

namespace Playnite.Scripting.PowerShell
{
    public class PowerShellScript : PlayniteScript
    {
        private static ILogger logger = LogManager.GetLogger();
        private readonly List<ApplicationEvent> supportedEvents;

        public PowerShellRuntime Runtime
        {
            get; private set;
        }

        public PowerShellScript(string path) : base(path)
        {
            Runtime = new PowerShellRuntime();
            Runtime.ExecuteFile(path);
            supportedEvents = GetSupportedEvents();
        }

        public override void Dispose()
        {
            base.Dispose();
            Runtime.Dispose();
        }

        private List<ApplicationEvent> GetSupportedEvents()
        {
            var events = new List<ApplicationEvent>();
            var existingEvents = Runtime.Execute($"Get-Command -Name \"On*\" -CommandType Function");
            if (existingEvents is FunctionInfo appEvent)
            {
                var ev = GetEventFromFunction(appEvent);
                if (ev != null)
                {
                    events.Add(ev.Value);
                }
            }
            else if (existingEvents is List<object> appEvents)
            {
                foreach (FunctionInfo appev in appEvents)
                {
                    var ev = GetEventFromFunction(appev);
                    if (ev != null)
                    {
                        events.Add(ev.Value);
                    }
                }
            }

            return events;
        }

        private ApplicationEvent? GetEventFromFunction(FunctionInfo function)
        {
            foreach (ApplicationEvent ev in Enum.GetValues(typeof(ApplicationEvent)))
            {
                if (function.Name.Equals(ev.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    return ev;
                }
            }

            return null;
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
            if (supportedEvents.Contains(ApplicationEvent.OnApplicationStarted))
            {
                Runtime.Execute("OnApplicationStarted");
            }
        }

        public override void OnApplicationStopped()
        {
            if (supportedEvents.Contains(ApplicationEvent.OnApplicationStopped))
            {
                Runtime.Execute("OnApplicationStopped");
            }
        }

        public override void OnLibraryUpdated()
        {
            if (supportedEvents.Contains(ApplicationEvent.OnLibraryUpdated))
            {
                Runtime.Execute("OnLibraryUpdated");
            }
        }

        public override void OnGameStarting(Game game)
        {
            if (supportedEvents.Contains(ApplicationEvent.OnGameStarting))
            {
                Runtime.Execute("OnGameStarting $__game", new Dictionary<string, object>()
                {
                    { "__game", game }
                });
            }
        }

        public override void OnGameStarted(Game game)
        {
            if (supportedEvents.Contains(ApplicationEvent.OnGameStarted))
            {
                Runtime.Execute("OnGameStarted $__game", new Dictionary<string, object>()
                {
                    { "__game", game }
                });
            }
        }

        public override void OnGameStopped(Game game, long ellapsedSeconds)
        {
            if (supportedEvents.Contains(ApplicationEvent.OnGameStopped))
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
            if (supportedEvents.Contains(ApplicationEvent.OnGameInstalled))
            {
                Runtime.Execute("OnGameInstalled $__game", new Dictionary<string, object>()
                {
                    { "__game", game }
                });
            }
        }

        public override void OnGameUninstalled(Game game)
        {
            if (supportedEvents.Contains(ApplicationEvent.OnGameUninstalled))
            {
                Runtime.Execute("OnGameUninstalled $__game", new Dictionary<string, object>()
                {
                    { "__game", game }
                });
            }
        }

        public override void OnGameSelected(GameSelectionEventArgs args)
        {
            if (supportedEvents.Contains(ApplicationEvent.OnGameSelected))
            {
                Runtime.Execute("OnGameSelected $__eventArgs", new Dictionary<string, object>()
                {
                    { "__eventArgs", args }
                });
            }
        }
    }
}
