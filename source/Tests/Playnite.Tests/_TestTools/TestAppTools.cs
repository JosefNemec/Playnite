using Playnite.Emulators;
using Playnite.SDK.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Tests
{
    public class TestAppTools
    {
        public const string PlatformSpecId = "test_app_platform";

        public static EmulatorDefinition GetEmulatorDefitionObj()
        {
            return new EmulatorDefinition
            {
                Id = "testApp",
                Name = "Test App",
                Profiles = new List<EmulatorDefinitionProfile>
                {
                    new EmulatorDefinitionProfile
                    {
                        ImageExtensions = new List<string> { "iso", "mp3", "zip" },
                        Name = "default",
                        StartupExecutable = @"^TestApp\.exe$",
                        StartupArguments = "some args"
                    }
                }
            };
        }

        public static Emulator GetEmulatorObj()
        {
            return new Emulator
            {
                Name = "Test App",
                CustomProfiles = new ObservableCollection<CustomEmulatorProfile>
                {
                    new CustomEmulatorProfile
                    {
                        Name = "test profile",
                        Arguments = "{ImagePath}",
                        Executable = "TestPath.exe",
                        ImageExtensions = new List<string> { "iso", "mp3", "zip" }
                    }
                }
            };
        }

        public static Platform GetPlatformObj()
        {
            return new Platform
            {
                Name = "Test App platform",
                SpecificationId = PlatformSpecId
            };
        }
    }
}
