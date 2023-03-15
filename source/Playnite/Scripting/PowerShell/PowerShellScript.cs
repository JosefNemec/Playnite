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

        public PowerShellScript(string path, string name) : base(path, name)
        {
            Runtime = new PowerShellRuntime(Name);
            Runtime.ImportModule(path);
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
                var res = InvokeFunction(nameof(Plugin.GetGameMenuItems), new List<object> { args });
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
                var res = InvokeFunction(nameof(Plugin.GetMainMenuItems), new List<object> { args });
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
            if (Runtime.GetFunction(nameof(Plugin.GetMainMenuItems)) != null)
            {
                menus.Add(SupportedMenuMethods.MainMenu);
            }
            if (Runtime.GetFunction(nameof(Plugin.GetGameMenuItems)) != null)
            {
                menus.Add(SupportedMenuMethods.GameMenu);
            }
            return menus;
        }

        internal List<ApplicationEvent> GetSupportedEvents()
        {
            var events = new List<ApplicationEvent>();
            foreach (ApplicationEvent ev in Enum.GetValues(typeof(ApplicationEvent)))
            {
                if (Runtime.GetFunction(ev.ToString()) != null)
                {
                    events.Add(ev);
                }
            }
            return events;
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
                InvokeFunction(ApplicationEvent.OnApplicationStarted.ToString());
            }
        }

        public override void OnApplicationStopped()
        {
            if (SupportedEvents.Contains(ApplicationEvent.OnApplicationStopped))
            {
                InvokeFunction(ApplicationEvent.OnApplicationStopped.ToString());
            }
        }

        public override void OnLibraryUpdated()
        {
            if (SupportedEvents.Contains(ApplicationEvent.OnLibraryUpdated))
            {
                InvokeFunction(ApplicationEvent.OnLibraryUpdated.ToString());
            }
        }

        public override void OnGameStarting(OnGameStartingEventArgs args)
        {
            if (SupportedEvents.Contains(ApplicationEvent.OnGameStarting))
            {
                InvokeFunction(ApplicationEvent.OnGameStarting.ToString(), new List<object> { args });
            }
        }

        public override void OnGameStarted(OnGameStartedEventArgs args)
        {
            if (SupportedEvents.Contains(ApplicationEvent.OnGameStarted))
            {
                InvokeFunction(ApplicationEvent.OnGameStarted.ToString(), new List<object> { args });
            }
        }

        public override void OnGameStopped(OnGameStoppedEventArgs args)
        {
            if (SupportedEvents.Contains(ApplicationEvent.OnGameStopped))
            {
                InvokeFunction(ApplicationEvent.OnGameStopped.ToString(), new List<object> { args });
            }
        }

        public override void OnGameStartupCancelled(OnGameStartupCancelledEventArgs args)
        {
            if (SupportedEvents.Contains(ApplicationEvent.OnGameStartupCancelled))
            {
                InvokeFunction(ApplicationEvent.OnGameStartupCancelled.ToString(), new List<object> { args });
            }
        }

        public override void OnGameInstalled(OnGameInstalledEventArgs args)
        {
            if (SupportedEvents.Contains(ApplicationEvent.OnGameInstalled))
            {
                InvokeFunction(ApplicationEvent.OnGameInstalled.ToString(), new List<object> { args });
            }
        }

        public override void OnGameUninstalled(OnGameUninstalledEventArgs args)
        {
            if (SupportedEvents.Contains(ApplicationEvent.OnGameUninstalled))
            {
                InvokeFunction(ApplicationEvent.OnGameUninstalled.ToString(), new List<object> { args });
            }
        }

        public override void OnGameSelected(OnGameSelectedEventArgs args)
        {
            if (SupportedEvents.Contains(ApplicationEvent.OnGameSelected))
            {
                InvokeFunction(ApplicationEvent.OnGameSelected.ToString(), new List<object> { args });
            }
        }

        public override object InvokeFunction(string functionName)
        {
            return Runtime.InvokeFunction(functionName, new List<object>());
        }

        public override object InvokeFunction(string functionName, List<object> arguments)
        {
            return Runtime.InvokeFunction(functionName, arguments);
        }
    }
}
