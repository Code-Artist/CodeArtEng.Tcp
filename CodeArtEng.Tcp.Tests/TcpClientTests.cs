using NUnit.Framework;
using System;
using System.IO;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Text;

namespace CodeArtEng.Tcp.Tests
{

    [TestFixture]
    public class TcpClientTests
    {
        private void TcpDelay() { } //Thread.Sleep(1); }  //Machine dependent?
        private readonly TcpServer Server = new TcpServer();
        private TcpClient Client;

        [OneTimeSetUp]
        public void Setup()
        {
            Debug.WriteLine("Test Fixture Setup");
            int port = 16000;
            Server.MessageDelimiter = Convert.ToByte('\t');
            Server.Start(port);
            Server.ClientConnected += Server_ClientConnected;
            Thread.Sleep(500);
            Client = new TcpClient("127.0.0.1", port);
            Client.Connect();
            Client.ReadTimeout = 500;
            for (int x = 0; x < 10; x++)
            {
                Thread.Sleep(500);
                if (Server.Clients.Count != 0) break;
                Trace.WriteLine(x.ToString());
            }
            Trace.WriteLine("Client Connected");
        }


        [OneTimeTearDown]
        public void TearDown()
        {
            Debug.WriteLine("Test Fixture Tear Down");
            Server.Stop();
            Client.Dispose();
            Client = null;
        }

        private void Server_ClientConnected(object sender, TcpServerEventArgs e)
        {
            Debug.WriteLine("EVENT: Server_ClientConnected");
            e.Client.MessageReceived += Client_MessageReceived;
        }

        private void Client_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            Debug.WriteLine("EVENT: Client_MessageReceived");
            string MessageReceiveBuffer = e.ReceivedMessage;
            Debug.WriteLine("RX: " + e.ReceivedMessage);
            e.Client.WriteToClient(MessageReceiveBuffer);
        }

        [Test]
        public void HostName()
        {
            Assert.AreEqual("127.0.0.1", Client.HostName);
        }

        [Test]
        public void ConnectionPort()
        {
            Assert.AreEqual(16000, Client.Port);
        }

        [Test]
        public void CommunicateWithServer()
        {
            string message = "Message from Client";
            Client.Write(message + "\t");
            TcpDelay();
            string returnString = Client.ReadString();
            Assert.AreEqual(message, returnString);
        }

        [Test]
        public void CommunicateWithServerBytes()
        {
            string message = "Message from Client";
            Client.Write(message + "\t");
            TcpDelay();
            Assert.AreEqual(message.Length, Client.ReadBytes().Count());
        }

        [Test]
        public void ReadEmptyBuffer()
        {
            Assert.AreEqual(0, Client.ReadBytes().Length);
        }

        private bool DataReceiveEventRaised;
        private string DataReceived;
        private bool ReadDataByEvent = false;
        private void Client_DataReceived(object sender, TcpDataReceivedEventArgs e)
        {
            if (!ReadDataByEvent) return;
            DataReceived = e.GetString();
            DataReceiveEventRaised = true;
            ReadDataByEvent = false;
        }

        [Test]
        public void ClientMessageReceivedEvent()
        {
            Client.DataReceived += Client_DataReceived;
            try
            {
                DataReceiveEventRaised = false;
                DataReceived = string.Empty;
                ReadDataByEvent = true;

                foreach (TcpServerConnection client in Server.Clients)
                    client.WriteToClient("Testing_ABCD");
                Thread.Sleep(500);
                Assert.AreEqual(true, DataReceiveEventRaised);
                Assert.AreEqual("Testing_ABCD", DataReceived);
            }
            finally { Client.DataReceived -= Client_DataReceived; }
        }

        [Test]
        public void ClientConnectionStatusChangedEvent()
        {
            try
            {
                Client.ConnectionStatusChanged += Client_ConnectionStatusChanged;
                ConnectionStatusChangedEventRaised = false;
                Client.Disconnect();
                Assert.AreEqual(true, ConnectionStatusChangedEventRaised);

                Client.Connect();
                Assert.AreEqual(true, ConnectionStatusChangedEventRaised);
            }
            catch
            {
                Assert.Fail();
            }
            finally
            {
                Client.ConnectionStatusChanged -= Client_ConnectionStatusChanged;
                Client.Connect();
            }

        }

        private bool ConnectionStatusChangedEventRaised = false;
        private void Client_ConnectionStatusChanged(object sender, EventArgs e)
        {
            ConnectionStatusChangedEventRaised = true;
        }

        [Test]
        public void SendBigData()
        {
            Client.DataReceived += Client_DataReceived;
            try
            {
                DataReceiveEventRaised = false;
                DataReceived = string.Empty;
                ReadDataByEvent = true;
                
                string file = Path.GetFullPath(Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "../../../TestData", "Log_153KB.txt"));
                string contents = File.ReadAllText(file);
                Client.Write(contents + "\t");
                Thread.Sleep(200);
                Assert.AreEqual(contents.Length, DataReceived?.Length);
            }
            finally { Client.DataReceived -= Client_DataReceived; }

        }

        [Test]
        public void ReceiveEndByTimoutMode_SendSlowCommand()
        {
            Client.DataReceived += Client_DataReceived;
            string message = "Testing";
            try
            {
                DataReceiveEventRaised = false;
                DataReceived = string.Empty;
                ReadDataByEvent = true;

                Server.InterMessageTimeout = 100;
                Server.MessageReceivedEndMode = TcpServerMessageEndMode.Timeout;
                for(int x=0; x < message.Length; x++)
                {
                    Client.Write(message.Substring(x, 1));
                    Assert.IsFalse(DataReceiveEventRaised);
                    Thread.Sleep(10);
                }
                Thread.Sleep(200);
                Assert.IsTrue(DataReceiveEventRaised);
                Assert.AreEqual(message, DataReceived);

            }
            finally
            {
                Client.DataReceived -= Client_DataReceived;
                Server.MessageReceivedEndMode = TcpServerMessageEndMode.Delimiter;
            }
        }
    }
}
