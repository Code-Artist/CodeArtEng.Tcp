using CodeArtEng.Tcp;
using System;
using System.Text;
using System.Linq;
using System.Windows.Forms;

namespace TcpServerExample
{
    public partial class Form1 : Form
    {
        readonly TcpServer Server;
        TcpServerConnection tcpClient;
        readonly TcpAppServer AppServer;

        private delegate void TCPMethod(object sender, TcpServerEventArgs e);
        private delegate void TCPDataMethod(object sender, TcpServerDataEventArgs e);
        private delegate void TCPMessageReceivedMethod(object sender, MessageReceivedEventArgs e);

        public Form1()
        {
            InitializeComponent();
            Server = new TcpServer();
            Server.MaxClients = 5;
            Server.ServerStarted += Server_StateChanged;
            Server.ServerStopped += Server_StateChanged;
            Server.ClientConnected += Server_ClientConnected;
            Server.ClientDisconnected += Server_ClientDisconnected;
            propertyGrid1.SelectedObject = Server;

            AppServer = new TcpAppServer()
            {
                WelcomeMessage = "Welcome to TCP Application Server. Copyright (C) Code Art Engineering."
            };
            AppServer.MaxClients = 5;
            AppServer.ExecutionTimeout = 1000;
            AppServer.ClientConnected += AppServer_ClientConnected;
            AppServer.ClientDisconnected += AppServer_ClientDisconnected;
            AppServer.ClientSignedIn += AppServer_ClientInitialized;
            AppServer.ClientSigningOut += AppServer_ClientSigningOut;
            tcpClientsList1.AssignObject(AppServer);
            tcpAppServerQueue1.AssignObject(AppServer);

            //TCP Application Server Customization Test
            AppServer.RegisterCommand("CustomFunction", "Dummy Custom Function", customFunctionCallback);
            AppServer.RegisterCommand("CustomFunction2", "Dummy Custom Function with Parameter", customFunction2Callback,
                TcpAppParameter.CreateParameter("P1", "Parameter 1"),
                TcpAppParameter.CreateOptionalParameter("P2", "Parameter 2, optional.", "10"));
            AppServer.RegisterCommand("SlowCommand", "Command which take 10 seconds to complete. Simulate blocking!", SlowCommand);
            AppServer.RegisterQueuedCommand("SlowCommandQ", "Command which take 10 seconds to complete. Run in queue. Simulate blocking!", SlowCommand);

            CodeArtEng.Tcp.Tests.TcpAppServerSamplePlugin SamplePlugin = new CodeArtEng.Tcp.Tests.TcpAppServerSamplePlugin();
            AppServer.RegisterPluginType(typeof(CodeArtEng.Tcp.Tests.TcpAppServerSamplePlugin));
            AppServer.RegisterPluginType(typeof(CodeArtEng.Tcp.Tests.TcpAppServerSimpleMath));

            propertyGrid2.SelectedObject = AppServer;
        }

        private void SlowCommand(TcpAppInputCommand sender)
        {
            System.Threading.Thread.Sleep(4500);
            sender.OutputMessage = "Snail command completed";
            sender.Status = TcpAppCommandStatus.OK;
        }

        private void customFunction2Callback(TcpAppInputCommand sender)
        {
            sender.Status = TcpAppCommandStatus.OK;
            sender.OutputMessage = "P1 Value = " + sender.Command.Parameter("P1").Value + " P2 Value = " + sender.Command.Parameter("P2").Value;
        }

        private void customFunctionCallback(TcpAppInputCommand sender)
        {
            sender.Status = TcpAppCommandStatus.OK;
            sender.OutputMessage = "Custom Function Executed!";
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Server.Stop();
            Server.Dispose();
            AppServer.Stop();
            AppServer.Dispose();
        }

        private void LogInfo(RichTextBox target, string text)
        {
            //target.SelectionColor = System.Drawing.Color.Black;
            //target.AppendText(text + "\n");
            //target.SelectionStart = target.TextLength;
            //target.ScrollToCaret();
        }

        private void LogRX(RichTextBox target, string message)
        {
            //target.SelectionColor = System.Drawing.Color.DarkGreen;
            //target.AppendText(message + "\n");
            //target.SelectionStart = target.TextLength;
            //target.ScrollToCaret();
        }

        private void LogRX(RichTextBox target, byte[] data)
        {
            //target.SelectionColor = System.Drawing.Color.DarkGreen;
            //target.AppendText(Encoding.ASCII.GetString(data));
            //target.SelectionStart = target.TextLength;
            //target.ScrollToCaret();
        }

        private void LogTX(RichTextBox target, byte[] data)
        {
            //target.SelectionColor = System.Drawing.Color.Blue;
            //target.AppendText(Encoding.ASCII.GetString(data));
            //target.SelectionStart = target.TextLength;
            //target.ScrollToCaret();
        }

        #region [ TCP Server ]

        private void Server_ClientConnected(object sender, TcpServerEventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new TCPMethod(Server_ClientConnected), new object[] { sender, e });
                return;
            }
            tcpClient = e.Client;
            e.Client.MessageReceived += Client_MessageReceived1;
            e.Client.BytesReceived += Client_BytesReceived;
            e.Client.BytesSent += Client_BytesSent;
            LogInfo(tcpServerLog, "Client Connected: " + tcpClient.ClientIPAddress);
        }

        private void Server_ClientDisconnected(object sender, TcpServerEventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new TCPMethod(Server_ClientDisconnected), new object[] { sender, e });
                return;
            }
            LogInfo(tcpServerLog, "Client Disconnected: " + tcpClient.ClientIPAddress);
        }

        private void Client_BytesSent(object sender, TcpServerDataEventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new TCPDataMethod(Client_BytesSent), new object[] { sender, e });
                return;
            }

            //Display Bytes Sent
            LogTX(tcpServerLog, e.Data);
        }

        private void Client_MessageReceived1(object sender, MessageReceivedEventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new TCPMessageReceivedMethod(Client_MessageReceived1), new object[] { sender, e });
                return;

            }

            //Display Received Bytes
            if (!string.IsNullOrEmpty(txtReply.Text)) e.Client.WriteLineToClient(txtReply.Text);
        }

        private void Client_BytesReceived(object sender, TcpServerDataEventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new TCPDataMethod(Client_BytesReceived), new object[] { sender, e });
                return;
            }

            //Display Received Bytes
            LogRX(tcpServerLog, e.Data);
        }

        private void Server_StateChanged(object sender, EventArgs e)
        {
            //if (Server.IsServerStarted) BtStart.Text = "Stop";
            //else BtStart.Text = "Start";
        }

        private void BtStart_Click(object sender, EventArgs e)
        {
            Server.Start(Convert.ToInt16(txtPort.Text));
            propertyGrid1.Refresh();
        }

        private void BtStop_Click(object sender, EventArgs e)
        {
            Server.Stop();
            propertyGrid1.Refresh();
        }

        private void txtServerSend_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return)
            {
                foreach (TcpServerConnection client in Server.Clients)
                {
                    client.WriteLineToClient(txtServerSend.Text);
                }
            }
        }

        #endregion

        private void btAppServerStart_Click(object sender, EventArgs e)
        {
            AppServer.Start(Convert.ToInt32(txtAppServerPort.Text));
            propertyGrid2.Refresh();
        }

        private void btAppServerStop_Click(object sender, EventArgs e)
        {
            AppServer.Stop();
            propertyGrid2.Refresh();
        }

        private void Client_MessageSent(object sender, TcpServerDataEventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new TCPDataMethod(Client_MessageSent), new object[] { sender, e });
                return;
            }

            //Display Bytes Sent
            LogTX(tcpAppServerLog, e.Data);
        }

        private delegate void MessageReceivedDelegate(object sender, MessageReceivedEventArgs e);
        private void Client_MessageReceived(object sender, MessageReceivedEventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new MessageReceivedDelegate(Client_MessageReceived), new object[] { sender, e });
                return;
            }
            LogRX(tcpAppServerLog, e.ReceivedMessage);
        }

        private void AppServer_ClientInitialized(object sender, TcpAppServerEventArgs e)
        {
        }

        private void AppServer_ClientSigningOut(object sender, TcpAppServerEventArgs e)
        {
        }

        private void AppServer_ClientConnected(object sender, TcpServerEventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new TCPMethod(AppServer_ClientConnected), new object[] { sender, e });
                return;
            }
            e.Client.MessageReceived += Client_MessageReceived;
            e.Client.BytesSent += Client_MessageSent;
            LogInfo(tcpAppServerLog, "Client Connected: " + e.Client.ClientIPAddress);
        }

        private void AppServer_ClientDisconnected(object sender, TcpServerEventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new TCPMethod(AppServer_ClientDisconnected), new object[] { sender, e });
                return;
            }
            LogInfo(tcpAppServerLog, "Client Disconnected: " + e.Client.ClientIPAddress);
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
            Server.Start(Convert.ToInt32(txtPort.Text));
            AppServer.Start(Convert.ToInt32(txtAppServerPort.Text));
            propertyGrid2.Refresh();
        }
    }
}
