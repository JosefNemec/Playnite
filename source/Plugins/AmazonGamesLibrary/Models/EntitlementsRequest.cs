using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AmazonGamesLibrary.Models
{
    public class EntitlementsRequest
    {
        public string Operation = "GetEntitlements";
        public string clientId = "Sonic";
        public int syncPoint = 0;
        public string nextToken;
        public int maxResults = 500;
    }
}
