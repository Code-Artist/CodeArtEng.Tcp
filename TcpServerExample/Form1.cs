﻿using CodeArtEng.Tcp;
using System;
using System.Text;
using System.Windows.Forms;

namespace TcpServerExample
{
    public partial class Form1 : Form
    {
        TcpServer Server;
        TcpServerConnection tcpClient;
        TcpAppServer AppServer;

        private delegate void TCPMethod(object sender, TcpServerEventArgs e);
        private delegate void TCPDataMethod(object sender, TcpServerDataEventArgs e);

        public Form1()
        {
            InitializeComponent();
            Server = new TcpServer();
            Server.ServerStarted += Server_StateChanged;
            Server.ServerStopped += Server_StateChanged;
            Server.ClientConnected += Server_ClientConnected;
            Server.ClientDisconnected += Server_ClientDisconnected;
            propertyGrid1.SelectedObject = Server;

            AppServer = new TcpAppServer(this);
            AppServer.WelcomeMessage = "Welcome to TCP Application Server. Copyright (C) Code Art Engineering.";
            AppServer.ClientConnected += AppServer_ClientConnected;
            AppServer.ClientDisconnected += AppServer_ClientDisconnected;
            propertyGrid2.SelectedObject = AppServer;
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
            target.SelectionColor = System.Drawing.Color.Black;
            target.AppendText(text + "\n");
            target.SelectionStart = target.TextLength;
            target.ScrollToCaret();
        }

        private void LogRX(RichTextBox target, string message)
        {
            target.SelectionColor = System.Drawing.Color.DarkGreen;
            target.AppendText("[RX] " + message);
            target.SelectionStart = target.TextLength;
            target.ScrollToCaret();
        }

        private void LogRX(RichTextBox target, byte[] data)
        {
            target.SelectionColor = System.Drawing.Color.DarkGreen;
            target.AppendText("[RX] " + Encoding.ASCII.GetString(data));
            target.SelectionStart = target.TextLength;
            target.ScrollToCaret();
        }

        private void LogTX(RichTextBox target, byte[] data)
        {
            target.SelectionColor = System.Drawing.Color.Blue;
            target.AppendText("[TX] " + Encoding.ASCII.GetString(data));
            target.SelectionStart = target.TextLength;
            target.ScrollToCaret();
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

        private void Client_BytesReceived(object sender, TcpServerDataEventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new TCPDataMethod(Client_BytesReceived), new object[] { sender, e });
                return;
            }

            //Display Received Bytes
            LogRX(tcpServerLog, e.Data);
            if (!string.IsNullOrEmpty(txtReply.Text)) e.Client.WriteLineToClient(txtReply.Text);
        }

        private void Server_StateChanged(object sender, EventArgs e)
        {
            if (Server.IsServerStarted) BtStart.Text = "Stop";
            else BtStart.Text = "Start";
        }

        private void BtStart_Click(object sender, EventArgs e)
        {
            if (Server.IsServerStarted) Server.Stop();
            else Server.Start(Convert.ToInt16(txtPort.Text));
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


    }
}
