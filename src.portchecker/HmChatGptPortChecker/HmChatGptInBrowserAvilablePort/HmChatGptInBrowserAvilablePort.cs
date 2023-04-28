using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace HmChatGptInBrowserAvilablePort
{
    [Guid("D96B4C2A-E1C7-4E9E-8A8B-D49474E9457D")]
    public class HmUsedPortChecker
    {
        List<int> portsInUse;
        public int GetAvailablePort(int beginPort, int endPort)
        {
            var ipGP = IPGlobalProperties.GetIPGlobalProperties();
            var tcpEPs = ipGP.GetActiveTcpListeners();
            var udpEPs = ipGP.GetActiveUdpListeners();
            portsInUse = tcpEPs.Concat(udpEPs).Select(p => p.Port).ToList();

            for (int port = beginPort; port <= endPort; ++port)
            {
                if (!portsInUse.Contains(port))
                {
                    return port;
                }
            }

            return 0; // 空きポートが見つからない場合
        }
    }
}
