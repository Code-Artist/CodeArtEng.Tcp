using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
using System.Text;

namespace CodeArtEng.Tcp
{
    /// <summary>
    /// TCP Client Implementation
    /// </summary>
    public class TcpClient
    {
        private System.Net.Sockets.TcpClient Client = new System.Net.Sockets.TcpClient();
        private List<byte> ByteBuffer = new List<byte>();
        private byte[] FixedBuffer;
        private int BufferSize;

        /// <summary>
        /// Server's IP Address / Host Name.
        /// </summary>
        public string HostName { get; set; }

        /// <summary>
        /// TCP Server connection port.
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Constructor
        /// </summary>
        public TcpClient() { }
        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="hostName">Server Host</param>
        /// <param name="port">TCP Server Port</param>
        public TcpClient(string hostName, int port)
        {
            HostName = hostName;
            Port = port;
        }

        /// <summary>
        /// Check if connection with Server stil active.
        /// </summary>
        public bool Connected { get { return Client.IsConnected(); } }

        /// <summary>
        /// Attempt to establish connection with server.
        /// </summary>
        public void Connect()
        {
            Client.Connect(HostName, Port);
            BufferSize = Client.ReceiveBufferSize;
            FixedBuffer = new byte[BufferSize];
        }

        /// <summary>
        /// Disconnect client from server.
        /// </summary>
        public void Disconnect()
        {
            Client.Close();
        }

        /// <summary>
        /// Write string to server.
        /// </summary>
        /// <param name="message"></param>
        /// <remarks>Automatic check and establish connection with server.</remarks>
        public void Write(string message)
        {
            byte[] outputBuffer = Encoding.ASCII.GetBytes(message);
            Write(outputBuffer);
        }

        /// <summary>
        /// Write byte array to server.
        /// </summary>
        /// <param name="dataBytes"></param>
        /// <remarks>Automatic check and establish connection with server.</remarks>
        public void Write(byte [] dataBytes)
        {
            if (!Connected) Connect();

            NetworkStream tcpStream = Client.GetStream();
            Debug.WriteLine("Output Length = " + dataBytes.Length);
            tcpStream.Write(dataBytes, 0, dataBytes.Length);
            tcpStream.Flush();
        }

        /// <summary>
        /// Read byte array from server.
        /// </summary>
        /// <returns></returns>
        /// <remarks>Automatic check and establish connection with server.</remarks>
        public byte [] ReadBytes()
        {
            if (!Connected) Connect();

            NetworkStream tcpStream =  Client.GetStream();
            while(tcpStream.DataAvailable)
            {
                int readByte = tcpStream.Read(FixedBuffer, 0, BufferSize);
                if (readByte == 0) break;
                else if (readByte == BufferSize)
                    ByteBuffer.AddRange(FixedBuffer);

                else
                {
                    for (int x = 0; x < readByte; x++)
                        ByteBuffer.Add(FixedBuffer[x]);
                }
            }

            return ByteBuffer.ToArray();
        }

        /// <summary>
        /// Read string from server.
        /// </summary>
        /// <returns></returns>
        /// <remarks>Automatic check and establish connection with server.</remarks>
        public string ReadString()
        {
            return ASCIIEncoding.ASCII.GetString(ReadBytes());
        }
    }
}
