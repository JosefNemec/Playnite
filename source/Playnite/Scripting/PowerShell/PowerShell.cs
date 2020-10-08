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
using Playnite.SDK.Exceptions;
using Microsoft.PowerShell;

namespace Playnite.Scripting.PowerShell
{
    public class PowerShellRuntime : IScriptRuntime
    {
        private static NLog.Logger logger = NLog.LogManager.GetLogger("PowerShell");
        private System.Management.Automation.PowerShell powershell;
        private Runspace runspace;
        private PSModuleInfo module;
        private InitialSessionState initialSessionState;

        public static bool IsInstalled
        {
            get
            {
                return Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\PowerShell\3", "Install", null)?.ToString() == "1";
            }
        }

        public PowerShellRuntime(string runspaceName = "PowerShell")
        {
            initialSessionState = InitialSessionState.CreateDefault();
            initialSessionState.ExecutionPolicy = ExecutionPolicy.Unrestricted;
            initialSessionState.ThreadOptions = PSThreadOptions.UseCurrentThread;
            powershell = System.Management.Automation.PowerShell.Create(initialSessionState);
            runspace = powershell.Runspace;
            runspace.Name = runspaceName;
            SetVariable("ErrorActionPreference", "Stop");
            SetVariable("__logger", new Logger(runspaceName));
        }

        public void Dispose()
        {
            runspace.Close();
            runspace.Dispose();
        }

        public void ImportModule(string path)
        {
            powershell.Runspace.SessionStateProxy.Path.SetLocation(Path.GetDirectoryName(path));
            module = powershell
                .AddCommand("Import-Module")
                .AddParameter("PassThru")
                .AddArgument(path)
                .Invoke<PSModuleInfo>().FirstOrDefault();
            powershell.Streams.ClearStreams();
            powershell.Commands.Clear();
        }

        public object Execute(string script, string workDir = null)
        {
            return Execute(script, null, workDir);
        }

        public object Execute(string script, Dictionary<string, object> variables, string workDir = null)
        {
            if (!workDir.IsNullOrEmpty())
            {
                runspace.SessionStateProxy.Path.PushCurrentLocation("main");
                runspace.SessionStateProxy.Path.SetLocation(WildcardPattern.Escape(workDir));
            }

            try
            {
                using (var pipe = runspace.CreatePipeline(script))
                {
                    if (variables != null)
                    {
                        foreach (var key in variables.Keys)
                        {
                            runspace.SessionStateProxy.SetVariable(key, variables[key]);
                        }
                    }

                    Collection<PSObject> result = null;

                    try
                    {
                        result = pipe.Invoke();
                    }
                    catch (RuntimeException e)
                    {
                        throw new ScriptRuntimeException(e.Message, e.ErrorRecord.ScriptStackTrace);
                    }

                    if (result == null)
                    {
                        return null;
                    }
                    else
                    {
                        if (result.Count == 1)
                        {
                            return result[0].BaseObject;
                        }
                        else
                        {
                            return result.Select(a => a?.BaseObject).ToList();
                        }
                    }
                }
            }
            finally
            {
                if (!workDir.IsNullOrEmpty())
                {
                    runspace.SessionStateProxy.Path.PopLocation("main");
                }
            }
        }

        public object ExecuteFile(string path, string workDir = null)
        {
            var content = File.ReadAllText(path);
            return Execute(content, null, workDir);
        }

        public void SetVariable(string name, object value)
        {
            runspace.SessionStateProxy.SetVariable(name, value);
        }

        public object GetVariable(string name)
        {
            return runspace.SessionStateProxy.GetVariable(name);
        }

        public bool GetFunctionExits(string name)
        {
            using (var pipe = runspace.CreatePipeline($"Get-Command {name} -EA 0"))
            {
                var res = pipe.Invoke();
                return res.Count != 0;
            }
        }
    }
}
