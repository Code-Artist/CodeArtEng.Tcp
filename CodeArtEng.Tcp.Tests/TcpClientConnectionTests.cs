﻿using NUnit.Framework;
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
            Assert.Throws<SocketException>(() => { client.Connect(); });
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
                Assert.AreEqual(true, client.Connected);
                Assert.AreEqual(1, server.Clients.Count);
                Trace.WriteLine("Client Disconnect...");
                client.Disconnect();
                Thread.Sleep(200);
                Assert.AreEqual(0, server.Clients.Count);
                client.Disconnect();
                Thread.Sleep(200);
                Trace.WriteLine("Client Reconnect...");
                client.Connect();
                Thread.Sleep(200);
                Assert.AreEqual(1, server.Clients.Count);
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
                Assert.AreEqual(true, client.Connected);
                server.Stop();
                Thread.Sleep(200);
                Assert.AreEqual(0, server.Clients.Count);
                Assert.AreEqual(false, client.Connected); //Property does not reflect the connection status.
            }
        }

        [Test]
        public void ClientConnect_InvalidPort()
        {
            TcpClient client = new TcpClient("127.0.0.1", 90000);
            Assert.Throws<ArgumentOutOfRangeException>(() => { client.Connect(); });
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
                Assert.AreEqual(true, ClientConnectFlag);

                server.Stop();
                Thread.Sleep(200);
                Assert.AreEqual(false, ClientConnectFlag);
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
