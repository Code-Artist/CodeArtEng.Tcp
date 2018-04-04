using CodeArtEng.Tcp;
using System;
using System.Security.Cryptography.X509Certificates;
using System.Windows.Forms;

namespace TcpServerExample
{
    public partial class Form1 : Form
    {
        TcpServer Server;

        public Form1()
        {
            InitializeComponent();
            Server = new TcpServer();
            Server.Certificate = new X509Certificate2(Properties.Resources.mysitecert, "mysitecert", X509KeyStorageFlags.Exportable | X509KeyStorageFlags.PersistKeySet);
            Server.ServerStarted += Server_StateChanged;
            Server.ServerStopped += Server_StateChanged;
            Server.ClientConnected += Server_ClientConnected;
            Server.ClientDisconnected += Server_ClientDisconnected;
            propertyGrid1.SelectedObject = Server;
        }

   
        private void Server_ClientConnected(object sender, TcpServerEventArgs e)
        {
            Console.WriteLine("Client connected - " + e.Client.ClientIPAddress.ToString());
            e.Client.BytesReceived += Client_BytesReceived;
        }

        private void Server_ClientDisconnected(object sender, TcpServerEventArgs e)
        {
            Console.WriteLine("Client disconnected - " + e.Client.ClientIPAddress.ToString());
        }

        private static string ByteArrayToString(byte[] ba)
        {
            return BitConverter.ToString(ba, 0).Replace("-", " ");
        }

        private delegate void DelMethod(object sender, BytesReceivedEventArgs e);
        private void Client_BytesReceived(object sender, BytesReceivedEventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new DelMethod(Client_BytesReceived), new object[] { sender, e });
                return;
            }

            Console.WriteLine("Byte count = " + e.ReceivedBytes.Length);
            Console.WriteLine("Bytes recv array = " + ByteArrayToString(e.ReceivedBytes));

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
