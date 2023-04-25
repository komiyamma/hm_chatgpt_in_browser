using System.Net.NetworkInformation;

namespace HmChatGptInBrowser
{
    public partial class Program
    {
        // ★ このような関数は使えないので注意!!
        // Blazorならギリギリ使えるかもしれないが、汎用的には、使えない。
        // httpでは接続している状態でも長期的(5分程度)開いたまま、放置すると、
        // TcpConnectionInformationのリストからドロップアウトしてしまう。
        public static int TcpTargetPortConnectCount(int port)
        {
            IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();
            TcpConnectionInformation[] connections = properties.GetActiveTcpConnections();

            int httpConnections = 0;
            foreach (TcpConnectionInformation connection in connections)
            {
                if (connection.LocalEndPoint.Port == port && connection.State == TcpState.Established || connection.State == TcpState.TimeWait) {
                    httpConnections++;
                }
            }

            return httpConnections;
        }

    }
}
