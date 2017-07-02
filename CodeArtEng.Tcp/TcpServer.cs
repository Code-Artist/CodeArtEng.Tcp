using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace CodeArtEng.Tcp
{
    /// <summary>
    /// Data class for <see cref="TcpServer.ClientConnected"/> 
    /// and <see cref="TcpServer.ClientDisconnected"/> events.
    /// </summary>
    public class TcpServerEventArgs : EventArgs
    {
        /// <summary>
        /// Client object as <see cref="TcpServerConnection"/>
        /// </summary>
        public TcpServerConnection Client { get; set; }
    }

    /// <summary>
    /// Data class for <see cref="TcpServer.ClientConnecting"/> event.
    /// </summary>
    public class TcpServerConnectEventArgs : EventArgs
    {
        /// <summary>
        /// Option to accept / reject incoming connection.
        /// Setting this property to FALSE reject incoming connection.
        /// </summary>
        public bool Accept { get; set; } = true;
        /// <summary>
        /// Client object for incoming connection.
        /// </summary>
        public System.Net.Sockets.TcpClient Client { get; set; }
    }

    /// <summary>
    /// TCP Server implementation with multiple clients handling capability
    /// </summary>
    public class TcpServer : IDisposable
    {
        private TcpListener listener;
        private Thread ConnectionMonitoring = null;

        /// <summary>
        /// Server name. Shown in <see cref="Trace"/> log.
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// TCP Port to listen for incoming connection.
        /// </summary>
        public int Port { get; private set; }
        /// <summary>
        /// Check if Server is started.
        /// </summary>
        public bool IsServerStarted { get { return ConnectionMonitoring != null; } }

        private byte messageDelimiter;
        /// <summary>
        /// Delimiter character to split incoming message to mulitple string package.
        /// Each complete string which terminated with message delimiter will trigger 
        /// the <see cref="TcpServerConnection.MessageReceived"/> once.
        /// </summary>
        /// <remarks>
        /// Recommended to set this property before server start.
        /// Writing to this property will overwrite the <see cref="TcpServerConnection.MessageDelimiter"/>  
        /// for each <see cref="ActiveConnections"/>.
        /// </remarks>
        public byte MessageDelimiter
        {
            get { return messageDelimiter; }
            set
            {
                messageDelimiter = value;
                foreach (TcpServerConnection item in Clients)
                    item.MessageDelimiter = value;
            }
        }

        private List<TcpServerConnection> ActiveConnections = new List<TcpServerConnection>();
        private bool Abort = false;

        /// <summary>
        /// Occurs after server started.
        /// </summary>
        public event EventHandler ServerStarted;
        /// <summary>
        /// Occurs after server stopped.
        /// </summary>
        public event EventHandler ServerStopped;
        /// <summary>
        /// Occurs when incoming connection is detected.
        /// </summary>
        public event EventHandler<TcpServerConnectEventArgs> ClientConnecting;
        /// <summary>
        /// Occrus when incoming connection is accepted.
        /// </summary>
        public event EventHandler<TcpServerEventArgs> ClientConnected;
        /// <summary>
        /// Occurs when a connection is disconnected from server.
        /// </summary>
        public event EventHandler<TcpServerEventArgs> ClientDisconnected;

        /// <summary>
        /// Create a TCP server, <see cref="Name"/> defaulted as "TcpServer".
        /// </summary>
        public TcpServer() { Name = "TcpServer"; }
        /// <summary>
        /// Create a TCP server.
        /// </summary>
        /// <param name="name">Instance name.</param>
        public TcpServer(string name) { Name = name; }

        #region [ IDisposable Support ]

        private bool disposedValue = false; //To detect redundant calls

        /// <summary>
        /// Dispose function, do not call directly.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Stop();
                }
                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        /// <summary>
        ///
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // GC.SuppressFinalize(this);
        }
        #endregion

        /// <summary>
        /// Start server and listen to incoming connection from defined TCP Port.
        /// </summary>
        /// <param name="port">TCP Port to listen for incoming connection.</param>
        /// <exception cref="ArgumentOutOfRangeException">Port number is beyond the range of [0 - 65535] </exception>
        /// <remarks>
        /// Calling this function has no effect once server is started.
        /// </remarks>
        public void Start(int port)
        {
            if (ConnectionMonitoring != null) return;
            Port = port;
            listener = new TcpListener(IPAddress.Any, Port);

            Abort = false;
            ConnectionMonitoring = new Thread(MonitorIncomingConnection);
            ConnectionMonitoring.Name = "Connection Monitoring";
            ConnectionMonitoring.Start();
            ServerStarted?.Invoke(this, null);
            Trace.WriteLine(Name + ":TCP Server Started at port " + Port);
        }

        /// <summary>
        /// Stop the TCP server.
        /// </summary>
        /// <remarks>
        /// Calling this function had no effect once server is stopped.
        /// </remarks>
        public void Stop()
        {
            if (ConnectionMonitoring == null) return;
            Trace.WriteLine(Name + ": Shutting Down TCP Server...");

            Abort = true;
            while (ConnectionMonitoring.IsAlive)
            {
                Thread.Sleep(10);
            }
            DisconnectAllClients();
            ConnectionMonitoring = null;
            ServerStopped?.Invoke(this, null);
        }

        private void DisconnectAllClients()
        {
            lock (ActiveConnections)
            {
                SuppressDisconnectEvent = true;
                foreach (TcpServerConnection item in ActiveConnections)
                {
                    item.Dispose();
                }
                ActiveConnections.Clear();
                SuppressDisconnectEvent = false;
            }
        }

        private void MonitorIncomingConnection()
        {
            listener.Start();
            while (true)
            {
                try
                {
                    if (Abort)
                    {
                        //Terminating Server and Monitoring Loop
                        listener.Stop();
                        Trace.WriteLine(Name + ": TCP Server Stopped.");
                        DisconnectAllClients();
                        return;
                    }

                    if (!listener.Pending())
                    {
                        Thread.Sleep(10);
                    }
                    else
                    {
                        System.Net.Sockets.TcpClient client = listener.AcceptTcpClient();
                        Trace.WriteLine(Name + ": New Connection Detected...");
                        TcpServerConnectEventArgs eArgs = new TcpServerConnectEventArgs() { Client = client };
                        ClientConnecting?.Invoke(this, eArgs);

                        if (eArgs.Accept)
                        {
                            //Connection Accepted
                            TcpServerConnection newConnection = new TcpServerConnection(this, client);
                            newConnection.MessageDelimiter = MessageDelimiter;
                            Trace.WriteLine(Name + ": Connection Accepted: Client = " + newConnection.ClientIPAddress);
                            newConnection.ClientDisconnected += OnClientDisconnected;
                            lock (ActiveConnections) { ActiveConnections.Add(newConnection); }
                            ClientConnected?.Invoke(this, new TcpServerEventArgs() { Client = newConnection });
                        }
                        else
                        {
                            //Connection Refused
                            Trace.WriteLine(Name + ": Connection Rejected.");
                            client.Close();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Trace.WriteLine("[WARNING] " + Name + ": Exception raised from Connection Monitoring Thread!\n" + ex.Message);
                    continue;
                }
            }
        }

        private bool SuppressDisconnectEvent = false;
        private void OnClientDisconnected(object sender, EventArgs e)
        {
            Trace.WriteLine(Name + ": Client disconnected from server.");
            if (SuppressDisconnectEvent) return;
            TcpServerConnection client = sender as TcpServerConnection;
            ClientDisconnected?.Invoke(this, new TcpServerEventArgs() { Client = client });
            client.Dispose();
            ActiveConnections.Remove(client);
        }

        /// <summary>
        /// Return this list of active clients.
        /// Reading from this property will return a list of cloned active clients.
        /// </summary>
        public IList<TcpServerConnection> Clients { get { return new List<TcpServerConnection>(ActiveConnections); } }
    }

    /// <summary>
    /// Data class for <see cref="TcpServerConnection.BytesReceived"/> event.
    /// </summary>
    public class BytesReceivedEventArgs : EventArgs
    {
        internal BytesReceivedEventArgs(TcpServerConnection client, byte[] data, int length)
        {
            Client = client;
            ReceivedBytes = new byte[length];
            Array.Copy(data, ReceivedBytes, length);
        }

        /// <summary>
        /// Client object.
        /// </summary>
        public TcpServerConnection Client { get; set; }
        /// <summary>
        /// Incoming bytes received
        /// </summary>
        public byte[] ReceivedBytes { get; set; }
    }

    /// <summary>
    /// Data class for <see cref="TcpServerConnection.MessageReceived"/> event.
    /// </summary>
    public class MessageReceivedEventArgs : EventArgs
    {
        /// <summary>
        /// Client object.
        /// </summary>
        public TcpServerConnection Client { get; set; }
        /// <summary>
        /// Incoming message.
        /// </summary>
        public string ReceivedMessage { get; set; }
        /// <summary>
        /// Incoming message in byte array.
        /// </summary>
        public byte [] ReceivedBytes { get; set; }
    }

    /// <summary>
    /// Wrapper class for <see cref="TcpClient"/> connection.
    /// </summary>
    public class TcpServerConnection : IDisposable
    {
        private System.Net.Sockets.TcpClient Client;
        private TcpServer Server;
        private NetworkStream TcpStream;
        private int BufferSize;
        private byte[] buffer;
        private string Message;
        private List<byte> MessageBuffer;

        /// <summary>
        /// IP address for connected client.
        /// </summary>
        public IPAddress ClientIPAddress { get; private set; }
        /// <summary>
        /// Delimiter character to split incoming message to mulitple string package.
        /// Each complete string which terminated with message delimiter will trigger 
        /// the <see cref="TcpServerConnection.MessageReceived"/> once.
        /// </summary>
        /// <remarks>The value is assigned by <see cref="TcpServer"/></remarks>
        public byte MessageDelimiter { get; internal set; }

        /// <summary>
        /// Occurs when client is disconnected from server.
        /// </summary>
        public event EventHandler ClientDisconnected;
        /// <summary>
        /// Occurs when one or more bytes is sent from client.
        /// </summary>
        public event EventHandler<BytesReceivedEventArgs> BytesReceived;
        /// <summary>
        /// Occurs when a message terminated with <see cref="MessageDelimiter"/> is received.
        /// </summary>
        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        /// <summary>
        /// Get client connection status.
        /// </summary>
        public bool Connected { get; private set; } = true;

        internal TcpServerConnection(TcpServer parent, System.Net.Sockets.TcpClient client)
        {
            Server = parent;
            Client = client;
            BufferSize = Client.ReceiveBufferSize;
            TcpStream = Client.GetStream();
            ClientIPAddress = ((IPEndPoint)Client.Client.RemoteEndPoint).Address;

            buffer = new byte[BufferSize];
            TcpStream.Flush();
            Message = string.Empty;
            MessageBuffer = new List<byte>();
            BeginRead();
        }

        #region [ IDisposable Support ]

        private bool disposedValue = false; // To detect redundant calls

        /// <summary>
        /// Dispose function, do not call directly.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    Close();
                }
                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        /// <summary>
        /// 
        /// </summary>
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
        }

        #endregion

        /// <summary>
        /// Close client connection
        /// </summary>
        public void Close()
        {
            TcpStream.Close();
            Connected = false;
        }

        private void BeginRead()
        {
            TcpStream.BeginRead(buffer, 0, BufferSize, EndRead, TcpStream);
        }

        private void EndRead(IAsyncResult result)
        {
            try
            {
                int byteRead = TcpStream.EndRead(result);
                if (byteRead == 0)
                {
                    Close();
                    ClientDisconnected?.Invoke(this, null);
                    return;
                }
                else
                {
                    Debug.WriteLine("Received " + byteRead + " bytes.");
                    BytesReceived?.Invoke(this, new BytesReceivedEventArgs(this, buffer, byteRead));
                    
                    //Build string until delimeter character is detected.
                    EventHandler<MessageReceivedEventArgs> OnMessageReceived = MessageReceived;
                    if (OnMessageReceived != null)
                    {

                        for (int x = 0; x < byteRead; x++)
                        {
                            byte b = buffer[x];
                            if (b != MessageDelimiter)
                            {
                                MessageBuffer.Add(b);
                            }
                            else
                            {
                                byte[] messageBytes = MessageBuffer.ToArray();
                                Message = Encoding.ASCII.GetString(messageBytes);
                                MessageBuffer.Clear();
                                OnMessageReceived(this, new MessageReceivedEventArgs() { Client = this, ReceivedMessage = Message , ReceivedBytes = messageBytes});
                            }
                        }
                    }
                    BeginRead();
                }
            }
            catch (ObjectDisposedException)
            {
                ClientDisconnected?.Invoke(this, null);
                return;
            }
            catch (Exception ex)
            {
                Trace.WriteLine("Exception raised from TcpServerConnection: " + ex.Message);
                Trace.WriteLine("Client disconnected from server.");
                ClientDisconnected?.Invoke(this, null);
                return;
            }
        }

        /// <summary>
        /// Write string to client.
        /// </summary>
        /// <param name="message">Message string</param>
        public void WriteToClient(string message)
        {
            WriteToClient(Encoding.ASCII.GetBytes(message));
        }

        /// <summary>
        /// Write byte array to client.
        /// </summary>
        /// <param name="buffer"></param>
        public void WriteToClient(byte[] buffer)
        {
            TcpStream.Write(buffer, 0, buffer.Length);
            TcpStream.Flush();
        }

        /// <summary>
        /// Clear internal message buffer.
        /// </summary>
        /// <remarks>
        /// Internal message buffer store and build message from 
        /// receive bytes until <see cref="MessageDelimiter"/> is 
        /// detected. 
        /// </remarks>
        public void FlushLineBuffer() { MessageBuffer.Clear(); }
    }
}
