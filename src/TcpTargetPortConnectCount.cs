using System.Net.NetworkInformation;

namespace HmChatGptInBrowser
{
    public partial class Program
    {
        public static int TcpTargetPortConnectCount(int port)
        {
            IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
            TcpConnectionInformation[] connections = properties.GetActiveTcpConnections();

            int httpConnections = 0;
            foreach (TcpConnectionInformation connection in connections)
            {
                if (connection.LocalEndPoint.Port == port && connection.State == TcpState.Established)
                {
                    httpConnections++;
                }
            }

            return httpConnections;
        }

    }
}
