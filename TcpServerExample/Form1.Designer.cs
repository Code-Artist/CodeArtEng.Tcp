namespace TcpServerExample
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
            this.propertyGrid1 = new System.Windows.Forms.PropertyGrid();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label5 = new System.Windows.Forms.Label();
            this.txtServerSend = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.txtReply = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtPort = new System.Windows.Forms.TextBox();
            this.BtStart = new System.Windows.Forms.Button();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.panel3 = new System.Windows.Forms.Panel();
            this.BtStop = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.tcpServerLog = new System.Windows.Forms.RichTextBox();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.propertyGrid2 = new System.Windows.Forms.PropertyGrid();
            this.panel2 = new System.Windows.Forms.Panel();
            this.btAppServerStop = new System.Windows.Forms.Button();
            this.label6 = new System.Windows.Forms.Label();
            this.btAppServerStart = new System.Windows.Forms.Button();
            this.txtAppServerPort = new System.Windows.Forms.TextBox();
            this.tcpAppServerLog = new System.Windows.Forms.RichTextBox();
            this.panel4 = new System.Windows.Forms.Panel();
            this.panel1.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.panel3.SuspendLayout();
            this.tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // propertyGrid1
            // 
            this.propertyGrid1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propertyGrid1.Location = new System.Drawing.Point(3, 34);
            this.propertyGrid1.Name = "propertyGrid1";
            this.propertyGrid1.Size = new System.Drawing.Size(278, 426);
            this.propertyGrid1.TabIndex = 0;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.label5);
            this.panel1.Controls.Add(this.txtServerSend);
            this.panel1.Controls.Add(this.label4);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.txtReply);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(565, 93);
            this.panel1.TabIndex = 1;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.Location = new System.Drawing.Point(68, 71);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(230, 13);
            this.label5.TabIndex = 7;
            this.label5.Text = "Press ENTER to send. (Broadcast to all Clients)";
            // 
            // txtServerSend
            // 
            this.txtServerSend.Location = new System.Drawing.Point(71, 48);
            this.txtServerSend.Name = "txtServerSend";
            this.txtServerSend.Size = new System.Drawing.Size(481, 20);
            this.txtServerSend.TabIndex = 6;
            this.txtServerSend.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtServerSend_KeyDown);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(30, 51);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(37, 13);
            this.label4.TabIndex = 5;
            this.label4.Text = "SEND";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Italic, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(68, 30);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(226, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Leave the text box empty to disable auto reply.";
            // 
            // txtReply
            // 
            this.txtReply.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtReply.Location = new System.Drawing.Point(71, 6);
            this.txtReply.Name = "txtReply";
            this.txtReply.Size = new System.Drawing.Size(481, 20);
            this.txtReply.TabIndex = 3;
            this.txtReply.Text = "ACK";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 8);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(59, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Auto Reply";
            // 
            // txtPort
            // 
            this.txtPort.Location = new System.Drawing.Point(40, 5);
            this.txtPort.Name = "txtPort";
            this.txtPort.Size = new System.Drawing.Size(49, 20);
            this.txtPort.TabIndex = 1;
            this.txtPort.Text = "1000";
            // 
            // BtStart
            // 
            this.BtStart.Location = new System.Drawing.Point(95, 3);
            this.BtStart.Name = "BtStart";
            this.BtStart.Size = new System.Drawing.Size(54, 23);
            this.BtStart.TabIndex = 0;
            this.BtStart.Text = "Start";
            this.BtStart.UseVisualStyleBackColor = true;
            this.BtStart.Click += new System.EventHandler(this.BtStart_Click);
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(867, 495);
            this.tabControl1.TabIndex = 2;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.splitContainer1);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(859, 469);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "TCP Server";
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
            this.splitContainer1.Panel2.Controls.Add(this.tcpServerLog);
            this.splitContainer1.Panel2.Controls.Add(this.panel1);
            this.splitContainer1.Size = new System.Drawing.Size(853, 463);
            this.splitContainer1.SplitterDistance = 284;
            this.splitContainer1.TabIndex = 4;
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.BtStop);
            this.panel3.Controls.Add(this.label2);
            this.panel3.Controls.Add(this.BtStart);
            this.panel3.Controls.Add(this.txtPort);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel3.Location = new System.Drawing.Point(3, 3);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(278, 31);
            this.panel3.TabIndex = 0;
            // 
            // BtStop
            // 
            this.BtStop.Location = new System.Drawing.Point(155, 3);
            this.BtStop.Name = "BtStop";
            this.BtStop.Size = new System.Drawing.Size(54, 23);
            this.BtStop.TabIndex = 3;
            this.BtStop.Text = "Stop";
            this.BtStop.UseVisualStyleBackColor = true;
            this.BtStop.Click += new System.EventHandler(this.BtStop_Click);
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
            // tcpServerLog
            // 
            this.tcpServerLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tcpServerLog.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tcpServerLog.Location = new System.Drawing.Point(0, 93);
            this.tcpServerLog.Name = "tcpServerLog";
            this.tcpServerLog.ReadOnly = true;
            this.tcpServerLog.Size = new System.Drawing.Size(565, 370);
            this.tcpServerLog.TabIndex = 2;
            this.tcpServerLog.Text = "";
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.splitContainer2);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(859, 469);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "TCP App Server";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(3, 3);
            this.splitContainer2.Name = "splitContainer2";
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.propertyGrid2);
            this.splitContainer2.Panel1.Controls.Add(this.panel2);
            this.splitContainer2.Panel1.Padding = new System.Windows.Forms.Padding(3);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.tcpAppServerLog);
            this.splitContainer2.Panel2.Controls.Add(this.panel4);
            this.splitContainer2.Size = new System.Drawing.Size(853, 463);
            this.splitContainer2.SplitterDistance = 284;
            this.splitContainer2.TabIndex = 5;
            // 
            // propertyGrid2
            // 
            this.propertyGrid2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.propertyGrid2.Location = new System.Drawing.Point(3, 34);
            this.propertyGrid2.Name = "propertyGrid2";
            this.propertyGrid2.Size = new System.Drawing.Size(278, 426);
            this.propertyGrid2.TabIndex = 0;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.btAppServerStop);
            this.panel2.Controls.Add(this.label6);
            this.panel2.Controls.Add(this.btAppServerStart);
            this.panel2.Controls.Add(this.txtAppServerPort);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel2.Location = new System.Drawing.Point(3, 3);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(278, 31);
            this.panel2.TabIndex = 0;
            // 
            // btAppServerStop
            // 
            this.btAppServerStop.Location = new System.Drawing.Point(155, 3);
            this.btAppServerStop.Name = "btAppServerStop";
            this.btAppServerStop.Size = new System.Drawing.Size(54, 23);
            this.btAppServerStop.TabIndex = 3;
            this.btAppServerStop.Text = "Stop";
            this.btAppServerStop.UseVisualStyleBackColor = true;
            this.btAppServerStop.Click += new System.EventHandler(this.btAppServerStop_Click);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(8, 8);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(26, 13);
            this.label6.TabIndex = 2;
            this.label6.Text = "Port";
            // 
            // btAppServerStart
            // 
            this.btAppServerStart.Location = new System.Drawing.Point(95, 3);
            this.btAppServerStart.Name = "btAppServerStart";
            this.btAppServerStart.Size = new System.Drawing.Size(54, 23);
            this.btAppServerStart.TabIndex = 0;
            this.btAppServerStart.Text = "Start";
            this.btAppServerStart.UseVisualStyleBackColor = true;
            this.btAppServerStart.Click += new System.EventHandler(this.btAppServerStart_Click);
            // 
            // txtAppServerPort
            // 
            this.txtAppServerPort.Location = new System.Drawing.Point(40, 5);
            this.txtAppServerPort.Name = "txtAppServerPort";
            this.txtAppServerPort.Size = new System.Drawing.Size(49, 20);
            this.txtAppServerPort.TabIndex = 1;
            this.txtAppServerPort.Text = "12000";
            // 
            // tcpAppServerLog
            // 
            this.tcpAppServerLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tcpAppServerLog.Font = new System.Drawing.Font("Courier New", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.tcpAppServerLog.Location = new System.Drawing.Point(0, 93);
            this.tcpAppServerLog.Name = "tcpAppServerLog";
            this.tcpAppServerLog.ReadOnly = true;
            this.tcpAppServerLog.Size = new System.Drawing.Size(565, 370);
            this.tcpAppServerLog.TabIndex = 2;
            this.tcpAppServerLog.Text = "";
            // 
            // panel4
            // 
            this.panel4.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel4.Location = new System.Drawing.Point(0, 0);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(565, 93);
            this.panel4.TabIndex = 1;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(867, 495);
            this.Controls.Add(this.tabControl1);
            this.Name = "Form1";
            this.Text = "TCP Server";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Shown += new System.EventHandler(this.Form1_Shown);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.tabPage2.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PropertyGrid propertyGrid1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Button BtStart;
        private System.Windows.Forms.TextBox txtPort;
        private System.Windows.Forms.TextBox txtReply;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Button BtStop;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.RichTextBox tcpServerLog;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtServerSend;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.PropertyGrid propertyGrid2;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Button btAppServerStop;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Button btAppServerStart;
        private System.Windows.Forms.TextBox txtAppServerPort;
        private System.Windows.Forms.RichTextBox tcpAppServerLog;
        private System.Windows.Forms.Panel panel4;
    }
}

