using Playnite.Common;
using Playnite.Database;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Tests
{
    public class GameDbTestWrapper : IDisposable
    {
        private readonly bool createdTempPath;
        private readonly TempDirectory tempDir;
        public readonly GameDatabase DB;
        public readonly string DbDirectory;

        public GameDbTestWrapper(bool openDb = true)
        {
            var stack = new StackTrace(1);
            var method = stack.GetFrame(0).GetMethod();
            tempDir = TempDirectory.Create(true, Paths.GetSafePathName($"{method.DeclaringType.Name}_{method.Name}"));
            DbDirectory = tempDir.TempPath;
            DB = new GameDatabase(DbDirectory);
            Game.DatabaseReference = DB;
            createdTempPath = true;

            if (openDb)
            {
                DB.OpenDatabase();
            }
        }

        public GameDbTestWrapper(TempDirectory dbPath, bool openDb = true)
        {
            DbDirectory = dbPath.TempPath;
            DB = new GameDatabase(DbDirectory);
            Game.DatabaseReference = DB;
            createdTempPath = false;

            if (openDb)
            {
                DB.OpenDatabase();
            }
        }

        public void Dispose()
        {
            DB.Dispose();
            if (createdTempPath)
            {
                tempDir.Dispose();
            }
        }
    }
}
