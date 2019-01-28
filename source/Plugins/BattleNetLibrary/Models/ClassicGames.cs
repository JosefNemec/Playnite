using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleNetLibrary.Models
{
    public class ClassicGames
    {
        public class ClassicGame
        {
            public string localizedGameName;
            public string regionalGameFranchiseIconFilename;
            public List<string> cdKeys;
        }

        public List<ClassicGame> classicGames;
    }
}
