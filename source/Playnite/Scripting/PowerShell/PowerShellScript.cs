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
using Playnite.SDK.Plugins;

namespace Playnite.Scripting.PowerShell
{
    public class PowerShellScript : PlayniteScript
    {
        private static ILogger logger = LogManager.GetLogger();

        public PowerShellRuntime Runtime { get; }

        public PowerShellScript(string path) : base(path)
        {
            Runtime = new PowerShellRuntime(Name);
            Runtime.ExecuteFile(path);
            SupportedEvents = GetSupportedEvents();
            SupportedMenus = GetSupportedMenus();
        }

        public override void Dispose()
        {
            base.Dispose();
            Runtime.Dispose();
        }

        public override List<ScriptGameMenuItem> GetGameMenuItems(GetGameMenuItemsArgs args)
        {
            if (SupportedMenus.Contains(SupportedMenuMethods.GameMenu))
            {
                var res = InvokeFunction("GetGameMenuItems", new List<object> { args });
                if (res is ScriptGameMenuItem item)
                {
                    return new List<ScriptGameMenuItem> { item };
                }
                else if (res is List<object> items)
                {
                    return items.Cast<ScriptGameMenuItem>().ToList();
                }
                else
                {
                    return base.GetGameMenuItems(args);
                }
            }
            else
            {
                return base.GetGameMenuItems(args);
            }
        }

        public override List<ScriptMainMenuItem> GetMainMenuItems(GetMainMenuItemsArgs args)
        {
            if (SupportedMenus.Contains(SupportedMenuMethods.MainMenu))
            {
                var res = InvokeFunction("GetMainMenuItems", new List<object> { args });
                if (res is ScriptMainMenuItem item)
                {
                    return new List<ScriptMainMenuItem> { item };
                }
                else if (res is List<object> items)
                {
                    return items.Cast<ScriptMainMenuItem>().ToList();
                }
                else
                {
                    return base.GetMainMenuItems(args);
                }
            }
            else
            {
                return base.GetMainMenuItems(args);
            }
        }

        internal List<SupportedMenuMethods> GetSupportedMenus()
        {
            var menus = new List<SupportedMenuMethods>();
            var existingMenuFunctions = Runtime.Execute($"Get-Command -Name \"Get*MenuItems\" -CommandType Function");
            if (existingMenuFunctions is FunctionInfo menuFunction)
            {
                var func = GetMenuFunctionFromFunction(menuFunction);
                if (func != null)
                {
                    menus.Add(func.Value);
                }
            }
            else if (existingMenuFunctions is List<object> menuFunctions)
            {
                foreach (FunctionInfo fnc in menuFunctions)
                {
                    var func = GetMenuFunctionFromFunction(fnc);
                    if (func != null)
                    {
                        menus.Add(func.Value);
                    }
                }
            }

            return menus;
        }

        internal List<ApplicationEvent> GetSupportedEvents()
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

        private SupportedMenuMethods? GetMenuFunctionFromFunction(FunctionInfo menuFunction)
        {
            if (menuFunction.Name.Equals(nameof(Plugin.GetMainMenuItems), StringComparison.Ordinal))
            {
                return SupportedMenuMethods.MainMenu;
            }
            else if (menuFunction.Name.Equals(nameof(Plugin.GetGameMenuItems), StringComparison.Ordinal))
            {
                return SupportedMenuMethods.GameMenu;
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
            if (SupportedEvents.Contains(ApplicationEvent.OnApplicationStarted))
            {
                Runtime.Execute("OnApplicationStarted");
            }
        }

        public override void OnApplicationStopped()
        {
            if (SupportedEvents.Contains(ApplicationEvent.OnApplicationStopped))
            {
                Runtime.Execute("OnApplicationStopped");
            }
        }

        public override void OnLibraryUpdated()
        {
            if (SupportedEvents.Contains(ApplicationEvent.OnLibraryUpdated))
            {
                Runtime.Execute("OnLibraryUpdated");
            }
        }

        public override void OnGameStarting(Game game)
        {
            if (SupportedEvents.Contains(ApplicationEvent.OnGameStarting))
            {
                Runtime.Execute("OnGameStarting $__game", new Dictionary<string, object>()
                {
                    { "__game", game }
                });
            }
        }

        public override void OnGameStarted(Game game)
        {
            if (SupportedEvents.Contains(ApplicationEvent.OnGameStarted))
            {
                Runtime.Execute("OnGameStarted $__game", new Dictionary<string, object>()
                {
                    { "__game", game }
                });
            }
        }

        public override void OnGameStopped(Game game, long ellapsedSeconds)
        {
            if (SupportedEvents.Contains(ApplicationEvent.OnGameStopped))
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
            if (SupportedEvents.Contains(ApplicationEvent.OnGameInstalled))
            {
                Runtime.Execute("OnGameInstalled $__game", new Dictionary<string, object>()
                {
                    { "__game", game }
                });
            }
        }

        public override void OnGameUninstalled(Game game)
        {
            if (SupportedEvents.Contains(ApplicationEvent.OnGameUninstalled))
            {
                Runtime.Execute("OnGameUninstalled $__game", new Dictionary<string, object>()
                {
                    { "__game", game }
                });
            }
        }

        public override void OnGameSelected(GameSelectionEventArgs args)
        {
            if (SupportedEvents.Contains(ApplicationEvent.OnGameSelected))
            {
                Runtime.Execute("OnGameSelected $__eventArgs", new Dictionary<string, object>()
                {
                    { "__eventArgs", args }
                });
            }
        }

        public override object InvokeFunction(string functionName)
        {
            return Runtime.Execute(functionName);
        }

        public override object InvokeFunction(string functionName, List<object> arguments)
        {
            var scriptString = functionName;
            var args = new Dictionary<string, object>();
            for (int i = 0; i < arguments.Count; i++)
            {
                scriptString += $" $__arg{i}";
                args.Add($"__arg{i}", arguments[i]);
            }

            return Runtime.Execute(scriptString, args);
        }
    }
}
