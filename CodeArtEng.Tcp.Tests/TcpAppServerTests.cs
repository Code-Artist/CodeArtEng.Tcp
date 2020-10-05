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
        private TcpAppServer Server = new TcpAppServer();
        private TcpAppClient Client = new TcpAppClient("localhost", 25000);

        [OneTimeSetUp]
        public void Setup()
        {
            Server.Start(25000);
            System.Threading.Thread.Sleep(100);
            Client.Connect();
            System.Threading.Thread.Sleep(100);
            Assert.AreEqual(1, Server.Clients.Count);
            Debug.WriteLine("Fixture Setup Completed.");
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            Server.Dispose(); Server = null;
            Client.Dispose(); Client = null;
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
                Assert.IsTrue(server.IsServerStarted);
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
            Assert.AreEqual(TcpAppCommandStatus.OK, result.Status);
            Assert.AreEqual("Microsoft.TestHost.x86", result.ReturnMessage);
        }

        [Test]
        public void SystemCommand_ProgramVersion()
        {
            TcpAppCommandResult result = ExecuteCommand("ApplicationVersion?");
            Assert.AreEqual(TcpAppCommandStatus.OK, result.Status);
            Assert.AreEqual("16.6.0", result.ReturnMessage);
        }

        [Test]
        public void SystemCommand_Help()
        {
            TcpAppCommandResult result = ExecuteCommand("Help");
            Console.WriteLine(result.ReturnMessage);
            List<string> lines = new List<string>();
            lines.AddRange(result.ReturnMessage.Split('\n'));

            //Check first line in help contents
            Assert.AreEqual(TcpAppCommandStatus.OK, result.Status);
            Assert.Greater(lines.Count, 10);
        }

    }
}
