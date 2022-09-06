using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;

namespace Playnite.Common
{
    public class Network
    {
        public static async Task<bool> GetIsConnectedToInternet()
        {
            // NetworkListManager is too slow (5 or so seconds) even if connection is available.
            // InternetGetConnectedState is no longer recommended to be used by MS.
            // Plus both options are not portable to non-Windows systems.
            // 1.1.1.1 is Cloudflare's DNS server.
            using (var ping = new Ping())
            {
                try
                {
                    return (await ping.SendPingAsync(new System.Net.IPAddress(new byte[] { 1, 1, 1, 1 }), 5_000)).Status == IPStatus.Success;
                }
                catch
                {
                    return false;
                }
            }
        }
    }
}
