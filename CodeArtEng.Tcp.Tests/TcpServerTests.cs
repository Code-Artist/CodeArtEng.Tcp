using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace CodeArtEng.Tcp.Tests
{

    [TestFixture]
    public class TcpServerConnectionTests
    {
        private void TcpDelay() { Thread.Sleep(50); }
        private bool ServerStartedEvent;
        private bool ServerStoppedEvent;

        public void SubscribeEvent(TcpServer server)
        {
            server.ServerStarted += (sender, e) => { ServerStartedEvent = true; Trace.WriteLine("Server Started"); };
            server.ServerStopped += (sender, e) => { ServerStoppedEvent = true; Trace.WriteLine("Server Stopped"); };
            ServerStartedEvent = false;
            ServerStoppedEvent = false;
        }

        [Test]
        public void StartStopServerPort1000()
        {
            using (TcpServer server = new TcpServer())
            {
                SubscribeEvent(server);
                server.Start(200);
                Assert.AreEqual(true, ServerStartedEvent);
                Assert.AreEqual(true, server.IsServerStarted);
                Assert.AreEqual(false, ServerStoppedEvent);
                server.Stop();
                Assert.AreEqual(true, ServerStoppedEvent);
                Assert.AreEqual(false, server.IsServerStarted);
            }
        }

        [Test, ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void StartServerInvalidPort_ArgumentOutofRangeException()
        {
            using (TcpServer server = new TcpServer())
            {
                ServerStartedEvent = false;
                server.Start(99999);
                Assert.AreEqual(false, ServerStartedEvent);
            }
        }

        private bool ClientConnectingEvent = false;
        private bool ClientConnectedEvent = false;

        [Test]
        public void ClientConnectEventsTest()
        {
            using (TcpServer server = new TcpServer())
            {
                int Port = 10000;
                SubscribeEvent(server);
                server.Start(Port);
                server.ClientConnecting += (sender, e) => { ClientConnectingEvent = true; Trace.WriteLine("Client Connecting"); };
                server.ClientConnected += (sender, e) => { ClientConnectedEvent = true; Trace.WriteLine("Client Connected"); };
                ClientConnectingEvent = false;
                ClientConnectedEvent = false;

                TcpClient client = new TcpClient("127.0.0.1", Port);
                client.Connect();
                Assert.AreEqual(true, client.Connected);
                Trace.WriteLine("Client Connected to Server.");
                TcpDelay();
                Assert.AreEqual(true, ClientConnectingEvent);
                Assert.AreEqual(true, ClientConnectedEvent);
                Assert.AreEqual(1, server.Clients.Count);
                Trace.WriteLine("Test End");
            }
        }

        [Test]
        public void ServerStop()
        {
            using (TcpServer server = new TcpServer())
            {
                int Port = 18000;
                server.Start(Port);
                TcpClient client = new TcpClient("127.0.0.1", Port);
                client.Connect();

                TcpDelay();
                Assert.AreEqual(1, server.Clients.Count);
                server.Stop();
                Assert.AreEqual(0, server.Clients.Count);
            }
        }

        private bool ClientDisconnectedEvent = false;

        [Test]
        public void ClientDisconnectFromServer()
        {
            ClientDisconnectedEvent = false;
            using (TcpServer server = new TcpServer())
            {
                int Port = 10010;
                server.Start(Port);
                server.ClientDisconnected += (sender, e) => { ClientDisconnectedEvent = true; };
                TcpClient client = new TcpClient("127.0.0.1", Port);
                client.Connect();

                TcpDelay();
                Assert.AreEqual(1, server.Clients.Count);
                client.Disconnect();
                TcpDelay();
                Assert.AreEqual(0, server.Clients.Count);
                Assert.AreEqual(true, ClientDisconnectedEvent);
            }
        }

        [Test]
        public void ServerTerminateClientConnection()
        {
            int ClientCount = 10;
            using (TcpServer server = new TcpServer())
            {
                //Prevent TcpClient dispose by GC
                List<TcpClient> Clients = new List<TcpClient>();

                server.Start(10020);
                TcpDelay();
                for (int x = 0; x < ClientCount; x++)
                {
                    TcpClient client = new TcpClient("127.0.0.1", 10020);
                    Clients.Add(client);
                    client.Connect();
                }

                TcpDelay();
                Assert.AreEqual(ClientCount, server.Clients.Count);

                server.Clients[5].Close();
                TcpDelay();
                Trace.WriteLine("Connection Closed.");
                Assert.AreEqual(ClientCount - 1, server.Clients.Count);

                Trace.WriteLine("Test Ended.");
                Clients.Clear();
            }
        }

        [Test]
        public void MultipleClientsConnectedToServer()
        {
            int ClientCount = 10;
            using (TcpServer server = new TcpServer())
            {
                //Prevent TcpClient dispose by GC
                List<TcpClient> Clients = new List<TcpClient>();

                server.Start(10020);
                TcpDelay();
                for (int x = 0; x < ClientCount; x++)
                {
                    TcpClient client = new TcpClient("127.0.0.1", 10020);
                    Clients.Add(client);
                    client.Connect();
                }

                TcpDelay();
                Assert.AreEqual(ClientCount, server.Clients.Count);
                Trace.WriteLine("Test Ended.");
                Clients.Clear();
            }
        }
    }

    [TestFixture]
    public class TcpServerTest
    {
        private TcpServer Server = new TcpServer();
        private TcpClient Client;

        private void TcpDelay() { Thread.Sleep(10); }

        [TestFixtureSetUp]
        public void Setup()
        {
            Debug.WriteLine("Test Fixture Setup");
            int port = 12300;
            Server.MessageDelimiter = Convert.ToByte('\n');
            Server.Start(port);
            Server.ClientConnected += Server_ClientConnected;
            TcpDelay();
            Client = new TcpClient("127.0.0.1", port);
            Client.Connect();
            for (int x = 0; x < 10; x++)
            {
                TcpDelay();
                if (Server.Clients.Count != 0) break;
                Trace.WriteLine(x.ToString());
            }
            Trace.WriteLine("Client Connected");
        }

        [TestFixtureTearDown]
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
        private void TcpServerTest_BytesReceived(object sender, BytesReceivedEventArgs e)
        {
            Data = e.ReceivedBytes;
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

    }
}
