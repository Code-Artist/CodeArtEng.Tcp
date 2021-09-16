using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace CodeArtEng.Tcp.Tests
{

    [TestFixture]
    public class TcpServerTest
    {
        private TcpServer Server = new TcpServer();
        private TcpClient Client;
        private int Port;

        private void TcpDelay() { Thread.Sleep(20); }

        [OneTimeSetUp]
        public void Setup()
        {
            Debug.WriteLine("Test Fixture Setup");
            Port = 12300;
            Server.MessageDelimiter = Convert.ToByte('\n');
            Server.Start(Port);
            Server.ClientConnected += Server_ClientConnected;
            TcpDelay();
            Client = new TcpClient("127.0.0.1", Port);
            Client.Connect();
            for (int x = 0; x < 10; x++)
            {
                TcpDelay();
                if (Server.Clients.Count != 0) break;
                Trace.WriteLine(x.ToString());
            }
            Trace.WriteLine("Client Connected");
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            Debug.WriteLine("Test Fixture Tear Down");
            Server.Stop();
            Client = null;
        }

        private void Server_ClientConnected(object sender, TcpServerEventArgs e)
        {
            Debug.WriteLine("EVENT: Server_ClientConnected");
            e.Client.MessageReceived += Client_MessageReceived;
        }

        private string MessageReceiveBuffer;
        private void Client_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            Debug.WriteLine("EVENT: Client_MessageReceived");
            MessageReceiveBuffer = e.ReceivedMessage;
            Debug.WriteLine("RX " + e.ReceivedMessage);
        }

        [Test]
        public void ServerPort() { Assert.AreEqual(12300, Server.Port); }

        [Test]
        public void ServerConnectFailed()
        {
            TcpServer server2 = new TcpServer();
            Assert.Throws<System.Net.Sockets.SocketException>(() => { server2.Start(Port); });
        }

        [Test]
        public void ServerClientIPAddress()
        {
            Assert.AreEqual(1, Server.Clients.Count);
            Assert.AreEqual("127.0.0.1", Server.Clients[0].ClientIPAddress.ToString());
        }

        [Test]
        public void ClientConnected()
        {
            Assert.AreEqual(true, Client.Connected);
        }

        [Test]
        public void ClientConnectDisconnect()
        {
            Assert.AreEqual(1, Server.Clients.Count);
            TcpClient c = new TcpClient("127.0.0.1", 12300);
            c.Connect();
            TcpDelay();
            c.Disconnect();
            TcpDelay();
            Assert.AreEqual(1, Server.Clients.Count);
        }

        [Test]
        public void ServerReceivedBytesFromClient()
        {
            Server.Clients[0].BytesReceived += TcpServerTest_BytesReceived;
            try
            {
                Client.Write("Test");
                TcpDelay();
                Assert.AreEqual(4, Data.Length);
                Assert.AreEqual(new byte[] {
                    (byte)'T', (byte)'e', (byte)'s', (byte)'t' },
                    Data);
            }
            finally
            {
                Server.Clients[0].BytesReceived -= TcpServerTest_BytesReceived;
            }

        }

        private byte[] Data;
        private void TcpServerTest_BytesReceived(object sender, TcpServerDataEventArgs e)
        {
            Data = e.Data;
        }

        [Test]
        public void ServerReceivedStringFromClient()
        {
            Server.Clients[0].FlushLineBuffer();
            MessageReceiveBuffer = string.Empty;
            Client.Write("Test Data");
            TcpDelay();
            Assert.AreEqual(string.Empty, MessageReceiveBuffer);
            Debug.WriteLine("XXXX");

            Client.Write(" 001\nLine 2");
            TcpDelay();
            Assert.AreEqual("Test Data 001", MessageReceiveBuffer);
        }

        private List<string> Messages;

        [Test]
        public void ServerFlushBufferTest()
        {
            Server.Clients[0].FlushLineBuffer();
            MessageReceiveBuffer = string.Empty;
            Client.Write("DUMMY");
            TcpDelay();
            Server.Clients[0].FlushLineBuffer();
            Client.Write("DATA\n");
            TcpDelay();
            Assert.AreEqual("DATA", MessageReceiveBuffer);
        }

        [Test]
        public void ServerReceiveMultipleLinesFromClient()
        {
            Messages = new List<string>();
            Server.Clients[0].FlushLineBuffer();
            Server.Clients[0].MessageReceived += TcpServerTest_MessageReceived;

            try
            {
                Client.Write("Line 1\nLine 2\nLine 3\nDUMMY");
                TcpDelay();
                Assert.AreEqual(3, Messages.Count);
                Assert.AreEqual("Line 1", Messages[0]);
                Assert.AreEqual("Line 3", Messages[2]);
            }
            finally
            {
                Server.Clients[0].MessageReceived -= TcpServerTest_MessageReceived;
            }
        }

        private void TcpServerTest_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            Messages.Add(e.ReceivedMessage);
        }

        [Test]
        public void TcpServerMultipleClients()
        {
            TcpServer server = new TcpServer();
            try
            {
                server.Start(12350);
                TcpDelay();

                for (int x = 0; x < 10; x++)
                {
                    TcpClient client = new TcpClient("127.0.0.1", 12350);
                    client.Connect();
                    TcpDelay();
                }
                TcpDelay();
                Assert.AreEqual(10, server.Clients.Count);
            }
            finally { server.Dispose(); }
        }

    }
}
