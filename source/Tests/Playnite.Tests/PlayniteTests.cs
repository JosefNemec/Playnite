using Moq;
using Playnite.API;
using Playnite.Metadata;
using Playnite.SDK;
using Playnite.SDK.Metadata;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Tests
{
    public static class PlayniteTests
    {
        public static string ResourcesPath
        {
            get
            {
                return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Resources");
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
            return new MetadataFile(fileName, file);
        }

        public static MetadataFile GenerateFakeFile()
        {
            var file = new byte[20];
            random.NextBytes(file);
            var fileName = Guid.NewGuid().ToString() + ".file";
            return new MetadataFile(fileName, file);
        }

        public static void SetEntryAssembly(Assembly assembly)
        {
            AppDomainManager manager = new AppDomainManager();
            FieldInfo entryAssemblyfield = manager.GetType().GetField("m_entryAssembly", BindingFlags.Instance | BindingFlags.NonPublic);
            entryAssemblyfield.SetValue(manager, assembly);

            AppDomain domain = AppDomain.CurrentDomain;
            FieldInfo domainManagerField = domain.GetType().GetField("_domainManager", BindingFlags.Instance | BindingFlags.NonPublic);
            domainManagerField.SetValue(domain, manager);
        }

        public static Mock<IPlayniteAPI> GetTestingApi()
        {
            var api = new Mock<IPlayniteAPI>();
            var notification = new Mock<INotificationsAPI>();
            api.Setup(a => a.Paths).Returns(new PlaynitePathsAPI());
            api.Setup(a => a.ApplicationInfo).Returns(new PlayniteInfoAPI());
            api.Setup(a => a.Resources).Returns(new ResourceProvider());
            api.Setup(a => a.Notifications).Returns(notification.Object);
            return api;
        }
    }
}
