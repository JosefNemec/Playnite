using Playnite.Common;
using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Scripting.Batch
{
    public class BatchRuntimeException : Exception
    {
        public BatchRuntimeException()
        {
        }

        public BatchRuntimeException(string message) : base(message)
        {
        }
    }

    public class BatchRuntime : IScriptRuntime
    {
        private static readonly ILogger logger = LogManager.GetLogger();

        public void Dispose()
        {
        }

        public object Execute(string script, string workDir = null)
        {
            var scriptPath = Path.Combine(PlaynitePaths.TempPath, $"BatchRuntime_{GlobalRandom.Next()}.bat");
            FileSystem.PrepareSaveFile(scriptPath);
            FileSystem.WriteStringToFile(scriptPath, script);

            try
            {
                return ExecuteFile(scriptPath, workDir);
            }
            finally
            {
                FileSystem.DeleteFile(scriptPath);
            }
        }

        public object ExecuteFile(string scriptPath, string workDir = null)
        {
            var processInfo = new ProcessStartInfo("cmd.exe", "/c " + scriptPath)
            {
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardError = true,
                RedirectStandardOutput = true
            };

            if (!workDir.IsNullOrEmpty())
            {
                processInfo.WorkingDirectory = workDir;
            }

            var errorData = string.Empty;
            var process = new Process();
            process.StartInfo = processInfo;
            process.OutputDataReceived += (s, e) =>
            {
                if (e.Data != null)
                {
                    logger.Info(e.Data);
                }
            };

            process.ErrorDataReceived += (s, e) =>
            {
                if (e.Data != null)
                {
                    errorData += e.Data;
                    logger.Error(e.Data);
                }
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();
            var exitCode = process.ExitCode;
            process.Close();

            if (!errorData.IsNullOrEmpty())
            {
                throw new BatchRuntimeException(errorData);
            }

            return exitCode;
        }
    }
}
