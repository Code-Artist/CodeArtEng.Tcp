using CodeArtEng.Tcp;
using System;
using System.Windows.Forms;

namespace TcpClientExample
{
    public partial class Form1 : Form
    {
        private TcpClient Client;
        private TcpAppClient appClient;
        public Form1()
        {
            InitializeComponent();
            Client = new TcpClient();
            appClient = new TcpAppClient();
            appClient.CommandSend += AppClient_CommandSend;
            appClient.ResponseReceived += AppClient_ResponseReceived;

            Client.HostName = "127.0.0.1";
            Client.Port = 1000;
            Client.DataReceived += Client_DataReceived;
            Client.ConnectionStatusChanged += Client_ConnectionStatusChanged;
            propertyGrid1.SelectedObject = Client;
            btWrite.Enabled = false;
        }

        private void AppClient_ResponseReceived(object sender, TcpAppClientEventArgs e)
        {
            tcpAppLog.SelectionColor = System.Drawing.Color.DarkGreen;
            tcpAppLog.AppendText(e.Message);
            tcpAppLog.SelectionStart = tcpAppLog.TextLength;
            tcpAppLog.ScrollToCaret();
        }

        private void AppClient_CommandSend(object sender, TcpAppClientEventArgs e)
        {
            tcpAppLog.SelectionColor = System.Drawing.Color.Blue;
            tcpAppLog.AppendText(e.Message +"\n");
            tcpAppLog.SelectionStart = tcpAppLog.TextLength;
            tcpAppLog.ScrollToCaret();
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            Client.Dispose();
            Client = null;
            appClient.Dispose();
            appClient = null;
        }

        #region [ TCP Client ]

        private void LogRX(RichTextBox target, string message)
        {
            target.SelectionColor = System.Drawing.Color.DarkGreen;
            target.AppendText("[RX] " + message);
            target.SelectionStart = target.TextLength;
            target.ScrollToCaret();
        }

        private void LogTX(RichTextBox target, string message)
        {
            target.SelectionColor = System.Drawing.Color.Blue;
            target.AppendText("[TX] " + message);
            target.SelectionStart = target.TextLength;
            target.ScrollToCaret();
        }


        private void btConnect_Click_1(object sender, EventArgs e)
        {
            Client.Port = Convert.ToInt16(txtPort.Text);
            Client.Connect();
        }

        private void btDisconnect_Click(object sender, EventArgs e)
        {
            Client.Disconnect();
        }

        private void Client_ConnectionStatusChanged(object sender, EventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new DelMethod(Client_ConnectionStatusChanged), new object[] { sender, e });
                return;
            }
            txtInput.Enabled = Client.Connected;
            btWrite.Enabled = txtInput.Enabled;
            propertyGrid1.Refresh();
        }

        private delegate void DelMethod(object sender, TcpDataReceivedEventArgs e);
        private void Client_DataReceived(object sender, TcpDataReceivedEventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new DelMethod(Client_DataReceived), new object[] { sender, e });
                return;
            }
            LogRX(tcpClientLog, e.GetString());
        }

        private void btWrite_Click(object sender, EventArgs e)
        {
            WriteToServer();
        }

        private void WriteToServer()
        {
            LogTX(tcpClientLog, txtInput.Text);
            Client.WriteLine(txtInput.Text);
        }

        #endregion

        private void HandleException(Exception ex)
        {
            tcpAppLog.SelectionColor = System.Drawing.Color.Red;
            tcpAppLog.AppendText("[ERR] " + ex.Message + "\r\n");
            tcpAppLog.SelectionStart = tcpAppLog.TextLength;
            tcpAppLog.ScrollToCaret();
        }

        private void btConnectApp_Click(object sender, EventArgs e)
        {
            try
            {
                appClient.HostName = txtConnectString.Text.Split(':')[0];
                appClient.Port = Convert.ToInt32(txtConnectString.Text.Split(':')[1]);
                appClient.Connect();
            }
            catch(Exception ex) { HandleException(ex); }
        }

        private void btDisconnectApp_Click(object sender, EventArgs e)
        {
            try
            {
                appClient.Disconnect();
            }
            catch (Exception ex) { HandleException(ex); }
        }

        private void btHelp_Click(object sender, EventArgs e)
        {
            ExecuteCommand("heLP");
        }

        private void ExecuteCommand(string cmd)
        {
            try
            {
                TcpAppCommandResult result = appClient.ExecuteCommand(cmd);
                if (result.Status == TcpAppCommandStatus.ERR) throw new Exception("ERROR: Execution FAILED! " + result.ReturnMessage);
            }
            catch (Exception ex) { HandleException(ex); }
        }

        private void txtInput_KeyDown(object sender, KeyEventArgs e)
        {
            if(e.KeyCode == Keys.Return)
            {
                WriteToServer();
            }
        }
    }
}
