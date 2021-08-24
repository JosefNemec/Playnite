using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Common
{
    public class GlobalRandom
    {
        private static readonly Random generator = new Random();

        public static int Next()
        {
            return generator.Next();
        }

        public static int Next(int minValue, int maxValue)
        {
            return generator.Next(minValue, maxValue);
        }

        public static int Next(int maxValue)
        {
            return generator.Next(maxValue);
        }

        public static void NextBytes(byte[] buffer)
        {
            NextBytes(buffer);
        }

        public static double NextDouble()
        {
            return generator.NextDouble();
        }
    }
}
