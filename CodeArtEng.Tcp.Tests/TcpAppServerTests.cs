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
        private TcpClient Client = new TcpClient("localhost", 25000);

        [TestFixtureSetUp]
        public void Setup()
        {
            Server.Start(25000);
            System.Threading.Thread.Sleep(100);
            Client.Connect();
            System.Threading.Thread.Sleep(100);
            Assert.AreEqual(1, Server.Clients.Count);
            Debug.WriteLine("Fixture Setup Completed.");
        }

        [TestFixtureTearDown]
        public void TearDown()
        {
            Server.Dispose(); Server = null;
            Client.Dispose(); Client = null;
            Debug.WriteLine("Fixture Tear Down Completed.");
        }

        [Test]
        public void InitServer()
        {
            TcpAppServer server = new TcpAppServer(null);
        }

        [Test, ExpectedException(typeof(ArgumentException))]
        public void RegisterDuplicatedCommand_ArgumentException()
        {
            TcpAppServer server = new TcpAppServer(null);
            server.RegisterCommand("DummyCommand", "Dummy", null);
            server.RegisterCommand("dummycommand", "Dummy", null);
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

        private string ExecuteCommand(string command)
        {
            Client.Write("#TCP# " + command + "\r\n");
            return Client.ReadString();
        }

        [Test]
        public void SystemCommand_TcpAppInit()
        {
            string result = ExecuteCommand("TcpAppInit");
            Assert.AreEqual("#TCP# TcpAppInit OK NUnit 2.6.2.12296\r\n", result);
        }

        [Test]
        public void SystemCommand_ProgramName()
        {
            string result = ExecuteCommand("programNAME?");
            Assert.AreEqual("#TCP# ProgramName? OK NUnit\r\n", result);
        }

        [Test]
        public void SystemCommand_ProgramVersion()
        {
            string result = ExecuteCommand("ProgramVersion?");
            Assert.AreEqual("#TCP# ProgramVersion? OK " + System.Reflection.Assembly.GetCallingAssembly().GetName().Version.ToString() + "\r\n", result);
        }

        [TestCase("MinimizeWindow")]
        [TestCase("RestoreWindow")]
        [TestCase("BringToFront")]
        //[TestCase("SetWindowPosition")] - Missing required argument.
        public void SystemCommand_GUI_MainForm_NULL(string command)
        {
            //ToDo: Assign dummy main form for test?
            string result = ExecuteCommand(command);
            Assert.AreEqual("#TCP# " + command + " ERR Main Form not assigned!\r\n", result);
        }

        [Test]
        public void SystemCommand_Help()
        {
            string result = ExecuteCommand("Help");
            Console.WriteLine(result);
            List<string> lines = new List<string>();
            lines.AddRange(result.Split('\n'));
            //Check first line in help contents
            Assert.IsTrue(lines[0].StartsWith("#TCP# Help OK TCP Aplication Server Version"));
        }

        [Test]
        public void Execute_InvalidCommand_Error()
        {
            string result = ExecuteCommand("BigBadWolf");
            Assert.AreEqual("#TCP# BigBadWolf ERR Invalid Command!\r\n", result);
        }

    }
}
