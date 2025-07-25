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

        private void Server_ClientDisconnected(object sender, TcpServerEventArgs e)
        {
            throw new NotImplementedException();
        }

        [Test]
        public void StartStopServerPort1000()
        {
            using (TcpServer server = new TcpServer())
            {
                SubscribeEvent(server);
                server.Start(200);
                Assert.That(ServerStartedEvent, Is.EqualTo(true));
                Assert.That(server.IsServerStarted, Is.EqualTo(true));
                Assert.That(ServerStoppedEvent, Is.EqualTo(false));
                server.Stop();
                Assert.That(ServerStoppedEvent, Is.EqualTo(true));
                Assert.That(server.IsServerStarted, Is.EqualTo(false));
            }
        }

        [Test]
        public void StartServerInvalidPort_ArgumentOutofRangeException()
        {
            using (TcpServer server = new TcpServer())
            {
                ServerStartedEvent = false;
                Assert.Throws<ArgumentOutOfRangeException>(() => { server.Start(99999); });
                //Assert.AreEqual(false, ServerStartedEvent);
            }
        }

        private bool ClientConnectingEvent = false;
        private bool ClientConnectedEvent = false;
        private bool ClientDisconnectedEvent = false;

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
                Assert.That(client.Connected, Is.EqualTo(true));
                Trace.WriteLine("Client Connected to Server.");
                TcpDelay();
                Assert.That(ClientConnectingEvent, Is.EqualTo(true));
                Assert.That(ClientConnectedEvent, Is.EqualTo(true));
                Assert.That(server.Clients.Count, Is.EqualTo(1));
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
                Assert.That(server.Clients.Count, Is.EqualTo(1));
                server.Stop();
                Assert.That(server.Clients.Count, Is.EqualTo(0));
            }
        }

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
                Assert.That(server.Clients.Count, Is.EqualTo(1));
                client.Disconnect();
                TcpDelay();
                Assert.That(server.Clients.Count, Is.EqualTo(0));
                Assert.That(ClientDisconnectedEvent, Is.EqualTo(true));
            }
        }

        [Test]
        public void ClientReconnect()
        {
            using (TcpServer server = new TcpServer())
            {
                int Port = 10010;
                server.Start(Port);
                TcpClient client = new TcpClient("127.0.0.1", Port);
                client.Connect();
                Assert.That(client.Connected, Is.True);

                //Simulate Connection Broken.
                server.Stop();
                Assert.That(client.Connected, Is.False, "Server stopped, expected client disconnected");
                server.Start(Port);

                client.WriteLine("Test");
                Assert.That(client.Connected, Is.True);

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
                server.ClientDisconnected += (sender, e) => { server.Clients.Remove(e.Client); };
                TcpDelay();
                for (int x = 0; x < ClientCount; x++)
                {
                    TcpClient client = new TcpClient("127.0.0.1", 10020);
                    Clients.Add(client);
                    client.Connect();
                }

                TcpDelay();
                TcpDelay();
                Assert.That(server.Clients.Count, Is.EqualTo(ClientCount));

                server.Clients[5].Close();
                TcpDelay();
                Trace.WriteLine("Connection Closed.");
                Assert.That(server.Clients.Count, Is.EqualTo(ClientCount - 1));

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
                Thread.Sleep(1000);

                TcpDelay();
                Assert.That(server.Clients.Count, Is.EqualTo(ClientCount));
                Trace.WriteLine("Test Ended.");
                Clients.Clear();
            }
        }

        [Test]
        public void MultipleClientsConnectToServer_RejectAfterMaxAchieved()
        {
            int ClientCount = 5;
            using (TcpServer server = new TcpServer())
            {
                //Prevent TcpClient dispose by GC
                List<TcpClient> Clients = new List<TcpClient>();
                server.MaxClients = ClientCount;
                server.Start(10020);
                TcpDelay();
                for (int x = 0; x < ClientCount; x++)
                {
                    TcpClient client = new TcpClient("127.0.0.1", 10020);
                    Clients.Add(client);
                    client.Connect();
                }
                Thread.Sleep(1000);
                TcpDelay();
                Assert.That(server.Clients.Count, Is.EqualTo(ClientCount));

                TcpClient extraClient = new TcpClient("127.0.0.1", 10020);
                Assert.Throws<TcpClientException>(() => { extraClient.Connect(); });
            }
        }
    }
}
