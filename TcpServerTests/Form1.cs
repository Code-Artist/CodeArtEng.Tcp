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
using System.Diagnostics;

namespace TcpServerTests
{
    public partial class Form1 : Form
    {
        TcpServer server = new TcpServer("TServer");
        TcpClient client = new TcpClient("127.0.0.1", 10000);

        public Form1()
        {
            InitializeComponent();
            server.ServerStarted += Server_ServerStarted;
            server.ServerStopped += Server_ServerStopped;
            server.ClientConnected += Server_ClientConnected;
            server.ClientDisconnected += Server_ClientDisconnected;
        }

        private void Server_ServerStopped(object sender, EventArgs e)
        {
            UpdateForm();
        }

        private void Server_ServerStarted(object sender, EventArgs e)
        {
            //UpdateForm();
        }

        private void Server_ClientDisconnected(object sender, TcpServerEventArgs e)
        {
            UpdateForm();
        }

        private void Server_ClientConnected(object sender, TcpServerEventArgs e)
        {
            UpdateForm();
        }

        private void Form1_Shown(object sender, EventArgs e)
        {
        }

        private void UpdateForm()
        {
            if(InvokeRequired)
            {
                BeginInvoke(new MethodInvoker(UpdateForm), null);
                return;
            }
            txtServerStatus.Text = server.IsServerStarted ? "START" : "STOP";
            lstClients.Items.Clear();
            foreach(TcpServerConnection item in server.Clients)
            {
                lstClients.Items.Add(item.ClientIPAddress);
            }
        }

        private void btServerStart_Click(object sender, EventArgs e)
        {
            server.Start(Convert.ToInt16(txtServerPort.Text));
            UpdateForm();
        }

        private void btSeverStop_Click(object sender, EventArgs e)
        {
            server.Stop();
            UpdateForm();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            server.Dispose();
        }

        private void btMultiClients_Click(object sender, EventArgs e)
        {
            for(int x=0; x < 10; x++)
            {
                TcpClient client = new TcpClient("127.0.0.1", Convert.ToInt16(txtServerPort.Text));
                client.Connect();
                Trace.WriteLine(x.ToString());
            }
            UpdateForm();
        }
    }
}
