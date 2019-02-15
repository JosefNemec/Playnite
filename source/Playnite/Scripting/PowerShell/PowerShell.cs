using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Management.Automation;
using System.Management.Automation.Runspaces;
using System.Collections.ObjectModel;
using Playnite.API;
using Microsoft.Win32;
using System.IO;
using Microsoft.PowerShell;

namespace Playnite.Scripting.PowerShell
{
    public class PowerShellRuntime : IScriptRuntime
    {
        private static NLog.Logger logger = NLog.LogManager.GetLogger("PowerShell");
        private System.Management.Automation.PowerShell powerShell;
        private InitialSessionState initialSessionState;
        private PSModuleInfo module;

        public static bool IsInstalled
        {
            get
            {
                return Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\PowerShell\3", "Install", null)?.ToString() == "1";
            }
        }

        public PowerShellRuntime(string name)
        {
            initialSessionState = InitialSessionState.CreateDefault();
            initialSessionState.ExecutionPolicy = ExecutionPolicy.Unrestricted;
            initialSessionState.ThreadOptions = PSThreadOptions.UseCurrentThread;
            //initialSessionState.ApartmentState = System.Threading.ApartmentState.MTA;
            powerShell = System.Management.Automation.PowerShell.Create(initialSessionState);
            powerShell.Runspace.Name = name;
            SetVariable("ErrorActionPreference", "Stop");
            SetVariable("__logger", new Logger("PowerShell"));
        }

        public void Dispose()
        {

        }

        public void ImportModule(string path)
        {
            powerShell.Runspace.SessionStateProxy.Path.SetLocation(Path.GetDirectoryName(path));
            module = powerShell
                .AddCommand("Import-Module")
                .AddParameter("PassThru")
                .AddArgument(path)
                .Invoke<PSModuleInfo>().FirstOrDefault();
            powerShell.Commands.Clear();
        }

        public void SetVariable(string name, object value)
        {
            powerShell.Runspace.SessionStateProxy.SetVariable(name, value);
        }

        public object GetVariable(string name)
        {
            return powerShell.Runspace.SessionStateProxy.GetVariable(name);
        }

        public Collection<PSObject> CallFunction(string name, params object[] arguments)
        {
            powerShell.AddCommand(module.ExportedFunctions[name]);
            foreach (var argument in arguments)
            {
                powerShell.AddArgument(argument);
            }
            var result = powerShell.Invoke();
            powerShell.Commands.Clear();
            return result;
        }

        public bool GetFunctionExits(string name)
        {
            return module?.ExportedFunctions.ContainsKey(name) ?? false;
        }
    }
}
