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
using Playnite.SDK.Plugins;

namespace Playnite.Scripting.IronPython
{
    public class IronPythonScript : PlayniteScript
    {
        private static ILogger logger = LogManager.GetLogger();

        public IronPythonRuntime Runtime { get; }

        public IronPythonScript(string path) : base(path)
        {
            Runtime = new IronPythonRuntime(Name);
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
                var res = InvokeFunction("get_gamemenu_items", new List<object> { args });
                if (res is ScriptGameMenuItem item)
                {
                    return new List<ScriptGameMenuItem> { item };
                }
                else if (res is IEnumerable items)
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
                var res = InvokeFunction("get_mainmenu_items", new List<object> { args });
                if (res is ScriptMainMenuItem item)
                {
                    return new List<ScriptMainMenuItem> { item };
                }
                else if (res is IEnumerable items)
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
            if (Runtime.GetFunctionExits("get_gamemenu_items"))
            {
                menus.Add(SupportedMenuMethods.GameMenu);
            }

            if (Runtime.GetFunctionExits("get_mainmenu_items"))
            {
                menus.Add(SupportedMenuMethods.MainMenu);
            }

            return menus;
        }

        internal List<ApplicationEvent> GetSupportedEvents()
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
            if (SupportedEvents.Contains(ApplicationEvent.OnApplicationStarted))
            {
                Runtime.Execute("on_application_started()");
            }
        }

        public override void OnApplicationStopped()
        {
            if (SupportedEvents.Contains(ApplicationEvent.OnApplicationStopped))
            {
                Runtime.Execute("on_application_stopped()");
            }
        }

        public override void OnLibraryUpdated()
        {
            if (SupportedEvents.Contains(ApplicationEvent.OnLibraryUpdated))
            {
                Runtime.Execute("on_library_updated()");
            }
        }

        public override void OnGameStarting(Game game)
        {
            if (SupportedEvents.Contains(ApplicationEvent.OnGameStarting))
            {
                Runtime.Execute("on_game_starting(__game)", new Dictionary<string, object>()
                {
                    { "__game", game }
                });
            }
        }

        public override void OnGameStarted(Game game)
        {
            if (SupportedEvents.Contains(ApplicationEvent.OnGameStarted))
            {
                Runtime.Execute("on_game_started(__game)", new Dictionary<string, object>()
                {
                    { "__game", game }
                });
            }
        }

        public override void OnGameStopped(Game game, long ellapsedSeconds)
        {
            if (SupportedEvents.Contains(ApplicationEvent.OnGameStopped))
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
            if (SupportedEvents.Contains(ApplicationEvent.OnGameInstalled))
            {
                Runtime.Execute("on_game_installed(__game)", new Dictionary<string, object>()
                {
                    { "__game", game }
                });
            }
        }

        public override void OnGameUninstalled(Game game)
        {
            if (SupportedEvents.Contains(ApplicationEvent.OnGameUninstalled))
            {
                Runtime.Execute("on_game_uninstalled(__game)", new Dictionary<string, object>()
                {
                    { "__game", game }
                });
            }
        }

        public override void OnGameSelected(GameSelectionEventArgs args)
        {
            if (SupportedEvents.Contains(ApplicationEvent.OnGameSelected))
            {
                Runtime.Execute("on_game_selected(__event_args)", new Dictionary<string, object>()
                {
                    { "__event_args", args }
                });
            }
        }

        public override object InvokeFunction(string functionName)
        {
            return Runtime.Execute(functionName + "()");
        }

        public override object InvokeFunction(string functionName, List<object> arguments)
        {
            var scriptString = functionName + "(";
            var args = new Dictionary<string, object>();
            for (int i = 0; i < arguments.Count; i++)
            {
                scriptString += $"__arg{i},";
                args.Add($"__arg{i}", arguments[i]);
            }

            scriptString = scriptString.TrimEnd(new char[] { ',' }) + ")";
            return Runtime.Execute(scriptString, args);
        }
    }
}
