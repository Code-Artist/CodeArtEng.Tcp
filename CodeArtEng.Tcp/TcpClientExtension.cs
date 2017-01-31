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
            var prop =
                IPGlobalProperties.GetIPGlobalProperties()
                .GetActiveTcpConnections()
                .SingleOrDefault(x => x.LocalEndPoint.Equals(sender.Client.LocalEndPoint));
            return prop != null ? prop.State : TcpState.Unknown;
        }


    }
}
