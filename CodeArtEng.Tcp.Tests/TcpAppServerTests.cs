using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using System.Diagnostics;

namespace CodeArtEng.Tcp.Tests
{
    [TestFixture]
    public class TcpAppServerTests
    {
        private TcpAppServer Server = new TcpAppServerWindows();
        private TcpAppClient Client = new TcpAppClient("localhost", 25000);

        private TcpAppServer ServerNoQueue = new TcpAppServer(true);
        private TcpAppClient ClientNoQueue = new TcpAppClient("localhost", 25001);

        [OneTimeSetUp]
        public void Setup()
        {
            Server.Start(25000);
            ServerNoQueue.Start(25001);
            System.Threading.Thread.Sleep(100);
            Client.Connect();
            ClientNoQueue.Connect();
            System.Threading.Thread.Sleep(100);
            Assert.That(Server.Clients.Count, Is.EqualTo(1));
            Assert.That(ServerNoQueue.Clients.Count, Is.EqualTo(1));
            Debug.WriteLine("Fixture Setup Completed.");
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            Server.Dispose(); Server = null;
            Client.Dispose(); Client = null;

            ServerNoQueue.Dispose(); ServerNoQueue = null;
            ClientNoQueue.Dispose(); ClientNoQueue = null;
            Debug.WriteLine("Fixture Tear Down Completed.");
        }

        [Test]
        public void RegisterDuplicatedCommand_ArgumentException()
        {
            TcpAppServer server = new TcpAppServer();
            server.RegisterCommand("DummyCommand", "Dummy", null);
            Assert.Throws<ArgumentException>(() => { server.RegisterCommand("dummycommand", "Dummy", null); });
        }

        [Test]
        public void RegisterCommandWithParameterArray()
        {
            TcpAppServer server = new TcpAppServer();
            server.RegisterCommand("DummyCommand", "Dummy", null,
                TcpAppParameter.CreateParameter("P1", "Param 1"),
                TcpAppParameter.CreateParameter("P2", "Param 2"),
                TcpAppParameter.CreateParameterArray("PArr1", "Array Param 1", true)
                );
        }

        [Test]
        public void RegisterCommandWithParameterArray_ArrayNotLastParam_ArgumentException()
        {
            TcpAppServer server = new TcpAppServer();
            Assert.Throws<ArgumentException>(() =>
{
    server.RegisterCommand("DummyCommand", "Dummy", null,
        TcpAppParameter.CreateParameter("P1", "Param 1"),
        TcpAppParameter.CreateParameterArray("PArr1", "Array Param 1", true),
        TcpAppParameter.CreateParameter("P2", "Param 2")
        );
});
        }

        [Test]
        public void StartServer()
        {
            using (TcpAppServer server = new TcpAppServer())
            {
                server.Start(25100);
                Assert.That(server.IsServerStarted, Is.True);
            }
        }

        private TcpAppCommandResult ExecuteCommand(string command)
        {
            return Client.ExecuteCommand(command);
        }

        [Test]
        public void ExecuteCommand_InvalidCommand_Error()
        {
            Assert.Throws<TcpAppClientException>(() => { ExecuteCommand("BigBadWolf"); });
        }

        [Test]
        public void SystemCommand_ProgramName()
        {
            TcpAppCommandResult result = ExecuteCommand("applicationNAME?");
            Assert.That(result.Status, Is.EqualTo(TcpAppCommandStatus.OK));
            Assert.That(result.ReturnMessage.ToLower().Contains("testhost"), Is.True);
        }

        [Test]
        public void SystemCommand_ProgramVersion()
        {
            TcpAppCommandResult result = ExecuteCommand("ApplicationVersion?");
            Assert.That(result.Status, Is.EqualTo(TcpAppCommandStatus.OK));
            Assert.That(result.ReturnMessage.Length > 0, Is.True);  //NUnit Agent Version
        }

        [Test]
        public void SystemCommand_Help()
        {
            TcpAppCommandResult result = ExecuteCommand("Help");
            Console.WriteLine(result.ReturnMessage);
            List<string> lines = new List<string>();
            lines.AddRange(result.ReturnMessage.Split('\n'));

            //Check first line in help contents
            Assert.That(result.Status, Is.EqualTo(TcpAppCommandStatus.OK));
            Assert.That(lines.Count, Is.GreaterThan(10));
        }


        [Test]
        public void AppServerNoQueue_FunctionCheck()
        {
            TcpAppCommandResult result = ClientNoQueue.ExecuteCommand("FunctionList?");
            Assert.That(result.ReturnMessage.Contains("FlushQueue"), Is.False);
            Assert.That(result.ReturnMessage.Contains("CheckStatus"), Is.False);
        }

        [Test]
        public void AppServerNoQueue_RegisterQueueCommand_InvalidOperationException()
        {
            Assert.Throws<InvalidOperationException>(() =>
            {
                ServerNoQueue.RegisterQueuedCommand("Dummy Queue", "Queue", delegate { });
            });
        }
    }
}
