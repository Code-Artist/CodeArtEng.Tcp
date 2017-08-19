using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace CodeArtEng.Tcp
{
    /// <summary>
    /// TCP Client Implementation
    /// </summary>
    public class TcpClient
    {
        private System.Net.Sockets.TcpClient Client = new System.Net.Sockets.TcpClient();
        private NetworkStream TcpStream;
        private List<byte> ByteBuffer = new List<byte>();
        private byte[] FixedBuffer;
        private int BufferSize;
        private bool ConnectState = false;

        private bool IncomingDataMonitoringThreadActive = true;
        private Thread IncomingDataMonitoring = null;

        private Thread ConnectionMonitoring = null;
        
        /// <summary>
        /// Occurs when incoming message is detected on input message buffer.
        /// </summary>
        /// <remarks>Event subscription had to be done before calling <see cref="Connect"/>. 
        /// A monitoring thread will be launched to watch <see cref="NetworkStream.DataAvailable"/> flag in function <see cref="Connect"/> 
        /// if and only if DataReceived event is subscribed.
        /// </remarks>
        public event EventHandler DataReceived;

        /// <summary>
        /// Occurs when connection is established / disconnected.
        /// </summary>
        /// <remarks>Handled by <see cref="Connected"/> property.</remarks>
        public event EventHandler ConnectionStatusChanged;

        /// <summary>
        /// Server's IP Address / Host Name.
        /// </summary>
        public string HostName { get; set; }

        /// <summary>
        /// TCP Server connection port.
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Gets or sets the amount of time in miliseconds to wait for a valid response from server.
        /// <see cref="TimeoutException"/> raised if for read operation if no response received.
        /// </summary>
        /// <value>Default value = -1 (Wait Forever) </value>
        public int ReadTimeout { get; set; } = -1;

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
        public bool Connected
        {
            get
            {
                bool state = Client.IsConnected();
                if (state == ConnectState) return ConnectState;
                ConnectState = state;
                ConnectionStatusChanged?.Invoke(this, null);
                return state;
            }

            private set
            {
                if (value == ConnectState) return;
                ConnectState = value;
                ConnectionStatusChanged?.Invoke(this, null);
            }
        }

        /// <summary>
        /// Attempt to establish connection with server.
        /// </summary>
        public void Connect()
        {
            Disconnect();
            Client.Connect(HostName, Port);
            TcpStream = Client.GetStream();
            Connected = true;
            BufferSize = Client.ReceiveBufferSize;
            FixedBuffer = new byte[BufferSize];

            if (DataReceived != null)
            {
                IncomingDataMonitoringThreadActive = true;
                IncomingDataMonitoring = new Thread(MonitorIncomingData);
                IncomingDataMonitoring.Start();
            }

            ConnectionMonitoring = new Thread(MonitorConnection);
            ConnectionMonitoring.Start();
        }

        /// <summary>
        /// Disconnect client from server.
        /// </summary>
        public void Disconnect()
        {
            //Gentle close, terminating thread properly
            IncomingDataMonitoringThreadActive = false;
            Thread.Sleep(10);
            IncomingDataMonitoring?.Abort();
            IncomingDataMonitoring = null;

            TcpStream?.Close();
            TcpStream = null;

            Connected = false;
            Client.Close();
            Client = new System.Net.Sockets.TcpClient();
        }

        /// <summary>
        /// Write string to server.
        /// </summary>
        /// <param name="message"></param>
        /// <remarks>Automatic check and establish connection with server.</remarks>
        public void Write(string message)
        {
            if (!Connected) Connect();
            byte[] outputBuffer = Encoding.ASCII.GetBytes(message);
            Write(outputBuffer);
        }

        /// <summary>
        /// Write byte array to server.
        /// </summary>
        /// <param name="dataBytes"></param>
        /// <remarks>Automatic check and establish connection with server.</remarks>
        public void Write(byte[] dataBytes)
        {
            if (!Connected) Connect();

            TcpStream.Write(dataBytes, 0, dataBytes.Length);
            TcpStream.Flush();
        }

        /// <summary>
        /// Discard input buffer in TCP Stream.
        /// </summary>
        public void FlushInputBuffer()
        {
            while (TcpStream.DataAvailable) { TcpStream.Read(FixedBuffer, 0, BufferSize); }
        }

        /// <summary>
        /// Read byte array from server.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="TimeoutException">No response recevied from server after defined <see cref="ReadTimeout"/> period.</exception>
        /// <remarks>Automatic check and establish connection with server.</remarks>
        public byte[] ReadBytes()
        {
            if (!Connected) Connect();
            ByteBuffer.Clear();

            DateTime tStart = DateTime.Now;
            if (ReadTimeout != -1)
            {
                while (!TcpStream.DataAvailable)
                {
                    if ((DateTime.Now - tStart).TotalMilliseconds > ReadTimeout)
                        throw new TimeoutException("Read timeout, no response from server!");
                    System.Threading.Thread.Sleep(10);
                }
            }

            while (true)
            {
                int readByte = TcpStream.Read(FixedBuffer, 0, BufferSize);
                if (readByte == 0) break;
                else if (readByte == BufferSize)
                {
                    ByteBuffer.AddRange(FixedBuffer);
                }
                else
                {
                    byte[] data = new byte[readByte];
                    System.Array.Copy(FixedBuffer, data, readByte);
                    ByteBuffer.AddRange(data);
                }

                if (!TcpStream.DataAvailable) break;
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
            if (!Connected) Connect();
            return ASCIIEncoding.ASCII.GetString(ReadBytes());
        }

        private void MonitorIncomingData()
        {
            bool incomingData = TcpStream.DataAvailable;
            while (IncomingDataMonitoringThreadActive) //Loop forever
            {
                if (!incomingData)
                {
                    if (TcpStream.DataAvailable)
                    {
                        incomingData = true;
                        DataReceived?.Invoke(this, null);
                    }
                }
                else
                {
                    if (!TcpStream.DataAvailable)
                        incomingData = false;
                }
                bool isConnect = Connected; //Read connection status
                Thread.Sleep(1);
            }

        }

        private void MonitorConnection()
        {
            while(Connected)
            {
                Thread.Sleep(100);
            }
        }
    }
}
