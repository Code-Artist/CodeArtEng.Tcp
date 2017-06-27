using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CodeArtEng.Tcp;

namespace TcpServerExample
{
    public partial class Form1 : Form
    {
        TcpServer Server;

        public Form1()
        {
            InitializeComponent();
            Server = new TcpServer();
            Server.ServerStarted += Server_StateChanged;
            Server.ServerStopped += Server_StateChanged;
            Server.ClientConnected += Server_ClientConnected;
            propertyGrid1.SelectedObject = Server;
        }

        private void Server_ClientConnected(object sender, TcpServerEventArgs e)
        {
            e.Client.BytesReceived += Client_BytesReceived;
        }

        private delegate void DelMethod(object sender, BytesReceivedEventArgs e);
        private void Client_BytesReceived(object sender, BytesReceivedEventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new DelMethod(Client_BytesReceived), new object[] { sender, e });
                return;
            }
            txtInput.Text = System.Text.ASCIIEncoding.ASCII.GetString(e.ReceivedBytes);
            if (txtInput.Text.Length > 0)
            {
                if (e.Client.Connected) e.Client.WriteToClient(txtReply.Text);
            }
        }

        private void Server_StateChanged(object sender, EventArgs e)
        {
            if (Server.IsServerStarted) btStart.Text = "Stop";
            else btStart.Text = "Start";
        }

        private void btStart_Click(object sender, EventArgs e)
        {
            if (Server.IsServerStarted) Server.Stop();
            else Server.Start(Convert.ToInt16(txtPort.Text));
            propertyGrid1.Refresh();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Server.Stop();
            Server.Dispose();
        }
    }
}
