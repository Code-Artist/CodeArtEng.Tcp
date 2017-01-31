using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using System.Net.Sockets;
using System.Diagnostics;

namespace CodeArtEng.Tcp.Tests
{
    [TestFixture]
    public class TcpClientConnectionTests
    {
        [Test, ExpectedException(typeof(SocketException))]
        public void ClientConnect_InvalidServer_SocketException()
        {
            TcpClient client = new TcpClient("127.0.0.1", 1000);
            client.Connect();
        }

        [Test]
        public void ClientConnectDisconnect()
        {
            using (TcpServer server = new TcpServer("TestServer"))
            {
                server.Start(12000);

                TcpClient client = new TcpClient("localhost", 12000);
                client.Connect();
                Thread.Sleep(200);
                Assert.AreEqual(true, client.Connected);
                Assert.AreEqual(1, server.Clients.Count);
                client.Disconnect();
                Thread.Sleep(200);
                Assert.AreEqual(0, server.Clients.Count);

            }
        }

        [Test]
        public void ServerDisconnected_ClientWrite_RaiseIOException()
        {
            using (TcpServer server = new TcpServer("TestServer2"))
            {
                server.Start(12100);
                TcpClient client = new TcpClient("localhost", 12100);
                client.Connect();
                Thread.Sleep(200);
                Assert.AreEqual(true, client.Connected);
                server.Stop();
                Thread.Sleep(200);
                Assert.AreEqual(0, server.Clients.Count);
                Assert.AreEqual(false, client.Connected); //Property does not reflec the connection status.
            }
        }

        [Test, ExpectedException(typeof(ArgumentOutOfRangeException))]
        public void ClientConnect_InvalidPort()
        {
            TcpClient client = new TcpClient("127.0.0.1", 90000);
            client.Connect();
        }
    }

    [TestFixture]
    public class TcpClientTests
    {
        private void TcpDelay() { Thread.Sleep(10); }
        private TcpServer Server = new TcpServer();
        private TcpClient Client;

        [TestFixtureSetUp]
        public void Setup()
        {
            Debug.WriteLine("Test Fixture Setup");
            int port = 16000;
            Server.MessageDelimiter = Convert.ToByte('\t');
            Server.Start(port);
            Server.ClientConnected += Server_ClientConnected;
            Thread.Sleep(500);
            Client = new TcpClient("127.0.0.1", port);
            Client.Connect();
            for (int x = 0; x < 10; x++)
            {
                Thread.Sleep(500);
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

        private void Client_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            Debug.WriteLine("EVENT: Client_MessageReceived");
            string MessageReceiveBuffer = e.ReceivedMessage;
            Debug.WriteLine("RX: " + e.ReceivedMessage);
            e.Client.WriteToClient("Server [" + MessageReceiveBuffer + "] ACK.");
        }

        [Test]
        public void HostName()
        {
            Assert.AreEqual("127.0.0.1", Client.HostName);
        }
        
        [Test]
        public void ConnectionPort()
        {
            Assert.AreEqual(16000, Client.Port);
        }

        [Test]
        public void CommunicateWithServer()
        {
            Client.Write("Message from Client\t");
            TcpDelay();
            string returnString = Client.ReadString();
            Assert.AreEqual("Server [Message from Client] ACK.", returnString);
            
        }

    }
}
