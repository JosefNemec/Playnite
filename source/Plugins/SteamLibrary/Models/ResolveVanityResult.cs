using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SteamLibrary.Models
{
    public class ResolveVanityResult
    {
        public class Response
        {
            public int success;
            public string steamid;
            public string message;
        }

        public Response response;
    }
}
