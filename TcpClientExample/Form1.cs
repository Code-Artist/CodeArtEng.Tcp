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

namespace TcpClientExample
{
    public partial class Form1 : Form
    {
        private TcpClient Client;
        public Form1()
        {
            InitializeComponent();
            Client = new TcpClient();

            Client.HostName = "127.0.0.1";
            Client.Port = 10000;
            propertyGrid1.SelectedObject = Client;
            btWrite.Enabled = false;
            btRead.Enabled = false;
        }

        private void btConnect_Click(object sender, EventArgs e)
        {
            Client.Connect();
            txtInput.Enabled = Client.Connected;
            btWrite.Enabled = txtInput.Enabled;
            btRead.Enabled = txtInput.Enabled;
            propertyGrid1.Refresh();
        }

        private void btWrite_Click(object sender, EventArgs e)
        {
            Client.Write(txtInput.Text);
        }

        private void btRead_Click(object sender, EventArgs e)
        {
            txtInput.Text = Client.ReadString();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Client.Disconnect();
        }
    }
}
