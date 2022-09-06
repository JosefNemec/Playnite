using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Tests
{
    public class TestDateTimes : DateTimes.IDateTimes
    {
        public DateTime Now { get; set; }
        public DateTime Today { get; set; }
    }
}
