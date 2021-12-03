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
using Playnite.SDK;

namespace Playnite.Scripting.PowerShell
{
    public interface IPowerShellRuntime : IDisposable
    {
        object Execute(string script, string workDir = null, Dictionary<string, object> variables = null);
        object ExecuteFile(string path, string workDir = null);
        object ExecuteFile(string path, string workDir = null, Dictionary<string, object> variables = null);
        void SetVariable(string name, object value);
        object GetVariable(string name);
        CommandInfo GetFunction(string name);
        void ImportModule(string path);
        object InvokeFunction(string name, List<object> arguments);
    }

    public class DummyPowerShellRuntime : IPowerShellRuntime
    {
        public DummyPowerShellRuntime()
        {
        }

        public object Execute(string script, string workDir = null, Dictionary<string, object> variables = null)
        {
            return null;
        }

        public object ExecuteFile(string path, string workDir = null)
        {
            return null;
        }

        public object ExecuteFile(string path, string workDir = null, Dictionary<string, object> variables = null)
        {
            return null;
        }

        public void SetVariable(string name, object value)
        {
        }

        public object GetVariable(string name)
        {
            return null;
        }

        public CommandInfo GetFunction(string name)
        {
            throw new NotImplementedException();
        }

        public void ImportModule(string path)
        {
            throw new NotImplementedException();
        }

        public object InvokeFunction(string name, List<object> arguments)
        {
            return null;
        }

        public void Dispose()
        {
        }
    }

    public class PowerShellRuntime : IPowerShellRuntime
    {
        private System.Management.Automation.PowerShell powershell;
        private Runspace runspace;
        private PSModuleInfo module;
        private InitialSessionState initialSessionState;
        public bool IsDisposed { get; internal set; }

        public static bool IsInstalled
        {
            get
            {
                return Registry.GetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\PowerShell\3", "Install", null)?.ToString() == "1";
            }
        }

        public PowerShellRuntime(string runspaceName)
        {
            if (!IsInstalled)
            {
                throw new Exception("PowerShell 5.1 is not installed.");
            }

            initialSessionState = InitialSessionState.CreateDefault();
            initialSessionState.ExecutionPolicy = ExecutionPolicy.Bypass;
            initialSessionState.ThreadOptions = PSThreadOptions.UseCurrentThread;
            powershell = System.Management.Automation.PowerShell.Create(initialSessionState);
            runspace = powershell.Runspace;
            runspace.Name = runspaceName;
            SetVariable("ErrorActionPreference", "Stop");
            SetVariable("__logger", LogManager.GetLogger(runspaceName));
        }

        public void Dispose()
        {
            IsDisposed = true;
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

        public object Execute(string script, string workDir = null, Dictionary<string, object> variables = null)
        {
            if (!workDir.IsNullOrEmpty() && Directory.Exists(workDir))
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
                            return result[0]?.BaseObject;
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
                if (!workDir.IsNullOrEmpty() && Directory.Exists(workDir) && runspace.RunspaceStateInfo.State == RunspaceState.Opened)
                {
                    runspace.SessionStateProxy.Path.PopLocation("main");
                }
            }
        }

        public object ExecuteFile(string path, string workDir = null)
        {
            return ExecuteFile(path, workDir);
        }

        public object ExecuteFile(string path, string workDir = null, Dictionary<string, object> variables = null)
        {
            var cmd = "& '{0}' $__FileArg".Format(Path.GetFullPath(path));
            return Execute(cmd, workDir, variables != null ? new Dictionary<string, object> { { "__FileArg", variables } } : null);
        }

        public void SetVariable(string name, object value)
        {
            runspace.SessionStateProxy.SetVariable(name, value);
        }

        public object GetVariable(string name)
        {
            return runspace.SessionStateProxy.GetVariable(name);
        }

        public CommandInfo GetFunction(string name)
        {
            if (module == null)
            {
                return null;
            }
            CommandInfo command;
            return module.ExportedCommands.TryGetValue(name, out command) ? command : null;
        }

        public object InvokeFunction(string name, List<object> arguments)
        {
            try
            {
                var command = GetFunction(name);
                powershell.AddCommand(command);
                foreach (var argument in arguments)
                {
                    powershell.AddArgument(argument);
                }

                var result = powershell.Invoke();
                if (result.Count == 1)
                {
                    return result[0].BaseObject;
                }
                else
                {
                    return result.Select(a => a?.BaseObject).ToList();
                }
            }
            finally
            {
                powershell.Streams.ClearStreams();
                powershell.Commands.Clear();
            }
        }
    }
}
