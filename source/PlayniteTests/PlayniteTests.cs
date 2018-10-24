using Playnite.Metadata;
using Playnite.SDK.Metadata;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayniteTests
{
    class PlayniteTests
    {
        public static string ResourcesPath
        {
            get
            {
                return Path.Combine(System.AppDomain.CurrentDomain.BaseDirectory, "Resources");
            }
        }

        public static string TempPath
        {
            get
            {
                var path = Path.Combine(Path.GetTempPath(), "playnite_unittests");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                return path;
            }
        }

        private static Random random = new Random();

        public static MetadataFile GenerateFakeFile(string directory)
        {
            var file = new byte[20];
            random.NextBytes(file);
            var fileName = Guid.NewGuid().ToString() + ".file";
            var filePath = Path.Combine(directory, fileName);
            File.WriteAllBytes(filePath, file);
            return new MetadataFile(filePath, fileName, file);
        }

        public static MetadataFile GenerateFakeFile()
        {
            var file = new byte[20];
            random.NextBytes(file);
            var fileName = Guid.NewGuid().ToString() + ".file";
            return new MetadataFile(fileName, fileName, file);
        }
    }
}
