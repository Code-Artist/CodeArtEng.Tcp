namespace CodeArtEng.Tcp
{
    partial class TcpClientsList
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.ClientTable = new System.Windows.Forms.DataGridView();
            this.colName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.colStatus = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.ClientTable)).BeginInit();
            this.SuspendLayout();
            // 
            // ClientTable
            // 
            this.ClientTable.AllowUserToAddRows = false;
            this.ClientTable.AllowUserToDeleteRows = false;
            this.ClientTable.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.ClientTable.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colName,
            this.colStatus});
            this.ClientTable.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ClientTable.Location = new System.Drawing.Point(0, 0);
            this.ClientTable.Name = "ClientTable";
            this.ClientTable.RowHeadersVisible = false;
            this.ClientTable.Size = new System.Drawing.Size(265, 347);
            this.ClientTable.TabIndex = 0;
            this.ClientTable.SelectionChanged += new System.EventHandler(this.ClientTable_SelectionChanged);
            // 
            // colName
            // 
            this.colName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.colName.HeaderText = "Name";
            this.colName.Name = "colName";
            this.colName.ReadOnly = true;
            // 
            // colStatus
            // 
            this.colStatus.HeaderText = "Status";
            this.colStatus.Name = "colStatus";
            this.colStatus.ReadOnly = true;
            // 
            // TcpClientsList
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.ClientTable);
            this.Name = "TcpClientsList";
            this.Size = new System.Drawing.Size(265, 347);
            ((System.ComponentModel.ISupportInitialize)(this.ClientTable)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView ClientTable;
        private System.Windows.Forms.DataGridViewTextBoxColumn colName;
        private System.Windows.Forms.DataGridViewTextBoxColumn colStatus;
    }
}
