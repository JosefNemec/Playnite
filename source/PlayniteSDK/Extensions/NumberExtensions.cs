using System;

namespace Playnite.SDK.Extensions
{
    public static class NumberExtensions
    {
        private static readonly string[] Suffix = {"B", "KB", "MB", "GB", "TB", "PB", "EB"}; // Longs run out around EB
        
        /// <summary>
        /// Converts a number of bytes into a human readable format (eg: "53.1 MB").
        /// </summary>
        /// <param name="byteCount"></param>
        /// <returns></returns>
        public static string ToFileSizeString(this long byteCount)
        {
            if (byteCount == 0)
                return "0" + Suffix[0];
            var bytes = Math.Abs(byteCount);
            var place = Convert.ToInt32(Math.Floor(Math.Log(bytes, 1024)));
            var num = Math.Round(bytes / Math.Pow(1024, place), 1);
            return $"{Math.Sign(byteCount) * num} {Suffix[place]}";
        }
    }
}