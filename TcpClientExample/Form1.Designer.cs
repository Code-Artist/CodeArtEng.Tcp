namespace TcpClientExample
{
    partial class Form1
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
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.propertyGrid1 = new System.Windows.Forms.PropertyGrid();
            this.panel3 = new System.Windows.Forms.Panel();
            this.btDisconnect = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.btConnect = new System.Windows.Forms.Button();
            this.txtPort = new System.Windows.Forms.TextBox();
            this.tcpClientLog = new System.Windows.Forms.RichTextBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btWrite = new System.Windows.Forms.Button();
            this.label5 = new System.Windows.Forms.Label();
            this.txtInput = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.tcpAppLog = new System.Windows.Forms.RichTextBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.btHelp = new System.Windows.Forms.Button();
            this.btDisconnectApp = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.txtConnectString = new System.Windows.Forms.TextBox();
            this.btConnectApp = new System.Windows.Forms.Button();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(881, 479);
            this.tabControl1.TabIndex = 2;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.splitContainer1);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(873, 453);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "TCP Client";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(3, 3);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.propertyGrid1);
            this.splitContainer1.Panel1.Controls.Add(this.panel3);
            this.splitContainer1.Panel1.Padding = new System.Windows.Forms.Padding(3);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.tcpClientLog);
            this.splitContainer1.Panel2.Controls.Add(this.panel1);
            this.splitContainer1.Size = new System.Drawing.Size(867, 447);
            this.splitContainer1.SplitterDistance = 288;
            this.splitContainer1.TabIndex = 5;
            // 
            // propertyGrid1
            // 
            this.propertyGrid1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propertyGrid1.Location = new System.Drawing.Point(3, 34);
            this.propertyGrid1.Name = "propertyGrid1";
            this.propertyGrid1.Size = new System.Drawing.Size(282, 410);
            this.propertyGrid1.TabIndex = 0;
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.btDisconnect);
            this.panel3.Controls.Add(this.label2);
            this.panel3.Controls.Add(this.btConnect);
            this.panel3.Controls.Add(this.txtPort);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel3.Location = new System.Drawing.Point(3, 3);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(282, 31);
            this.panel3.TabIndex = 0;
            // 
            // btDisconnect
            // 
            this.btDisconnect.Location = new System.Drawing.Point(198, 3);
            this.btDisconnect.Name = "btDisconnect";
            this.btDisconnect.Size = new System.Drawing.Size(69, 23);
            this.btDisconnect.TabIndex = 3;
            this.btDisconnect.Text = "Disconnect";
            this.btDisconnect.UseVisualStyleBackColor = true;
            this.btDisconnect.Click += new System.EventHandler(this.btDisconnect_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(8, 8);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(26, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Port";
            // 
            // btConnect
            // 
            this.btConnect.Location = new System.Drawing.Point(123, 3);
            this.btConnect.Name = "btConnect";
            this.btConnect.Size = new System.Drawing.Size(69, 23);
            this.btConnect.TabIndex = 0;
            this.btConnect.Text = "Connect";
            this.btConnect.UseVisualStyleBackColor = true;
            this.btConnect.Click += new System.EventHandler(this.btConnect_Click_1);
            // 
            // txtPort
            // 
            this.txtPort.Location = new System.Drawing.Point(40, 5);
            this.txtPort.Name = "txtPort";
            this.txtPort.Size = new System.Drawing.Size(49, 20);
            this.txtPort.TabIndex = 1;
            this.txtPort.Text = "1000";
            // 
            // tcpClientLog
            // 
            this.tcpClientLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tcpClientLog.Location = new System.Drawing.Point(0, 54);
            this.tcpClientLog.Name = "tcpClientLog";
            this.tcpClientLog.ReadOnly = true;
            this.tcpClientLog.Size = new System.Drawing.Size(575, 393);
            this.tcpClientLog.TabIndex = 2;
            this.tcpClientLog.Text = "";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btWrite);
            this.panel1.Controls.Add(this.label5);
            this.panel1.Controls.Add(this.txtInput);
            this.panel1.Controls.Add(this.label4);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(575, 54);
            this.panel1.TabIndex = 1;
            // 
            // btWrite
            // 
            this.btWrite.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btWrite.Location = new System.Drawing.Point(495, 6);
            this.btWrite.Name = "btWrite";
            this.btWrite.Size = new System.Drawing.Size(75, 23);
            this.btWrite.TabIndex = 8;
            this.btWrite.Text = "Write";
            this.btWrite.UseVisualStyleBackColor = true;
            this.btWrite.Click += new System.EventHandler(this.btWrite_Click);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(48, 31);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(114, 13);
            this.label5.TabIndex = 7;
            this.label5.Text = "Press ENTER to send.";
            // 
            // txtInput
            // 
            this.txtInput.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtInput.Location = new System.Drawing.Point(51, 8);
            this.txtInput.Name = "txtInput";
            this.txtInput.Size = new System.Drawing.Size(438, 20);
            this.txtInput.TabIndex = 6;
            this.txtInput.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtInput_KeyDown);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(10, 11);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(37, 13);
            this.label4.TabIndex = 5;
            this.label4.Text = "SEND";
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.tcpAppLog);
            this.tabPage2.Controls.Add(this.panel2);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(873, 453);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "TCP App Client";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // tcpAppLog
            // 
            this.tcpAppLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tcpAppLog.Location = new System.Drawing.Point(123, 3);
            this.tcpAppLog.Name = "tcpAppLog";
            this.tcpAppLog.Size = new System.Drawing.Size(747, 447);
            this.tcpAppLog.TabIndex = 0;
            this.tcpAppLog.Text = "";
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.btHelp);
            this.panel2.Controls.Add(this.btDisconnectApp);
            this.panel2.Controls.Add(this.label1);
            this.panel2.Controls.Add(this.txtConnectString);
            this.panel2.Controls.Add(this.btConnectApp);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel2.Location = new System.Drawing.Point(3, 3);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(120, 447);
            this.panel2.TabIndex = 1;
            // 
            // btHelp
            // 
            this.btHelp.Location = new System.Drawing.Point(8, 115);
            this.btHelp.Name = "btHelp";
            this.btHelp.Size = new System.Drawing.Size(100, 23);
            this.btHelp.TabIndex = 4;
            this.btHelp.Text = "Help";
            this.btHelp.UseVisualStyleBackColor = true;
            this.btHelp.Click += new System.EventHandler(this.btHelp_Click);
            // 
            // btDisconnectApp
            // 
            this.btDisconnectApp.Location = new System.Drawing.Point(8, 74);
            this.btDisconnectApp.Name = "btDisconnectApp";
            this.btDisconnectApp.Size = new System.Drawing.Size(100, 23);
            this.btDisconnectApp.TabIndex = 3;
            this.btDisconnectApp.Text = "Disconnect";
            this.btDisconnectApp.UseVisualStyleBackColor = true;
            this.btDisconnectApp.Click += new System.EventHandler(this.btDisconnectApp_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(5, 3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(91, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Connection String";
            // 
            // txtConnectString
            // 
            this.txtConnectString.Location = new System.Drawing.Point(8, 19);
            this.txtConnectString.Name = "txtConnectString";
            this.txtConnectString.Size = new System.Drawing.Size(100, 20);
            this.txtConnectString.TabIndex = 1;
            this.txtConnectString.Text = "127.0.0.1:12000";
            // 
            // btConnectApp
            // 
            this.btConnectApp.Location = new System.Drawing.Point(8, 45);
            this.btConnectApp.Name = "btConnectApp";
            this.btConnectApp.Size = new System.Drawing.Size(100, 23);
            this.btConnectApp.TabIndex = 0;
            this.btConnectApp.Text = "Connect";
            this.btConnectApp.UseVisualStyleBackColor = true;
            this.btConnectApp.Click += new System.EventHandler(this.btConnectApp_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(881, 479);
            this.Controls.Add(this.tabControl1);
            this.Name = "Form1";
            this.Text = "TCP Client";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.RichTextBox tcpAppLog;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtConnectString;
        private System.Windows.Forms.Button btConnectApp;
        private System.Windows.Forms.Button btDisconnectApp;
        private System.Windows.Forms.Button btHelp;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.PropertyGrid propertyGrid1;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Button btDisconnect;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btConnect;
        private System.Windows.Forms.TextBox txtPort;
        private System.Windows.Forms.RichTextBox tcpClientLog;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtInput;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button btWrite;
    }
}

