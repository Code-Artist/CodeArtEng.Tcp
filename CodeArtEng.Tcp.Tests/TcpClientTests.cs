using NUnit.Framework;
using System;
using System.IO;
using System.Diagnostics;
using System.Linq;
using System.Threading;

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
            Assert.That(Client.HostName, Is.EqualTo("127.0.0.1"));
        }

        [Test]
        public void ConnectionPort()
        {
            Assert.That(Client.Port, Is.EqualTo(16000));
        }

        [Test]
        public void CommunicateWithServer()
        {
            string message = "Message from Client";
            Client.Write(message + "\t");
            TcpDelay();
            string returnString = Client.ReadString();
            Assert.That(returnString, Is.EqualTo(message));
        }

        [Test]
        public void CommunicateWithServerBytes()
        {
            string message = "Message from Client";
            Client.Write(message + "\t");
            TcpDelay();
            Assert.That(Client.ReadBytes().Count(), Is.EqualTo(message.Length));
        }

        [Test]
        public void ReadEmptyBuffer()
        {
            Assert.That(Client.ReadBytes().Length, Is.EqualTo(0));
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
                Assert.That(DataReceiveEventRaised, Is.EqualTo(true));
                Assert.That(DataReceived, Is.EqualTo("Testing_ABCD"));
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
                Assert.That(ConnectionStatusChangedEventRaised, Is.EqualTo(true));

                Client.Connect();
                Assert.That(ConnectionStatusChangedEventRaised, Is.EqualTo(true));
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
                Assert.That(DataReceived?.Length, Is.EqualTo(contents.Length));
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
                for (int x = 0; x < message.Length; x++)
                {
                    Client.Write(message.Substring(x, 1));
                    Assert.That(DataReceiveEventRaised, Is.False);
                    Thread.Sleep(10);
                }
                Thread.Sleep(200);
                Assert.That(DataReceiveEventRaised, Is.True);
                Assert.That(DataReceived, Is.EqualTo(message));

            }
            finally
            {
                Client.DataReceived -= Client_DataReceived;
                Server.MessageReceivedEndMode = TcpServerMessageEndMode.Delimiter;
            }
        }
    }
}
