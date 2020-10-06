using System;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace CodeArtEng.Tcp.Tests
{
    [TestFixture]
    class TcpAppServerClientTests
    {
        public TcpAppServer Server;
        public TcpAppClient Client;
        public TcpClient Tcp;

        [OneTimeSetUp]
        public void Setup()
        {
            Server = new TcpAppServer();
            Server.Start(25000);
            Server.ClientSignedIn += Server_ClientSignedIn;
            Server.ClientSigningOut += Server_ClientSigningOut;
            Server.RegisterCommand("Custom_1", "Custom Command.", Custom1Callback);
            Server.RegisterPluginType(typeof(TcpAppServerSimpleMath));

            Client = new TcpAppClient("localhost", 25000);
            Tcp = new TcpClient("localhost", 25000);
        }

        [OneTimeTearDown]
        public void Finalize()
        {
            Server.Dispose();
            Client.Dispose();
            Tcp.Dispose();
        }

        private void Custom1Callback(TcpAppInputCommand sender)
        {
            sender.OutputMessage = "Result_1";
            sender.Status = TcpAppCommandStatus.OK;
        }

        private bool SignedOut = false;
        private void Server_ClientSigningOut(object sender, TcpAppServerEventArgs e)
        {
            SignedOut = true;
        }

        private bool SignedIn = false;
        private void Server_ClientSignedIn(object sender, TcpAppServerEventArgs e)
        {
            SignedIn = true;
        }

        private void CheckResult(TcpAppCommandResult result)
        {
            Assert.AreEqual(TcpAppCommandStatus.OK, result.Status);
        }

        [Test]
        public void SignIn()
        {
            SignedIn = SignedOut = false;
            Client.Connect();
            Assert.AreEqual(true, SignedIn);
            Thread.Sleep(10);
            Client.Disconnect();
            Assert.AreEqual(true, SignedOut);
        }

        [Test]
        public void SignInDualClient()
        {
            Client.Connect();
            TcpAppClient Client2 = new TcpAppClient("localhost", 25000);
            Client2.Connect();
            TestContext.Progress.WriteLine("Client ConnectionID = " + Client.ConnectionID);
            TestContext.Progress.WriteLine("Client2 ConnectionID = " + Client2.ConnectionID);
            Assert.AreNotEqual(Client.ConnectionID, Client2.ConnectionID);
        }

        [Test]
        public void ExecuteSystemCommand_NotSignedIn()
        {
            Client.Connect();
            CheckResult(Client.ExecuteCommand("Help"));
        }

    }
}
