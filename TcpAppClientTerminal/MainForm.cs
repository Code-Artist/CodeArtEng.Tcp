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

namespace TcpAppClientTerminal
{
    public partial class MainForm : Form
    {
        private TcpAppClient Client;

        public MainForm()
        {
            InitializeComponent();
            PnSendCommand.Enabled = false;

            Client = new TcpAppClient();
            Client.ConnectionStatusChanged += Client_ConnectionStatusChanged;
            Client.ResponseReceived += Client_ResponseReceived;
            Client.CommandSend += Client_CommandSend;

            TerminalOutput.Clear();

        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            Client.Dispose(); Client = null;
        }

        private void Client_CommandSend(object sender, TcpAppEventArgs e)
        {
            AppendOutput(e.Message + "\n", Color.Yellow);
        }

        private void Client_ResponseReceived(object sender, TcpAppEventArgs e)
        {
            if (e.Message.Contains(" ERR "))
                AppendOutput(e.Message, Color.Red);
            else
                AppendOutput(e.Message, Color.Lime);
        }

        private void AppendOutput(string message, Color textColor)
        {
            TerminalOutput.SelectionStart = TerminalOutput.Text.Length;
            TerminalOutput.SelectionColor = textColor;
            TerminalOutput.AppendText(message);
            TerminalOutput.ScrollToCaret();
        }

        private void Client_ConnectionStatusChanged(object sender, EventArgs e)
        {
            if (Client.Connected)
            {
                TxtHostName.Enabled = TxtPort.Enabled = false;
                PnSendCommand.Enabled = true;
                BtConnect.Text = "Disconnect";
            }
            else
            {
                TxtHostName.Enabled = TxtPort.Enabled = true;
                PnSendCommand.Enabled = false;
                BtConnect.Text = "Connect";
            }
        }

        private void BtConnect_Click(object sender, EventArgs e)
        {
            if (Client.Connected)
            {
                Client.Disconnect();
            }
            else
            {
                Client.HostName = TxtHostName.Text;
                Client.Port = Convert.ToInt32(TxtPort.Text);
                Client.Connect();

                CbFunctions.Items.Clear();
                CbFunctions.Items.Add("(NONE)");
                CbFunctions.Items.AddRange(Client.Commands.ToArray());
                CbFunctions.SelectedIndex = 0;
            }
        }

        private void SendCommand()
        {
            try
            {
                string command = string.Empty;
                if (CbFunctions.SelectedIndex != 0) command = CbFunctions.SelectedItem.ToString();

                if (!string.IsNullOrEmpty(CommandBox.Text))
                {
                    if (!CommandBox.Items.Contains(CommandBox.Text)) CommandBox.Items.Add(CommandBox.Text);
                    command += " " + CommandBox.Text;
                }
                Client.ExecuteCommand(command.Trim());
            }
            catch (Exception ex)
            {
                AppendOutput("ERROR: " + ex.Message + "\r\n",  Color.Red);
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
    }
}
