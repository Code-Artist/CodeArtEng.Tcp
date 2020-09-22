using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Sockets;
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
                Assert.AreEqual(true, ServerStartedEvent);
                Assert.AreEqual(true, server.IsServerStarted);
                Assert.AreEqual(false, ServerStoppedEvent);
                server.Stop();
                Assert.AreEqual(true, ServerStoppedEvent);
                Assert.AreEqual(false, server.IsServerStarted);
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
        public void ClientReconnect()
        {
            using(TcpServer server = new TcpServer())
            {
                int Port = 10010;
                server.Start(Port);
                TcpClient client = new TcpClient("127.0.0.1", Port);
                client.Connect();
                Assert.IsTrue(client.Connected);

                //Simulate Connection Broken.
                server.Stop();
                Assert.IsFalse(client.Connected, "Server stopped, expected client disconnected");
                server.Start(Port);

                client.WriteLine("Test");
                Assert.IsTrue(client.Connected);

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
                Thread.Sleep(1000);

                TcpDelay();
                Assert.AreEqual(ClientCount, server.Clients.Count);
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
                Assert.AreEqual(ClientCount, server.Clients.Count);

                TcpClient extraClient = new TcpClient("127.0.0.1", 10020);
                Assert.Throws<TcpClientException>(() => { extraClient.Connect(); });
            }
        }
    }
}
