using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace CodeArtEng.Tcp
{
    /// <summary>
    /// Data class for <see cref="TcpServerConnection.BytesReceived"/> event.
    /// </summary>
    public class TcpServerDataEventArgs : EventArgs
    {
        internal TcpServerDataEventArgs(TcpServerConnection client, byte[] data, int length)
        {
            Client = client;
            Data = new byte[length];
            Array.Copy(data, Data, length);
        }

        /// <summary>
        /// Client object.
        /// </summary>
        public TcpServerConnection Client { get; set; }
        /// <summary>
        /// Incoming bytes received
        /// </summary>
        public byte[] Data { get; set; }
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
        public byte[] ReceivedBytes { get; set; }
    }

    /// <summary>
    /// Wrapper class for <see cref="TcpClient"/> connection.
    /// </summary>
    public class TcpServerConnection : IDisposable
    {
        private readonly System.Net.Sockets.TcpClient Client;
        private NetworkStream TcpStream;
        private readonly int BufferSize;
        private readonly byte[] buffer;
        private string Message;
        private readonly List<byte> MessageBuffer;

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
        public event EventHandler<TcpServerDataEventArgs> BytesReceived;
        /// <summary>
        /// Occurs when reply is sent to client.
        /// </summary>
        public event EventHandler<TcpServerDataEventArgs> BytesSent;
        /// <summary>
        /// Occurs when a message terminated with <see cref="MessageDelimiter"/> is received.
        /// </summary>
        public event EventHandler<MessageReceivedEventArgs> MessageReceived;

        /// <summary>
        /// Get client connection status.
        /// </summary>
        public bool Connected { get; private set; } = true;

        internal TcpServerConnection(System.Net.Sockets.TcpClient client)
        {
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
            GC.SuppressFinalize(this);
        }

        #endregion

        /// <summary>
        /// Close client connection
        /// </summary>
        public void Close()
        {
            if (TcpStream != null)
            {
                TcpStream.Close();
                TcpStream = null;
            }
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
                if(TcpStream == null)
                {
                    ClientDisconnected?.Invoke(this, null);
                    return;
                }
                
                int byteRead = TcpStream.EndRead(result);
                if (byteRead == 0)
                {
                    Close();
                    ClientDisconnected?.Invoke(this, null);
                    return;
                }
                else
                {
                    //Debug.WriteLine("Received " + byteRead + " bytes.");
                    BytesReceived?.Invoke(this, new TcpServerDataEventArgs(this, buffer, byteRead));

                    //Build string until delimeter character is detected.
                    EventHandler<MessageReceivedEventArgs> OnMessageReceived = MessageReceived;
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
                            OnMessageReceived?.Invoke(this, new MessageReceivedEventArgs() { Client = this, ReceivedMessage = Message, ReceivedBytes = messageBytes });
                            ProcessReceivedMessageCallback?.Invoke(this, Message, messageBytes);
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
        /// Message Received callback prototype
        /// </summary>
        /// <param name="client"></param>
        /// <param name="message"></param>
        /// <param name="messageBytes"></param>
        public delegate void ProcessReceivedMessage(TcpServerConnection client, string message, byte[] messageBytes);
        /// <summary>
        /// Message Received callback. To be implement by derived class to process received message.
        /// </summary>
        public ProcessReceivedMessage ProcessReceivedMessageCallback { get; set; }

        /// <summary>
        /// Write string to client.
        /// </summary>
        /// <param name="message">Message string</param>
        public void WriteToClient(string message)
        {
            WriteToClient(Encoding.ASCII.GetBytes(message));
        }

        /// <summary>
        /// Write string to client terminate with \r\n [0x0D 0x0A]
        /// </summary>
        /// <param name="message"></param>
        public void WriteLineToClient(string message)
        {
            WriteToClient(Encoding.ASCII.GetBytes(message + "\r\n"));
        }

        /// <summary>
        /// Write byte array to client.
        /// </summary>
        /// <param name="buffer"></param>
        public void WriteToClient(byte[] buffer)
        {
            if (buffer == null) throw new ArgumentNullException(nameof(buffer));

            TcpStream.Write(buffer, 0, buffer.Length);
            BytesSent?.Invoke(this, new TcpServerDataEventArgs(this, buffer, buffer.Length));
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
