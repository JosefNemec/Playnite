using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PlayniteServices.Models
{
    public class User
    {
        public string Id { get; set; }
        public string WinVersion { get; set; }
        public string PlayniteVersion { get; set; }
        public DateTime LastLaunch { get; set; }
        public bool Is64Bit { get; set; }
    }
}
