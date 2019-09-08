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
        private TcpAppServer Server = new TcpAppServer(null);
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
            TcpAppServer server = new TcpAppServer(null);
            server.RegisterCommand("DummyCommand", "Dummy", null);
            Assert.Throws<ArgumentException>(() => { server.RegisterCommand("dummycommand", "Dummy", null); });
        }

        [Test]
        public void StartServer()
        {
            using (TcpAppServer server = new TcpAppServer(null))
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
        public void SystemCommand_TcpAppInit()
        {
            TcpAppCommandResult result = ExecuteCommand("TcpAppInit");
            Assert.AreEqual(TcpAppCommandStatus.OK, result.Status);
            Assert.AreEqual("Microsoft.TestHost.x86 16.0.1", result.ReturnMessage);
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
            TcpAppCommandResult result = ExecuteCommand("Applicationversion?");
            Assert.AreEqual(TcpAppCommandStatus.OK, result.Status);
            Assert.AreEqual("16.0.1", result.ReturnMessage);
        }

        [TestCase("MinimizeWindow")]
        [TestCase("RestoreWindow")]
        [TestCase("BringToFront")]
        //[TestCase("SetWindowPosition")] - Missing required argument.
        public void SystemCommand_GUI_MainForm_NULL(string command)
        {
            //ToDo: Assign dummy main form for test?
            TcpAppCommandResult result = ExecuteCommand(command);
            Assert.AreEqual(TcpAppCommandStatus.ERR, result.Status);
            Assert.AreEqual("Main Form not assigned!", result.ReturnMessage);
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
            Assert.IsTrue(lines[0].StartsWith("TCP Aplication Server Version"));
        }

    }
}
