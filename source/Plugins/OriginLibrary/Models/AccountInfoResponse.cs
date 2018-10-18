using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OriginLibrary.Models
{
    public class AccountInfoResponse
    {
        public class AcccountInfoData
        {
            public long pidId;
        }

        public string error;
        public AcccountInfoData pid;
    }
}
