using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.SDK
{
    public class Logger : ILogger
    {
        private readonly string loggerName;

        public Logger(string loggerName)
        {
            this.loggerName = loggerName;
        }

        public void Debug(string message)
        {
            WriteMessage("|DEBUG: " + message);
        }

        public void Debug(Exception exception, string message)
        {
            WriteMessage(exception, "|DEBUG: " + message);
        }

        public void Error(string message)
        {
            WriteMessage("|ERROR: " + message);
        }

        public void Error(Exception exception, string message)
        {
            WriteMessage(exception, "|ERROR: " + message);
        }

        public void Info(string message)
        {
            WriteMessage("|INFO: " + message);
        }

        public void Info(Exception exception, string message)
        {
            WriteMessage(exception, "|INFO: " + message);
        }

        public void Trace(string message)
        {
            WriteMessage("|TRACE: " + message);
        }

        public void Trace(Exception exception, string message)
        {
            WriteMessage(exception, "|TRACE: " + message);
        }

        public void Warn(string message)
        {
            WriteMessage("|WARN: " + message);
        }

        public void Warn(Exception exception, string message)
        {
            WriteMessage(exception, "|WARN: " + message);
        }

        private void WriteMessage(string message)
        {
            LogManager.WriteMessage(loggerName + message);
        }

        private void WriteMessage(Exception exception, string message)
        {
            LogManager.WriteMessage(loggerName + message);
            LogManager.WriteMessage(exception.ToString());
        }
    }

    public class LogManager
    {
        private static FileStream logStream;
        private static StreamWriter logWriter;
        private static object writeLock = new object();

        public static void Initialize(string filePath)
        {
            logStream = new FileStream(filePath, FileMode.Append, FileAccess.Write);
            logWriter = new StreamWriter(logStream) { AutoFlush = true };
        }

        public static void Dispose()
        {
            logWriter.Dispose();
            logStream.Dispose();
        }

        public static void WriteMessage(string message)
        {
            lock (writeLock)
            {
                logWriter.WriteLine(DateTime.Now.ToLongTimeString() + "|" + message);
            }

            if (Debugger.IsAttached)
            {
                Console.WriteLine(message);
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static ILogger GetLogger()
        {
            var className = (new StackFrame(1)).GetMethod().DeclaringType.Name;
            return GetLogger(className);
        }

        public static ILogger GetLogger(string loggerName)
        {
            return new Logger(loggerName);
        }
    }
}
