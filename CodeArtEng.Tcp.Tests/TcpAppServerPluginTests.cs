using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using CodeArtEng.Tcp;
using System.Diagnostics;

namespace CodeArtEng.Tcp.Tests
{
    internal class DummyPlugin : ITcpAppServerPlugin
    {
        public string PluginName { get; } = "Dummy";
        public string PluginDescription { get; } = null;
        public string Alias { get; set; } = "NoName";

        public bool DisposeRequest()
        {
            throw new NotImplementedException();
        }

        public TcpAppInputCommand GetPluginCommand(string[] commandArguments)
        {
            throw new NotImplementedException();
        }

        public void ShowHelp(TcpAppInputCommand sender)
        {
            throw new NotImplementedException();
        }
    }



    [TestFixture]
    class TcpAppServerPluginTests
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
            Server.RegisterPluginType(typeof(TcpAppServerSamplePlugin));
            Server.RegisterPluginType(typeof(TcpAppSimpleString));
            Server.RegisterPluginType(typeof(TcpAppServerSimpleMath));
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            Server.Dispose(); Server = null;
            Client.Dispose(); Client = null;
            Debug.WriteLine("Fixture Tear Down Completed.");
        }

        [Test]
        public void MathPluginSum()
        {
            TcpAppCommandResult result = Client.ExecuteCommand("CreatePlugin simplemath M1");
            Assert.AreEqual(TcpAppCommandStatus.OK, result.Status);
            result = Client.ExecuteCommand("M1 Sum 10 20 30 45");
            Assert.AreEqual("105", result.ReturnMessage);
        }

        [Test]
        public void MathPlugin_CommandTimeout()
        {
            TcpAppClient newClient = new TcpAppClient("127.0.0.1", 25000);
            newClient.Connect();
            TcpAppCommandResult result = newClient.ExecuteCommand("CreatePlugin simplemath M2");
            Assert.AreEqual(TcpAppCommandStatus.OK, result.Status);
            Assert.Throws<TcpAppClientException>(() => { newClient.ExecuteCommand("M2 TimeoutSim"); });
        }


        [Test]
        public void StringPluginReplace()
        {
            TcpAppCommandResult result = Client.ExecuteCommand("CreatePlugin simplestring S1");
            Assert.AreEqual(TcpAppCommandStatus.OK, result.Status);
            result = Client.ExecuteCommand("Execute S1 Replace \"Hell Of World\"  \"l Of \"  \"lo \"");
            Assert.AreEqual("Hello World", result.ReturnMessage);
        }

        [Test]
        public void RegisterPluginDuplicate_Exception()
        {
            Assert.Throws<ArgumentException>(() =>
            {
                Server.RegisterPluginType(typeof(TcpAppServerSamplePlugin));
            });
        }

        [Test]
        public void CreateInstance()
        {
            TcpAppCommandResult result = Client.ExecuteCommand("CreatePlugin SamplePlugin MyPlugin1");
            Assert.AreEqual(TcpAppCommandStatus.OK, result.Status);
            Assert.NotNull(Server.Plugins.FirstOrDefault(x => x.Alias == "MyPlugin1"));
        }

        [Test]
        public void CreateInstance_CaseInsensitive()
        {
            TcpAppCommandResult result = Client.ExecuteCommand("CreatePlugin sampleplugin MyPlugin2");
            Assert.AreEqual(TcpAppCommandStatus.OK, result.Status);
            Assert.NotNull(Server.Plugins.FirstOrDefault(x => x.Alias == "MyPlugin2"));
        }

        [Test]
        public void DisposeInstance()
        {
            TcpAppCommandResult result = Client.ExecuteCommand("CreatePlugin SamplePlugin MyPlugin3");
            Assert.AreEqual(TcpAppCommandStatus.OK, result.Status);
            result = Client.ExecuteCommand("DisposePlugin MyPlugin3");
            Assert.AreEqual(TcpAppCommandStatus.OK, result.Status);
        }

        [Test]
        public void DisposeInstance_CaseInsensitive()
        {
            TcpAppCommandResult result = Client.ExecuteCommand("CreatePlugin SamplePlugin MyPlugin4");
            Assert.AreEqual(TcpAppCommandStatus.OK, result.Status);
            Client.ExecuteCommand("disposeplugin myplugin4");
            result = Client.ExecuteCommand("Plugins?");
            Assert.IsFalse(result.ReturnMessage.Contains("MyPlugin4"));
        }

        [Test]
        public void ObjectsList()
        {
            TcpAppCommandResult result = Client.ExecuteCommand("CreatePlugin SamplePlugin MyPlugin4");
            Assert.AreEqual(TcpAppCommandStatus.OK, result.Status);
            result = Client.ExecuteCommand("Plugins?");
            Assert.AreEqual(TcpAppCommandStatus.OK, result.Status);
            Assert.IsTrue(result.ReturnMessage.Contains("MyPlugin4"));
        }

        [Test]
        public void CreateInstanceInvalidType_ERROR()
        {
            TcpAppCommandResult result = Client.ExecuteCommand("CreatePlugin Plugin Plugin1");
            Assert.AreEqual(TcpAppCommandStatus.ERR, result.Status);
        }

        [Test]
        public void CreateInstance_MissingArgument()
        {
            TcpAppCommandResult result = Client.ExecuteCommand("CreatePlugin");
            Assert.AreEqual(TcpAppCommandStatus.ERR, result.Status);
        }


        [Test]
        public void ExecutePluginCommand()
        {
            TcpAppCommandResult result = Client.ExecuteCommand("CreatePlugin SamplePlugin X1");
            Assert.AreEqual(TcpAppCommandStatus.OK, result.Status);
            result = Client.ExecuteCommand("Execute X1 PluginCommand1");
            Assert.AreEqual(TcpAppCommandStatus.OK, result.Status);
            Assert.AreEqual("Command 1 Executed!", result.ReturnMessage);
        }

        [Test]
        public void ExecutePluginCommand_Cancel()
        {
            Server.BeforeExecutePluginCommand += Server_BeforeExecutePluginCommand;
            try
            {
                TcpAppCommandResult result = Client.ExecuteCommand("CreatePlugin SamplePlugin Z1");
                Assert.AreEqual(TcpAppCommandStatus.OK, result.Status);
                result = Client.ExecuteCommand("Execute Z1 PluginCommand1");
                Assert.AreEqual(TcpAppCommandStatus.ERR, result.Status);
                Assert.AreEqual("Abort by UnitTest!", result.ReturnMessage);
            }
            finally
            {
                Server.BeforeExecutePluginCommand -= Server_BeforeExecutePluginCommand;
            }
        }

        private void Server_BeforeExecutePluginCommand(object sender, TcpAppServerExEventArgs e)
        {
            e.Cancel = true;
            e.Reason = "Abort by UnitTest!";
        }

        [Test]
        public void ExceutePluginCommand_CaseInsensitive()
        {
            TcpAppCommandResult result = Client.ExecuteCommand("CreatePlugin SamplePlugin PLUGIN_EXE");
            Assert.AreEqual(TcpAppCommandStatus.OK, result.Status);
            result = Client.ExecuteCommand("Execute plugin_exe PluginCommand1");
            Assert.AreEqual(TcpAppCommandStatus.OK, result.Status);
            Assert.AreEqual("Command 1 Executed!", result.ReturnMessage);
        }

        [Test]
        public void ExecutePluginCommand_CommandNotExist()
        {
            TcpAppCommandResult result = Client.ExecuteCommand("CreatePlugin SamplePlugin X3");
            Assert.AreEqual(TcpAppCommandStatus.OK, result.Status);
            result = Client.ExecuteCommand("Execute X2 UnknownCommand");
            Assert.AreEqual(TcpAppCommandStatus.ERR, result.Status);
        }

        [Test]
        public void ExecutePluginCommand_ExtraParameter_Ignored()
        {
            TcpAppCommandResult result = Client.ExecuteCommand("CreatePlugin SamplePlugin X2");
            Assert.AreEqual(TcpAppCommandStatus.OK, result.Status);
            result = Client.ExecuteCommand("Execute X2 PluginCommand1 Value1 Value2");
            Assert.AreEqual(TcpAppCommandStatus.OK, result.Status);
            Assert.AreEqual("Command 1 Executed!", result.ReturnMessage);
        }

        [Test]
        public void AddPlugin()
        {
            int count = Server.Plugins.Count;
            TcpAppServerSamplePlugin sample = new TcpAppServerSamplePlugin() { Alias = "Sample" };
            Server.AddPlugin(sample);
            TcpAppCommandResult result = Client.ExecuteCommand("Plugins?");
            Assert.AreEqual(TcpAppCommandStatus.OK, result.Status);
            Assert.IsTrue(result.ReturnMessage.Contains("Sample"));

            Server.DisposePlugin(sample);
            Assert.AreEqual(count, Server.Plugins.Count);
        }

        [Test]
        public void AddPlugin_NonRegisteredType_Exception()
        {
            DummyPlugin dummy = new DummyPlugin();
            Assert.Throws<ArgumentException>(() => { Server.AddPlugin(dummy); });
        }


    }
}
