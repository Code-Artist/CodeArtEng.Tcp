using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace CodeArtEng.Tcp.Tests
{
    [TestFixture]
    class TcpAppClientTests
    {
        private TcpAppServer Server;
        private TcpAppClient Client;

        [OneTimeSetUp]
        public void Setup()
        {
            Server = new TcpAppServer();
            Server.Start(12500);
            System.Threading.Thread.Sleep(1000);
            Client = new TcpAppClient("127.0.0.1", 12500);
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            Server.Stop();
            Server.Dispose();
            Client.Dispose();
        }

        [Test]
        public void ClientConnect()
        {
            Client.Connect();
        }
    }
}
