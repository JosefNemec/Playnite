using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TwitchLibrary.Models
{
    public class GameConfiguration
    {
        public const string ConfigFileName = "fuel.json";

        public class Config
        {
            public string Command;
            public string WorkingSubdirOverride;
            public List<string> Args;
            public List<string> AuthScopes;
            public string ClientId;
        }

        public string SchemaVersion;
        public Config Main;
    }
}
