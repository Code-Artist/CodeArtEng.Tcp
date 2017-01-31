namespace TcpServerTests
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.btServerStart = new System.Windows.Forms.Button();
            this.btSeverStop = new System.Windows.Forms.Button();
            this.txtServerPort = new System.Windows.Forms.TextBox();
            this.txtServerStatus = new System.Windows.Forms.TextBox();
            this.lstClients = new System.Windows.Forms.ListBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.btMultiClients = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.lstClients);
            this.groupBox1.Controls.Add(this.txtServerStatus);
            this.groupBox1.Controls.Add(this.txtServerPort);
            this.groupBox1.Controls.Add(this.btSeverStop);
            this.groupBox1.Controls.Add(this.btServerStart);
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(271, 136);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Server";
            // 
            // btServerStart
            // 
            this.btServerStart.Location = new System.Drawing.Point(6, 45);
            this.btServerStart.Name = "btServerStart";
            this.btServerStart.Size = new System.Drawing.Size(75, 23);
            this.btServerStart.TabIndex = 0;
            this.btServerStart.Text = "Start";
            this.btServerStart.UseVisualStyleBackColor = true;
            this.btServerStart.Click += new System.EventHandler(this.btServerStart_Click);
            // 
            // btSeverStop
            // 
            this.btSeverStop.Location = new System.Drawing.Point(6, 74);
            this.btSeverStop.Name = "btSeverStop";
            this.btSeverStop.Size = new System.Drawing.Size(75, 23);
            this.btSeverStop.TabIndex = 1;
            this.btSeverStop.Text = "Stop";
            this.btSeverStop.UseVisualStyleBackColor = true;
            this.btSeverStop.Click += new System.EventHandler(this.btSeverStop_Click);
            // 
            // txtServerPort
            // 
            this.txtServerPort.Location = new System.Drawing.Point(6, 19);
            this.txtServerPort.Name = "txtServerPort";
            this.txtServerPort.Size = new System.Drawing.Size(75, 20);
            this.txtServerPort.TabIndex = 2;
            this.txtServerPort.Text = "1020";
            // 
            // txtServerStatus
            // 
            this.txtServerStatus.Location = new System.Drawing.Point(6, 103);
            this.txtServerStatus.Name = "txtServerStatus";
            this.txtServerStatus.ReadOnly = true;
            this.txtServerStatus.Size = new System.Drawing.Size(75, 20);
            this.txtServerStatus.TabIndex = 3;
            // 
            // lstClients
            // 
            this.lstClients.FormattingEnabled = true;
            this.lstClients.Location = new System.Drawing.Point(87, 19);
            this.lstClients.Name = "lstClients";
            this.lstClients.Size = new System.Drawing.Size(120, 108);
            this.lstClients.TabIndex = 4;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.btMultiClients);
            this.groupBox2.Location = new System.Drawing.Point(12, 154);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(271, 142);
            this.groupBox2.TabIndex = 1;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Client";
            // 
            // btMultiClients
            // 
            this.btMultiClients.Location = new System.Drawing.Point(6, 19);
            this.btMultiClients.Name = "btMultiClients";
            this.btMultiClients.Size = new System.Drawing.Size(75, 23);
            this.btMultiClients.TabIndex = 0;
            this.btMultiClients.Text = "MultiClients";
            this.btMultiClients.UseVisualStyleBackColor = true;
            this.btMultiClients.Click += new System.EventHandler(this.btMultiClients_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(502, 400);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Name = "Form1";
            this.Text = "Form1";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.Form1_FormClosing);
            this.Shown += new System.EventHandler(this.Form1_Shown);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.ListBox lstClients;
        private System.Windows.Forms.TextBox txtServerStatus;
        private System.Windows.Forms.TextBox txtServerPort;
        private System.Windows.Forms.Button btSeverStop;
        private System.Windows.Forms.Button btServerStart;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button btMultiClients;
    }
}

