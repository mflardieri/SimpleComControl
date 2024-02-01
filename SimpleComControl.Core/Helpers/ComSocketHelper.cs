using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace SimpleComControl.Core.Helpers
{
    public static class ComSocketHelper
    {
        public static int TcpOpenPort()
        {
            TcpListener l = new(IPAddress.Loopback, 0);
            l.Start();
            int port = ((IPEndPoint)l.LocalEndpoint).Port;
            l.Stop();
            return port;
        }
        public static bool TcpNetWorkInfoAvailablity(int port)
        {
            if (port >= 0)
            {

                IPGlobalProperties ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
                TcpConnectionInformation[] tcpConnInfoArray = ipGlobalProperties.GetActiveTcpConnections();

                foreach (TcpConnectionInformation tcpi in tcpConnInfoArray)
                {
                    if (tcpi.LocalEndPoint.Port == port)
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }
    }
}
