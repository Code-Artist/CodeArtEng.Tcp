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
    internal class TcpServerTimeoutMode
    {
        private TcpServer Server = new TcpServer();
        private TcpClient Client;
        private int Port = 12500;

        [OneTimeSetUp]
        public void Setup()
        {
            Server.MessageReceivedEndMode = TcpServerMessageEndMode.Timeout;
            Server.InterMessageTimeout = 1000;
            Server.Start(Port);
            Server.ClientConnected += Server_ClientConnected;
        }

        private void Server_ClientConnected(object sender, TcpServerEventArgs e)
        {
            e.Client.ProcessReceivedMessageCallback = ClientMessageReceived;

        }

        private string RxMessage;
        private void ClientMessageReceived(TcpServerConnection client, string message, byte[] messageBytes)
        {
            RxMessage = message;
        }

        [Test]
        public void ClientSendMessage()
        {
            DateTime tStart = DateTime.Now;
            ClientSendReceive();
            double totalSeconds = (DateTime.Now - tStart).TotalSeconds;
                        Assert.That((totalSeconds >= 1) && (totalSeconds <= 3));
        }

        private void ClientSendReceive()
        {
            DateTime tStart = DateTime.Now;
            RxMessage = string.Empty;
            using (TcpClient client = new TcpClient("127.0.0.1", Port))
            {
                RxMessage = string.Empty;
                client.Connect();
                client.Write("123");
                Thread.Sleep(200);
                client.Write("456");
                do
                {
                    Thread.Sleep(100);
                } while (string.IsNullOrEmpty(RxMessage) &&
                  (DateTime.Now - tStart).TotalSeconds < 2);
            }
        }

        [Test]
        public void MultipleClientsSendMessage()
        {
            DateTime tStart = DateTime.Now;
            for (int x = 0; x < 3; x++)
            {
                ClientSendReceive();
            }
            double durations = (DateTime.Now - tStart).TotalSeconds;
                        Assert.That(durations > 3 && durations < 5);
        }

        [Test]
        public void SendMultipleMessage()
        {
            DateTime tStart = DateTime.Now;
            RxMessage = string.Empty;
            using (TcpClient client = new TcpClient("127.0.0.1", Port))
            {
                client.Connect();

                for (int x = 0; x < 3; x++)
                {
                    DateTime iStart = DateTime.Now;
                    RxMessage = string.Empty;
                    client.Write("123");
                    Thread.Sleep(200);
                    client.Write("456");
                    do
                    {
                        Thread.Sleep(100);
                    } while (string.IsNullOrEmpty(RxMessage) &&
                      (DateTime.Now - iStart).TotalSeconds < 2);
                }
            }
            double durations = (DateTime.Now - tStart).TotalSeconds;
                        Assert.That(durations > 3 && durations < 7);
            Assert.That(RxMessage,Is.EqualTo("123456"));
        }
    }
}
