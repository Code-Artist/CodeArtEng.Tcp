using CodeArtEng.Tcp;
using System;
using System.Windows.Forms;

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
            Client.DataReceived += Client_DataReceived;
            Client.ConnectionStatusChanged += Client_ConnectionStatusChanged;
            propertyGrid1.SelectedObject = Client;
            btWrite.Enabled = false;
            btRead.Enabled = false;
        }

        private void Client_ConnectionStatusChanged(object sender, EventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new DelMethod(Client_ConnectionStatusChanged), new object[] { sender, e });
                return;
            }
            propertyGrid1.Refresh();
        }

        private delegate void DelMethod(object sender, BytesReceivedEventArgs e);
        private void Client_DataReceived(object sender, EventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new DelMethod(Client_DataReceived), new object[] { sender, e });
                return;
            }
            txtInput.Text = Client.ReadString();
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
            Client.Dispose();
            Client = null;
        }
    }
}
