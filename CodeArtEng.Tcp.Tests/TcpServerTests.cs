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
        public void ServerPort() { Assert.That(Server.Port, Is.EqualTo(12300)); }

        [Test]
        public void ServerConnectFailed()
        {
            TcpServer server2 = new TcpServer();
            Assert.Throws<System.Net.Sockets.SocketException>(() => { server2.Start(Port); });
        }

        [Test]
        public void ServerClientIPAddress()
        {
            Assert.That(Server.Clients.Count, Is.EqualTo(1));
            Assert.That(Server.Clients[0].ClientIPAddress.ToString(), Is.EqualTo("127.0.0.1"));
        }

        [Test]
        public void ClientConnected()
        {
            Assert.That(Client.Connected, Is.EqualTo(true));
        }

        [Test]
        public void ClientConnectDisconnect()
        {
            Assert.That(Server.Clients.Count, Is.EqualTo(1));
            TcpClient c = new TcpClient("127.0.0.1", 12300);
            c.Connect();
            TcpDelay();
            c.Disconnect();
            TcpDelay();
            Assert.That(Server.Clients.Count, Is.EqualTo(1));
        }

        [Test]
        public void ServerReceivedBytesFromClient()
        {
            Server.Clients[0].BytesReceived += TcpServerTest_BytesReceived;
            try
            {
                Client.Write("Test");
                TcpDelay();
                Assert.That(Data.Length, Is.EqualTo(4));
                Assert.That(new byte[] { (byte)'T', (byte)'e', (byte)'s', (byte)'t' }, Is.EqualTo(Data));
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
            Assert.That(MessageReceiveBuffer, Is.EqualTo(string.Empty));
            Debug.WriteLine("XXXX");

            Client.Write(" 001\nLine 2");
            TcpDelay();
            Assert.That(MessageReceiveBuffer, Is.EqualTo("Test Data 001"));
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
            Assert.That(MessageReceiveBuffer, Is.EqualTo("DATA"));
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
                Assert.That(Messages.Count, Is.EqualTo(3));
                Assert.That(Messages[0], Is.EqualTo("Line 1"));
                Assert.That(Messages[2], Is.EqualTo("Line 3"));
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
                Assert.That(server.Clients.Count, Is.EqualTo(10));
            }
            finally { server.Dispose(); }
        }

    }
}
