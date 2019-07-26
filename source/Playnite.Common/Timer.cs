using Playnite.SDK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Common
{
    public class ExecutionTimer : IDisposable
    {
        private static ILogger logger = LogManager.GetLogger();
        private string name;
        private Stopwatch watch = new Stopwatch();

        public ExecutionTimer(string name)
        {
            this.name = name;
            watch.Start();
        }

        public void Dispose()
        {
            watch.Stop();
            logger.Debug($"--- Timer '{name}', {watch.ElapsedMilliseconds} ms to complete.");
        }
    }

    public class Timer
    {
        private static Random randomGen = new Random();

        public static IDisposable TimeExecution(string name)
        {
            return new ExecutionTimer(name);
        }

        public static int HoursToMilliseconds(int hours)
        {
            return MinutesToMilliseconds(hours * 60);
        }

        public static int MinutesToMilliseconds(int minutes)
        {
            return SecondsToMilliseconds(minutes * 60);
        }

        public static int SecondsToMilliseconds(int seconds)
        {
            return seconds * 1000;
        }
        
        public static DateTime GetRandomDateTime()
        {
            var startDate = new DateTime(1970, 1, 1);
            int range = (DateTime.Today - startDate).Days;
            return startDate.AddDays(randomGen.Next(range));
        }
    }
}
