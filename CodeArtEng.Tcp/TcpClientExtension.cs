using System.Linq;
using System.Net.NetworkInformation;

namespace System.Net.Sockets
{
    /// <summary>
    /// TCP Client Helper Class.
    /// </summary>
    public static class TcpClientExtension
    {
        /// <summary>
        /// Extension method to check if TCP client is connected.
        /// </summary>
        /// <param name="sender"></param>
        /// <returns></returns>
        public static bool IsConnected(this TcpClient sender)
        {
            return GetState(sender) == TcpState.Established;
        }

        private static TcpState GetState(TcpClient sender)
        {
            if (sender == null) return TcpState.Unknown;
            if (sender.Client == null) return TcpState.Closed;

            try
            {
                //In .NET 5.0, sender.Client.LocalEndPoint in IPV6 format.
                IPEndPoint clientEndPoint = sender.Client.LocalEndPoint as IPEndPoint;
                if (clientEndPoint == null) return TcpState.Closed;

                //Check and convert to IPV4 if client end point address is IPV6
                IPAddress senderIPV4 = clientEndPoint.Address.IsIPv4MappedToIPv6 ? clientEndPoint.Address.MapToIPv4() : clientEndPoint.Address;
                int port = (sender.Client.LocalEndPoint as IPEndPoint).Port;

                var prop =
                    IPGlobalProperties.GetIPGlobalProperties()
                    .GetActiveTcpConnections() //Return TCP connection in IPV4 format
                    .SingleOrDefault(x => (x.LocalEndPoint as IPEndPoint)?.Port == port && (x.LocalEndPoint as IPEndPoint).Address.Equals(senderIPV4));
                return prop != null ? prop.State : TcpState.Unknown;
            }
            catch { return TcpState.Unknown; }
        }


    }
}
