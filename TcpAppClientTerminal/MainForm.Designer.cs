namespace TcpAppClientTerminal
{
    partial class MainForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.PnSendCommand = new System.Windows.Forms.Panel();
            this.ChkCRLF = new System.Windows.Forms.CheckBox();
            this.CommandBox = new System.Windows.Forms.ComboBox();
            this.BtSend = new System.Windows.Forms.Button();
            this.TerminalOutput = new System.Windows.Forms.RichTextBox();
            this.BtConnect = new System.Windows.Forms.Button();
            this.TxtPort = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.TxtHostName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label3 = new System.Windows.Forms.Label();
            this.TxtTimeout = new System.Windows.Forms.TextBox();
            this.CbClientType = new System.Windows.Forms.ComboBox();
            this.ClientProperty = new System.Windows.Forms.PropertyGrid();
            this.BtSetup = new System.Windows.Forms.Button();
            this.BtAutoSum = new System.Windows.Forms.Button();
            this.PnSendCommand.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // PnSendCommand
            // 
            this.PnSendCommand.Controls.Add(this.ChkCRLF);
            this.PnSendCommand.Controls.Add(this.CommandBox);
            this.PnSendCommand.Controls.Add(this.BtSend);
            this.PnSendCommand.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.PnSendCommand.Location = new System.Drawing.Point(254, 463);
            this.PnSendCommand.Name = "PnSendCommand";
            this.PnSendCommand.Size = new System.Drawing.Size(675, 30);
            this.PnSendCommand.TabIndex = 0;
            // 
            // ChkCRLF
            // 
            this.ChkCRLF.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ChkCRLF.AutoSize = true;
            this.ChkCRLF.Location = new System.Drawing.Point(536, 6);
            this.ChkCRLF.Name = "ChkCRLF";
            this.ChkCRLF.Size = new System.Drawing.Size(53, 17);
            this.ChkCRLF.TabIndex = 3;
            this.ChkCRLF.Text = "CRLF";
            this.ChkCRLF.UseVisualStyleBackColor = true;
            // 
            // CommandBox
            // 
            this.CommandBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.CommandBox.FormattingEnabled = true;
            this.CommandBox.Location = new System.Drawing.Point(5, 4);
            this.CommandBox.Name = "CommandBox";
            this.CommandBox.Size = new System.Drawing.Size(525, 21);
            this.CommandBox.TabIndex = 1;
            this.CommandBox.KeyDown += new System.Windows.Forms.KeyEventHandler(this.CommandBox_KeyDown);
            // 
            // BtSend
            // 
            this.BtSend.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.BtSend.Location = new System.Drawing.Point(595, 3);
            this.BtSend.Name = "BtSend";
            this.BtSend.Size = new System.Drawing.Size(75, 22);
            this.BtSend.TabIndex = 2;
            this.BtSend.Text = "Send";
            this.BtSend.UseVisualStyleBackColor = true;
            this.BtSend.Click += new System.EventHandler(this.BtSend_Click);
            // 
            // TerminalOutput
            // 
            this.TerminalOutput.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(60)))));
            this.TerminalOutput.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TerminalOutput.Font = new System.Drawing.Font("Courier New", 10F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.TerminalOutput.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.TerminalOutput.Location = new System.Drawing.Point(254, 32);
            this.TerminalOutput.Name = "TerminalOutput";
            this.TerminalOutput.Size = new System.Drawing.Size(675, 431);
            this.TerminalOutput.TabIndex = 1;
            this.TerminalOutput.Text = "Response Text";
            // 
            // BtConnect
            // 
            this.BtConnect.Location = new System.Drawing.Point(458, 4);
            this.BtConnect.Name = "BtConnect";
            this.BtConnect.Size = new System.Drawing.Size(80, 23);
            this.BtConnect.TabIndex = 4;
            this.BtConnect.Text = "Connect";
            this.BtConnect.UseVisualStyleBackColor = true;
            this.BtConnect.Click += new System.EventHandler(this.BtConnect_Click);
            // 
            // TxtPort
            // 
            this.TxtPort.Location = new System.Drawing.Point(392, 5);
            this.TxtPort.Name = "TxtPort";
            this.TxtPort.Size = new System.Drawing.Size(60, 20);
            this.TxtPort.TabIndex = 3;
            this.TxtPort.Text = "12000";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(360, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(26, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Port";
            // 
            // TxtHostName
            // 
            this.TxtHostName.Location = new System.Drawing.Point(167, 6);
            this.TxtHostName.Name = "TxtHostName";
            this.TxtHostName.Size = new System.Drawing.Size(187, 20);
            this.TxtHostName.TabIndex = 1;
            this.TxtHostName.Text = "127.0.0.1";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(132, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(29, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Host";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.BtAutoSum);
            this.panel1.Controls.Add(this.BtSetup);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.TxtTimeout);
            this.panel1.Controls.Add(this.CbClientType);
            this.panel1.Controls.Add(this.BtConnect);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.TxtPort);
            this.panel1.Controls.Add(this.TxtHostName);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(929, 32);
            this.panel1.TabIndex = 4;
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(776, 8);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(67, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "Timeout (ms)";
            // 
            // TxtTimeout
            // 
            this.TxtTimeout.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.TxtTimeout.Location = new System.Drawing.Point(849, 5);
            this.TxtTimeout.Name = "TxtTimeout";
            this.TxtTimeout.Size = new System.Drawing.Size(68, 20);
            this.TxtTimeout.TabIndex = 6;
            this.TxtTimeout.Text = "2000";
            // 
            // CbClientType
            // 
            this.CbClientType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.CbClientType.FormattingEnabled = true;
            this.CbClientType.Items.AddRange(new object[] {
            "TcpAppClient",
            "TcpClient"});
            this.CbClientType.Location = new System.Drawing.Point(5, 5);
            this.CbClientType.Name = "CbClientType";
            this.CbClientType.Size = new System.Drawing.Size(121, 21);
            this.CbClientType.TabIndex = 5;
            this.CbClientType.SelectedIndexChanged += new System.EventHandler(this.CbClientType_SelectedIndexChanged);
            // 
            // ClientProperty
            // 
            this.ClientProperty.Dock = System.Windows.Forms.DockStyle.Left;
            this.ClientProperty.Location = new System.Drawing.Point(0, 32);
            this.ClientProperty.Name = "ClientProperty";
            this.ClientProperty.Size = new System.Drawing.Size(254, 461);
            this.ClientProperty.TabIndex = 5;
            // 
            // BtSetup
            // 
            this.BtSetup.Location = new System.Drawing.Point(544, 4);
            this.BtSetup.Name = "BtSetup";
            this.BtSetup.Size = new System.Drawing.Size(75, 23);
            this.BtSetup.TabIndex = 8;
            this.BtSetup.Text = "Setup";
            this.BtSetup.UseVisualStyleBackColor = true;
            this.BtSetup.Click += new System.EventHandler(this.BtAuto1_Click);
            // 
            // BtAutoSum
            // 
            this.BtAutoSum.Location = new System.Drawing.Point(622, 4);
            this.BtAutoSum.Name = "BtAutoSum";
            this.BtAutoSum.Size = new System.Drawing.Size(75, 23);
            this.BtAutoSum.TabIndex = 9;
            this.BtAutoSum.Text = "Sum";
            this.BtAutoSum.UseVisualStyleBackColor = true;
            this.BtAutoSum.Click += new System.EventHandler(this.BtAutoSum_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(929, 493);
            this.Controls.Add(this.TerminalOutput);
            this.Controls.Add(this.PnSendCommand);
            this.Controls.Add(this.ClientProperty);
            this.Controls.Add(this.panel1);
            this.Name = "MainForm";
            this.Text = "TCP Application Terminal";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainForm_FormClosing);
            this.PnSendCommand.ResumeLayout(false);
            this.PnSendCommand.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel PnSendCommand;
        private System.Windows.Forms.Button BtSend;
        private System.Windows.Forms.RichTextBox TerminalOutput;
        private System.Windows.Forms.TextBox TxtHostName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button BtConnect;
        private System.Windows.Forms.TextBox TxtPort;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ComboBox CommandBox;
        private System.Windows.Forms.ComboBox CbClientType;
        private System.Windows.Forms.PropertyGrid ClientProperty;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox TxtTimeout;
        private System.Windows.Forms.CheckBox ChkCRLF;
        private System.Windows.Forms.Button BtSetup;
        private System.Windows.Forms.Button BtAutoSum;
    }
}