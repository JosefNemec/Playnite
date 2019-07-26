using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace OriginLibrary.Models
{
    public class UsageResponse
    {
        public UsageResponse(string xml)
        {
            this.total = 0;
            this.lastSessionEndTimeStamp = null;

            var usage = XElement.Parse(xml);

            string value = usage?.Element("total")?.Value;
            if (!string.IsNullOrEmpty(value))
            {
                long.TryParse(value, out this.total);
            }

            value = usage?.Element("lastSessionEndTimeStamp")?.Value;
            if (!string.IsNullOrEmpty(value))
            {
                long time = 0;
                if (long.TryParse(value, out time) && time > 0)
                {
                    this.lastSessionEndTimeStamp = DateTimeOffset.FromUnixTimeMilliseconds(time).DateTime;
                }
            }
        }

        public long total;
        public DateTime? lastSessionEndTimeStamp;
    }
}
