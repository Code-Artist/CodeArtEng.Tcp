using NUnit.Framework;
using System;
using System.Diagnostics;
using System.Net.Sockets;
using System.Threading;

namespace CodeArtEng.Tcp.Tests
{
    [TestFixture]
    public class TcpClientConnectionTests
    {
        [Test]
        public void ClientConnect_InvalidServer_SocketException()
        {

            TcpClient client = new TcpClient("127.0.0.1", 1000);

            //.NET 5.0 and .NET 6.0 throw SocketExceptionFactory+ExtendedSocketException, derrived from SocketException
                        //Assert.Throws<SocketException>(() => { client.Connect(); });
            try { client.Connect(); }
            catch (SocketException)
            {
                                Assert.Pass();
                return;
            }

                        Assert.Fail();
        }

        [Test]
        public void ClientConnectDisconnect()
        {
            using (TcpServer server = new TcpServer("TestServer"))
            {
                server.Start(12000);

                TcpClient client = new TcpClient("localhost", 12000);
                Trace.WriteLine("Client Conneect...");
                client.Connect();
                Thread.Sleep(200);
                Assert.That(client.Connected,Is.EqualTo(true));
                Assert.That(server.Clients.Count,Is.EqualTo(1));
                Trace.WriteLine("Client Disconnect...");
                client.Disconnect();
                Thread.Sleep(200);
                Assert.That(server.Clients.Count,Is.EqualTo(0));
                client.Disconnect();
                Thread.Sleep(200);
                Trace.WriteLine("Client Reconnect...");
                client.Connect();
                Thread.Sleep(200);
                Assert.That(server.Clients.Count,Is.EqualTo(1));
                client.Dispose();
            }
        }

        [Test]
        public void ServerDisconnected_ClientConnectStatus()
        {
            using (TcpServer server = new TcpServer("TestServer2"))
            {
                server.Start(12100);
                TcpClient client = new TcpClient("localhost", 12100);
                client.Connect();
                Thread.Sleep(200);
                Assert.That(client.Connected,Is.EqualTo(true));
                server.Stop();
                Thread.Sleep(200);
                Assert.That(server.Clients.Count,Is.EqualTo(0));
                Assert.That(client.Connected,Is.EqualTo(false)); //Property does not reflect the connection status.
            }
        }

        [Test]
        public void ClientConnect_InvalidPort()
        {
                        Assert.Throws<ArgumentOutOfRangeException>(() =>
            {
                TcpClient client = new TcpClient("127.0.0.1", 90000);
                client.Connect();
            });
        }

        private bool ClientConnectFlag = false;
        private TcpClient Client1;

        [Test]
        public void ClientConnectDisconnect_Event()
        {
            TcpServer server = new TcpServer("TestServer-Event");
            server.Start(12200);
            try
            {
                Client1 = new TcpClient("127.0.0.1", 12200);
                Client1.ConnectionStatusChanged += Client_ConnectionStatusChanged;
                Client1.Connect();
                Thread.Sleep(200);
                Assert.That(ClientConnectFlag,Is.EqualTo(true));

                server.Stop();
                Thread.Sleep(200);
                Assert.That(ClientConnectFlag,Is.EqualTo(false));
            }
            finally
            {
                Client1.Disconnect();
                server.Dispose();
            }
        }

        private void Client_ConnectionStatusChanged(object sender, EventArgs e)
        {
            ClientConnectFlag = Client1.Connected;
        }

        [Test]
        public void ClientWriteNotConnectedToServer()
        {
                        Assert.Throws<TcpClientException>(() =>
            {
                using (TcpClient client = new TcpClient("localhost", 11900))
                {
                    client.Write("Testing");
                }
            });
        }
    }
}
