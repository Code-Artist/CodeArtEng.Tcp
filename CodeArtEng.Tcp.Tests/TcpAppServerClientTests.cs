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

        private readonly object LockObject = new object();

        [OneTimeSetUp]
        public void Setup()
        {
            Server = new TcpAppServer();
            Server.Start(25000);
            Server.ClientSignedIn += Server_ClientSignedIn;
            Server.ClientSigningOut += Server_ClientSigningOut;
            Server.RegisterCommand("Custom_1", "Custom Command.", Custom1Callback);
            Server.RegisterQueuedCommand("Delay", "Custom Delay", (TcpAppInputCommand sender) =>
                {
                    //Each connection execute command from different thread
                    int delay = Convert.ToInt32(sender.Command.Parameter("duration").Value);
                    //TestContext.Progress.WriteLine(sender.Command.Parameter("client").Value + " Sleep " + delay.ToString());
                    Thread.Sleep(delay);
                    sender.Status = TcpAppCommandStatus.OK;
                    sender.OutputMessage = "Delay Tick";
                },
                TcpAppParameter.CreateParameter("client", "client name"),
                TcpAppParameter.CreateParameter("duration", "Duration in ms"));
            Server.RegisterCommand("DelayNoQueue", "Custom Delay", (TcpAppInputCommand sender) =>
            {
                //Each connection execute command from different thread
                int delay = Convert.ToInt32(sender.Command.Parameter("duration").Value);
                //TestContext.Progress.WriteLine(sender.Command.Parameter("client").Value + " Sleep " + delay.ToString());
                Thread.Sleep(delay);
                sender.Status = TcpAppCommandStatus.OK;
                sender.OutputMessage = "MAIN";
            },
                TcpAppParameter.CreateParameter("client", "client name"),
                TcpAppParameter.CreateParameter("duration", "Duration in ms"));
            Server.RegisterPluginType(typeof(TcpAppServerSimpleMath));


            Client = new TcpAppClient("localhost", 25000);
            Tcp = new TcpClient("localhost", 25000);
        }


        [OneTimeTearDown]
        public void Cleanup()
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
            Assert.That(result.Status, Is.EqualTo(TcpAppCommandStatus.OK));
        }

        [Test]
        public void SignIn()
        {
            SignedIn = SignedOut = false;
            Client.Disconnect(); //This function should not throw exception
            Client.Connect();
            Assert.That(SignedIn, Is.EqualTo(true));
            Thread.Sleep(10);
            Client.Disconnect();
            Assert.That(SignedOut, Is.EqualTo(true));
        }

        private void Server_ClientSigningIn(object sender, TcpAppServerExEventArgs e)
        {
            Assert.That(string.IsNullOrEmpty(e.Value.ToString()), Is.False);
            e.Cancel = true;
        }

        [Test, Order(1)]
        public void SignInAccessDenied()
        {
            Server.ClientSigningIn += Server_ClientSigningIn;
            try
            {
                Assert.Throws<TcpAppClientException>(() =>
{
    Client.Connect();
});

            }
            finally
            {
                Server.ClientSigningIn -= Server_ClientSigningIn;
                Client.Disconnect();
                Thread.Sleep(100);
            }
        }

        [Test]
        public void SignInDualClient()
        {
            Client.Connect();
            TcpAppClient Client2 = new TcpAppClient("localhost", 25000);
            Client2.Connect();
            TestContext.Progress.WriteLine("Client ConnectionID = " + Client.ConnectionID);
            TestContext.Progress.WriteLine("Client2 ConnectionID = " + Client2.ConnectionID);
            Assert.That(Client2.ConnectionID, !Is.EqualTo(Client.ConnectionID));
        }

        [Test]
        public void ExecuteSystemCommand_NotSignedIn()
        {
            Client.Connect();
            CheckResult(Client.ExecuteCommand("Help"));
        }

        [Test]
        public void ExecuteSystemCommand_NotSignedIn_Error()
        {
            Client.Connect();
            CheckResult(Client.ExecuteCommand("FlushQueue"));
        }

        [Test]
        public void DualClientCommandTests()
        {
            Thread TClient2 = new Thread(Client2Execution);
            Thread TClient3 = new Thread(Client3Execution);
            try
            {
                Client.Connect();
                TClient2.Start();
                TClient3.Start();
                for (int x = 0; x < 15; x++)
                {
                    Assert.That(Client.ExecuteCommand("DelayNoQueue C1 100").Status == TcpAppCommandStatus.OK, Is.True);
                }
            }
            finally
            {
                Client.Disconnect();
                C2ThreadAbort = C3ThreadAbort = true;
                Thread.Sleep(500);
            }
        }

        private bool C2ThreadAbort = false;
        public void Client2Execution()
        {
            TcpAppClient client = new TcpAppClient("localhost", 25000);
            client.Connect();
            for (int x = 0; x < 10; x++)
            {
                if (client.ExecuteCommand("Delay C2 50").Status == TcpAppCommandStatus.ERR)
                {
                    TestContext.Progress.WriteLine("C2 Terminated on Error!");
                    Assert.Fail();
                    break;
                }
            }
            while (!C2ThreadAbort) { client.ExecuteCommand("CheckStatus"); Thread.Sleep(5000); }
        }

        private bool C3ThreadAbort = false;
        public void Client3Execution()
        {
            TcpAppClient client = new TcpAppClient("localhost", 25000);
            client.Connect();
            for (int x = 0; x < 10; x++)
            {
                if (client.ExecuteCommand("Delay C3 80").Status == TcpAppCommandStatus.ERR)
                {
                    TestContext.Progress.WriteLine("C3 Terminated on Error!");
                    Assert.Fail();
                    break;
                }
            }
            while (!C3ThreadAbort) { client.ExecuteCommand("CheckStatus"); Thread.Sleep(5000); }
        }

        [Test]
        public void SendCommandServerTimeoutReturnERR()
        {
            int defaultTimeout = Server.ExecutionTimeout;
            Server.ExecutionTimeout = 1000; //Simulate Server Timeout on 1 seconds
            try
            {
                Client.Connect();
                Assert.That(Client.ExecuteCommand("DelayNoQueue C1 5000", 10000).Status == TcpAppCommandStatus.ERR, Is.True);
            }
            finally
            {
                Server.ExecutionTimeout = defaultTimeout; //Restore Settings
            }
        }

        [Test, Order(0)]
        public void SendCommandClientTimeout()
        {
            int defaultTimeout = Server.ExecutionTimeout;
            Server.ExecutionTimeout = 1000; //Simulate Server Timeout on 1 seconds
            try
            {
                Client.Connect();
                Assert.That(Client.ExecuteCommand("DelayNoQueue C1 5000", 10000).Status == TcpAppCommandStatus.ERR, Is.True);
            }
            finally
            {
                Server.ExecutionTimeout = defaultTimeout; //Restore Settings
            }
            Thread.Sleep(500);
        }
    }
}
