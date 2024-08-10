using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace CodeArtEng.Tcp
{
    //ToDo: TcpClient, receive message without delimiter? Use timeout to predict end of message.

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
    /// TCP Server Message Receive End Mode
    /// </summary>
    public enum TcpServerMessageEndMode
    {
        /// <summary>
        /// Message end when delimiter character detected. (Default)
        /// </summary>
        Delimiter,
        /// <summary>
        /// Message end by timeout.
        /// </summary>
        Timeout
    }

    /// <summary>
    /// TCP Server implementation with multiple clients handling capability
    /// </summary>
    public class TcpServer : IDisposable
    {
        private TcpListener Listener;
        private Thread ConnectionMonitoring = null;

        /// <summary>
        /// Server name. Shown in <see cref="Trace"/> log.
        /// </summary>
        public string Name { get; private set; }
        /// <summary>
        /// TCP Port to listen for incoming connection.
        /// </summary>
        public int Port { get; protected set; }
        /// <summary>
        /// Check if Server is started.
        /// </summary>
        public bool IsServerStarted { get { return ConnectionMonitoring != null; } }

        /// <summary>
        /// Delimiter character to split incoming message to mulitple string package when <see cref="MessageReceivedEndMode"/>
        /// is set to <see cref="TcpServerMessageEndMode.Delimiter"/>
        /// Each complete string which terminated with message delimiter will trigger 
        /// the <see cref="TcpServerConnection.MessageReceived"/> once. Default value is '\r' (0x0D).
        /// </summary>
        /// <remarks>
        /// Recommended to set this property before server start.
        /// Writing to this property will overwrite the <see cref="TcpServerConnection.MessageDelimiter"/>  
        /// for each <see cref="ActiveConnections"/>.
        /// </remarks>
        public byte MessageDelimiter { get; set; } = Convert.ToByte('\r');
        /// <summary>
        /// When <see cref="MessageReceivedEndMode"/> set to <see cref="TcpServerMessageEndMode.Timeout"/>,
        /// Server will wait for defined timeout (ms) before trigger <see cref="TcpServerConnection.MessageReceived"/> event.
        /// Default value is 100ms
        /// </summary>
        public int InterMessageTimeout { get; set; } = 100;
        /// <summary>
        /// Message receive mode. See <see cref="TcpServerMessageEndMode"/>
        /// </summary>
        public TcpServerMessageEndMode MessageReceivedEndMode { get; set; } = TcpServerMessageEndMode.Delimiter;

        private readonly List<TcpServerConnection> ActiveConnections = new List<TcpServerConnection>();
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
        /// Occurs when incoming connection detected, before compare with max clients.
        /// Subscribe this event to decide if any of the existing client should be dropped.
        /// </summary>
        /// <remarks>This event is triggered only if <see cref="MaxClients"/> is greater than 0.</remarks>
        public event EventHandler<TcpServerConnectEventArgs> IncomingConnection;
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
            GC.SuppressFinalize(this);
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
        /// <exception cref="SocketException">Unable to start TCP Server on selected port.</exception>
        public void Start(int port)
        {
            if (ConnectionMonitoring != null) return;
            if (port < 0 || port > 65535)
                throw new ArgumentOutOfRangeException("Port must be between 0 and 65535.");
            Port = port;
            Listener = new TcpListener(IPAddress.Any, Port);
            Listener.Start();

            Abort = false;
            ConnectionMonitoring = new Thread(MonitorIncomingConnection) { Name = "Connection Monitoring" };
            ConnectionMonitoring.Start();
            ServerStarted?.Invoke(this, null);
            Trace.WriteLine(Name + ": TCP Server Started at port " + Port);
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
            Trace.WriteLine(Name + ": Shut Down.");
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

        protected virtual void OnIncomingConnection(TcpServerConnectEventArgs e)
        {
            IncomingConnection?.Invoke(this, null);
        }

        private void MonitorIncomingConnection()
        {
            while (true)
            {
                try
                {
                    if (Abort)
                    {
                        //Terminating Server and Monitoring Loop
                        Listener.Stop();
                        Trace.WriteLine(Name + ": TCP Server Stopped.");
                        DisconnectAllClients();
                        return;
                    }

                    if (!Listener.Pending())
                    {
                        Thread.Sleep(10);
                    }
                    else
                    {
                        System.Net.Sockets.TcpClient client = Listener.AcceptTcpClient();
                        Trace.WriteLine(Name + ": New Connection Detected...");

                        if (MaxClients > 0)
                        {
                            TcpServerConnectEventArgs arg = new TcpServerConnectEventArgs() { Client = client };
                            OnIncomingConnection(arg);

                            //MAX Clients Restriction applied
                            if (Clients.Count >= MaxClients)
                            {
                                //Number of connection exceeds, reject incoming connection.
                                Trace.WriteLine(Name + ": Connection Rejected, exceed MAX allowed clients!");
                                byte[] msgBuffer = Encoding.ASCII.GetBytes("Connection Rejected, exceed MAX allowed clients!\r\n");
                                client.GetStream().Write(msgBuffer, 0, msgBuffer.Length);
                                client.GetStream().Flush();
                                client.Close();
                                continue;
                            }
                        }

                        TcpServerConnectEventArgs eArgs = new TcpServerConnectEventArgs() { Client = client };
                        ClientConnecting?.Invoke(this, eArgs);

                        if (eArgs.Accept)
                        {
                            //Connection Accepted
                            TcpServerConnection newConnection = new TcpServerConnection(this, client);
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
        /// Maximum concurrent clients allowed.
        /// Unlimited clients allowed if value set to 0 and below.
        /// Changing the value of this property does not affect existing connections.
        /// </summary>
        public int MaxClients { get; set; } = 0;

        /// <summary>
        /// Return this list of active clients.
        /// Reading from this property will return a list of cloned active clients.
        /// </summary>
        public IList<TcpServerConnection> Clients { get { return new List<TcpServerConnection>(ActiveConnections); } }
    }

}
