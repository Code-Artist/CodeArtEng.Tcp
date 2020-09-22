﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using CodeArtEng.Tcp;
using System.Diagnostics;

namespace CodeArtEng.Tcp.Tests
{
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
            TcpAppCommandResult result = Client.ExecuteCommand("CreateObject simplemath M1");
            Assert.AreEqual(TcpAppCommandStatus.OK, result.Status);
            result = Client.ExecuteCommand("M1 Sum 10 20 30 45");
            Assert.AreEqual("105", result.ReturnMessage);
        }

        [Test]
        public void MathPlugin_CommandTimeout()
        {
            TcpAppClient newClient = new TcpAppClient("127.0.0.1", 25000);
            newClient.Connect();
            TcpAppCommandResult result = newClient.ExecuteCommand("CreateObject simplemath M2");
            Assert.AreEqual(TcpAppCommandStatus.OK, result.Status);
            Assert.Throws<TcpAppClientException>(() => { newClient.ExecuteCommand("M2 TimeoutSim"); });
        }


        [Test]
        public void StringPluginReplace()
        {
            TcpAppCommandResult result = Client.ExecuteCommand("CreateObject simplestring S1");
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
            TcpAppCommandResult result = Client.ExecuteCommand("CreateObject SamplePlugin MyPlugin1");
            Assert.AreEqual(TcpAppCommandStatus.OK, result.Status);
            Assert.NotNull(Server.PluginList.FirstOrDefault(x => x.Alias == "MyPlugin1"));
        }

        [Test]
        public void CreateInstance_CaseInsensitive()
        {
            TcpAppCommandResult result = Client.ExecuteCommand("CreateObject sampleplugin MyPlugin2");
            Assert.AreEqual(TcpAppCommandStatus.OK, result.Status);
            Assert.NotNull(Server.PluginList.FirstOrDefault(x => x.Alias == "MyPlugin2"));
        }

        [Test]
        public void DisposeInstance()
        {
            TcpAppCommandResult result = Client.ExecuteCommand("CreateObject SamplePlugin MyPlugin3");
            Assert.AreEqual(TcpAppCommandStatus.OK, result.Status);
            result = Client.ExecuteCommand("DisposeObject MyPlugin3");
            Assert.AreEqual(TcpAppCommandStatus.OK, result.Status);
        }

        [Test]
        public void DisposeInstance_CaseInsensitive()
        {
            TcpAppCommandResult result = Client.ExecuteCommand("CreateObject SamplePlugin MyPlugin4");
            Assert.AreEqual(TcpAppCommandStatus.OK, result.Status);
            Client.ExecuteCommand("disposeobject myplugin4");
            result = Client.ExecuteCommand("Objects?");
            Assert.IsFalse(result.ReturnMessage.Contains("MyPlugin4"));
        }

        [Test]
        public void ObjectsList()
        {
            TcpAppCommandResult result = Client.ExecuteCommand("CreateObject SamplePlugin MyPlugin4");
            Assert.AreEqual(TcpAppCommandStatus.OK, result.Status);
            result = Client.ExecuteCommand("Objects?");
            Assert.AreEqual(TcpAppCommandStatus.OK, result.Status);
            Assert.IsTrue(result.ReturnMessage.Contains("MyPlugin4"));
        }

        [Test]
        public void CreateInstanceInvalidType_ERROR()
        {
            TcpAppCommandResult result = Client.ExecuteCommand("CreateObject Plugin Plugin1");
            Assert.AreEqual(TcpAppCommandStatus.ERR, result.Status);
        }

        [Test]
        public void CreateInstance_MissingArgument()
        {
            TcpAppCommandResult result = Client.ExecuteCommand("CreateObject");
            Assert.AreEqual(TcpAppCommandStatus.ERR, result.Status);
        }


        [Test]
        public void ExecutePluginCommand()
        {
            TcpAppCommandResult result = Client.ExecuteCommand("CreateObject SamplePlugin X1");
            Assert.AreEqual(TcpAppCommandStatus.OK, result.Status);
            result = Client.ExecuteCommand("Execute X1 PluginCommand1");
            Assert.AreEqual(TcpAppCommandStatus.OK, result.Status);
            Assert.AreEqual("Command 1 Executed!", result.ReturnMessage);
        }

        [Test]
        public void ExceutePluginCommand_CaseInsensitive()
        {
            TcpAppCommandResult result = Client.ExecuteCommand("CreateObject SamplePlugin PLUGIN_EXE");
            Assert.AreEqual(TcpAppCommandStatus.OK, result.Status);
            result = Client.ExecuteCommand("Execute plugin_exe PluginCommand1");
            Assert.AreEqual(TcpAppCommandStatus.OK, result.Status);
            Assert.AreEqual("Command 1 Executed!", result.ReturnMessage);
        }

        [Test]
        public void ExecutePluginCommand_CommandNotExist()
        {
            TcpAppCommandResult result = Client.ExecuteCommand("CreateObject SamplePlugin X3");
            Assert.AreEqual(TcpAppCommandStatus.OK, result.Status);
            result = Client.ExecuteCommand("Execute X2 UnknownCommand");
            Assert.AreEqual(TcpAppCommandStatus.ERR, result.Status);
        }

        [Test]
        public void ExecutePluginCommand_ExtraParameter_Ignored()
        {
            TcpAppCommandResult result = Client.ExecuteCommand("CreateObject SamplePlugin X2");
            Assert.AreEqual(TcpAppCommandStatus.OK, result.Status);
            result = Client.ExecuteCommand("Execute X2 PluginCommand1 Value1 Value2");
            Assert.AreEqual(TcpAppCommandStatus.OK, result.Status);
            Assert.AreEqual("Command 1 Executed!", result.ReturnMessage);
        }

        [Test]
        public void AddPlugin()
        {
            TcpAppServerSamplePlugin sample = new TcpAppServerSamplePlugin() { Alias = "Sample" };
            Server.AddPlugin(sample);
            TcpAppCommandResult result = Client.ExecuteCommand("Objects?");
            Assert.AreEqual(TcpAppCommandStatus.OK, result.Status);
            Assert.IsTrue(result.ReturnMessage.Contains("Sample"));
        }
    }
}