using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Common
{
    public class TempDirectory : IDisposable
    {
        private bool autoDelete;

        public string TempPath { get; private set; }

        public static TempDirectory Create(bool autoDelete = true)
        {
            var stack = new StackTrace(1);
            var method = stack.GetFrame(0).GetMethod();
            var dirName = Paths.GetSafePathName($"{method.DeclaringType.Name}_{method.Name}");
            return new TempDirectory(dirName, autoDelete);
        }

        public TempDirectory(string dirName, bool autoDelete = true)
        {
            TempPath = Path.Combine(Path.GetTempPath(), "Playnite", dirName);
            FileSystem.CreateDirectory(TempPath, true);
            this.autoDelete = autoDelete;
        }

        public void Dispose()
        {
            if (autoDelete)
            {
                FileSystem.DeleteDirectory(TempPath);
            }
        }
    }
}
