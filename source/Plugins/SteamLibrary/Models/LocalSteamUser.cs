using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamLibrary.Models
{
    public class LocalSteamUser
    {
        public ulong Id
        {
            get; set;
        }

        public string AccountName
        {
            get; set;
        }

        public string PersonaName
        {
            get; set;
        }

        public bool Recent
        {
            get; set;
        }
    }
}
