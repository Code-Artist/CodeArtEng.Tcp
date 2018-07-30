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
    public class TcpClient : IDisposable
    {
        private System.Net.Sockets.TcpClient Client = new System.Net.Sockets.TcpClient();
        private NetworkStream TcpStream;
        private byte[] FixedBuffer;
        private int BufferSize;
        private bool ConnectState = false;
        private readonly object LockHandler = new object();

        private bool MonitoringThreadActive = true;
        private Thread IncomingDataMonitoring = null;

        private Thread ConnectionMonitoring = null;

        /// <summary>
        /// Occurs when incoming message is detected on input message buffer, cross thread event.
        /// </summary>
        /// <remarks>Event subscription had to be done before calling <see cref="Connect"/>. 
        /// A monitoring thread will be launched to watch <see cref="NetworkStream.DataAvailable"/> flag in function <see cref="Connect"/> 
        /// if and only if DataReceived event is subscribed.
        /// </remarks>
        public event EventHandler<TcpDataReceivedEventArgs> DataReceived;

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

            MonitoringThreadActive = true;
            IncomingDataMonitoring = new Thread(MonitorIncomingData);
            IncomingDataMonitoring.Name = "TCP Client Data Monitoring @ " + Port.ToString();
            IncomingDataMonitoring.Start();

            ConnectionMonitoring = new Thread(MonitorConnection);
            ConnectionMonitoring.Name = "TCP Client Connection Monitoring @ " + Port.ToString();
            ConnectionMonitoring.Start();

            ConnectionStatusChanged?.Invoke(this, null);
        }

        /// <summary>
        /// Disconnect client from server.
        /// </summary>
        public void Disconnect()
        {
            //Gentle close, terminating thread properly
            TerminateThreadsAndTCPStream();

            Connected = false;
            Client.Close();
            Client = new System.Net.Sockets.TcpClient();
        }

        private void TerminateThreadsAndTCPStream()
        {
            MonitoringThreadActive = false;
            IncomingDataMonitoring = null;
            ConnectionMonitoring = null;

            TcpStream?.Close();
            TcpStream = null;
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
            lock (LockHandler)
            {
                if (!Connected) Connect();

                DateTime tStart = DateTime.Now;
                if (ReadTimeout != -1)
                {
                    while (!TcpStream.DataAvailable)
                    {
                        if ((DateTime.Now - tStart).TotalMilliseconds > ReadTimeout)
                            throw new TimeoutException("Read timeout, no response from server!");
                        Thread.Sleep(10);
                    }
                }
                return readRawBytes();
            }
        }

        private byte[] readRawBytes()
        {
            List<byte> ByteBuffer = new List<byte>();
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
                    Array.Copy(FixedBuffer, data, readByte);
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
            bool incomingData = false;
            while (MonitoringThreadActive) //Loop forever
            {
                if (DataReceived == null)
                {
                    continue;
                }

                if (!incomingData)
                {
                    if (TcpStream.DataAvailable)
                    {
                        lock (LockHandler)
                        {
                            incomingData = true;
                            byte[] data = readRawBytes();
                            DataReceived?.Invoke(this, new TcpDataReceivedEventArgs() { Data = data });
                        }
                    }
                }
                else
                {
                    if (!TcpStream.DataAvailable)
                        incomingData = false;
                }
                bool isConnect = Connected; //Read connection status
                Thread.Sleep(50);
            }
        }

        private void MonitorConnection()
        {
            while (MonitoringThreadActive)
            {
                if (ConnectState)
                {
                    if (!Connected) ConnectionStatusChanged?.Invoke(this, null);
                }
                Thread.Sleep(50);
            }
        }

        #region [ IDisposable Support ]
        private bool disposedValue = false; // To detect redundant calls

        /// <summary>
        /// Dispose object
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    TerminateThreadsAndTCPStream();
                    Client.Close();
                    Client = null;
                }

                disposedValue = true;
            }
        }


        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }
        #endregion
    }
}
