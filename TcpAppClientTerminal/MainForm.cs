using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CodeArtEng.Tcp;

namespace TcpAppClientTerminal
{
    public partial class MainForm : Form
    {
        private TcpAppClient AppClient;
        private TcpClient TcpClient;
        private TcpClient ptrClient;

        public MainForm()
        {
            InitializeComponent();
            PnSendCommand.Enabled = false;

            AppClient = new TcpAppClient();
            AppClient.ConnectionStatusChanged += Client_ConnectionStatusChanged;
            AppClient.ResponseReceived += Client_ResponseReceived;
            AppClient.CommandSend += Client_CommandSend;

            TcpClient = new TcpClient();
            TcpClient.ConnectionStatusChanged += Client_ConnectionStatusChanged;
            TcpClient.DataReceived += TcpClient_DataReceived;

            CbClientType.SelectedIndex = 0;
            TerminalOutput.Clear();
        }
        private void CbClientType_SelectedIndexChanged(object sender, EventArgs e)
        {
            switch (CbClientType.SelectedIndex)
            {
                case 0:
                    ptrClient = AppClient;
                    TxtPort.Text = "12000";
                    PnTcpAppClient.Enabled = true;
                    break;

                case 1:
                    ptrClient = TcpClient;
                    TxtPort.Text = "1000";
                    PnTcpAppClient.Enabled = false;
                    break;
                default: throw new NotSupportedException("Not supported type!");
            }
            TxtTimeout.Enabled = (ptrClient == AppClient);
            ClientProperty.SelectedObject = ptrClient;
        }

        private void TcpClient_DataReceived(object sender, TcpDataReceivedEventArgs e)
        {
            AppendOutput(e.GetString(), Color.Lime);
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            ptrClient = null;
            AppClient.Dispose(); AppClient = null;
            TcpClient.Dispose(); TcpClient = null;
        }

        private void Client_CommandSend(object sender, TcpAppClientEventArgs e)
        {
            AppendOutput(e.Message + "\n", Color.Yellow);
            Application.DoEvents();
        }

        private void Client_ResponseReceived(object sender, TcpAppClientEventArgs e)
        {
            if (e.Message.StartsWith("ERR"))
                AppendOutput(e.Message, Color.Red);
            else
                AppendOutput(e.Message, Color.Lime);
        }


        private delegate void AppendOutputDelegate(string message, Color textColor);
        private void AppendOutput(string message, Color textColor)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new AppendOutputDelegate(AppendOutput), message, textColor);
                return;
            }
            TerminalOutput.SelectionStart = TerminalOutput.Text.Length;
            TerminalOutput.SelectionColor = textColor;
            TerminalOutput.AppendText(message);
            TerminalOutput.ScrollToCaret();
        }

        private void Client_ConnectionStatusChanged(object sender, EventArgs e)
        {
            if (ptrClient == null) return;
            if (ptrClient.Connected)
            {
                this.BeginInvoke(new MethodInvoker(ClientConnected));
            }
            else
            {
                this.BeginInvoke(new MethodInvoker(ClientDisconnected));
            }
        }

        private void ClientConnected()
        {
            TxtHostName.Enabled = TxtPort.Enabled = false;
            PnSendCommand.Enabled = true;
            BtConnect.Text = "Disconnect";
            CbClientType.Enabled = false;
        }

        private void ClientDisconnected()
        {
            TxtHostName.Enabled = TxtPort.Enabled = true;
            PnSendCommand.Enabled = false;
            BtConnect.Text = "Connect";
            CbClientType.Enabled = true;
        }

        private void BtConnect_Click(object sender, EventArgs e)
        {
            if (ptrClient.Connected)
            {
                ptrClient.Disconnect();
            }
            else
            {
                ptrClient.HostName = TxtHostName.Text;
                ptrClient.Port = Convert.ToInt32(TxtPort.Text);
                ptrClient.Connect();

                if (ptrClient is TcpAppClient)
                {
                    CommandBox.AutoCompleteSource = AutoCompleteSource.CustomSource;
                    CommandBox.AutoCompleteCustomSource.AddRange(AppClient.Commands.ToArray());
                    CommandBox.AutoCompleteMode = AutoCompleteMode.SuggestAppend;
                }
            }
        }

        private void SendCommand()
        {
            try
            {
                string command = string.Empty;
                if (!string.IsNullOrEmpty(CommandBox.Text))
                {
                    if (!CommandBox.Items.Contains(CommandBox.Text)) CommandBox.Items.Add(CommandBox.Text);
                    command = CommandBox.Text;
                    if (ChkCRLF.Checked) command += "\r\n";
                }
                if (ptrClient == AppClient)
                    AppClient.ExecuteCommand(command, Convert.ToInt32(TxtTimeout.Text));
                else
                {
                    TcpClient.Write(command);
                    AppendOutput(command, Color.FromArgb(224, 224, 224));
                }
            }
            catch (Exception ex)
            {
                AppendOutput("ERROR: " + ex.Message + "\r\n", Color.Red);
            }

        }

        private void BtSend_Click(object sender, EventArgs e)
        {
            SendCommand();
        }

        private void CommandBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Return) SendCommand();
        }

        private int id = 0;
        private void BtAuto1_Click(object sender, EventArgs e)
        {
            Random r = new Random();
            id = r.Next() % 1000;
            int timeout = Convert.ToInt32(TxtTimeout.Text);
            if (!AppClient.Connected) BtConnect.PerformClick();
            if (AppClient.Connected)
            {
                AppClient.ExecuteCommand("createplugin sampleplugin x" + id.ToString(), timeout);
                AppClient.ExecuteCommand("x" + id.ToString() + " plugincommand1", timeout);

                AppClient.ExecuteCommand("createplugin simplemath m" + id.ToString(), timeout);
            }

        }

        private void BtAutoSum_Click(object sender, EventArgs e)
        {
            int timeout = Convert.ToInt32(TxtTimeout.Text);
            if (ptrClient != AppClient) return;

            Random r = new Random((int)DateTime.Now.Ticks);
            int a = r.Next() % 1000;
            int b = r.Next() % 1000;
            int c = a + b;

            TcpAppCommandResult result;
            for (int x = 0; x < 100; x++)
            {
                System.Threading.Thread.Sleep(1000);
                result = AppClient.ExecuteCommand(string.Format("m{0} SumQ {1} {2}", id, a, b), timeout);
                if (result.Status == TcpAppCommandStatus.QUEUED)
                {
                    do
                    {
                        System.Threading.Thread.Sleep(200);
                        result = AppClient.ExecuteCommand("checkstatus", timeout);
                    } while (result.Status >= TcpAppCommandStatus.QUEUED);
                }

                if (result.ReturnMessage.Split('\n').First() != c.ToString()) MessageBox.Show("ERROR!");
            }
        }

        private void BtTestTimeout_Click(object sender, EventArgs e)
        {
            try
            {
                AppClient.ExecuteCommand("createplugin simplemath m1");
                AppClient.ExecuteCommand("m1 timeoutsim");
            }
            catch (Exception ex)
            {
                AppendOutput("ERROR: " + ex.Message + "\r\n", Color.Red);
            }
        }
    }
}
