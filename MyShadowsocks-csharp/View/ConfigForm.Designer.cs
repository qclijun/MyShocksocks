namespace Shadowsocks.View
{
    partial class ConfigForm
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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.serversListBox = new System.Windows.Forms.ListBox();
            this.ServerGroupBox = new System.Windows.Forms.GroupBox();
            this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
            this.label3 = new System.Windows.Forms.Label();
            this.PortTextBox = new System.Windows.Forms.TextBox();
            this.IPLabel = new System.Windows.Forms.Label();
            this.IPTextBox = new System.Windows.Forms.TextBox();
            this.PortLabel = new System.Windows.Forms.Label();
            this.PwdLabel = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.PwdTextBox = new System.Windows.Forms.TextBox();
            this.RemarkTextBox = new System.Windows.Forms.TextBox();
            this.TimeoutTextBox = new System.Windows.Forms.TextBox();
            this.EncryptionSelect = new System.Windows.Forms.ComboBox();
            this.OneTimeAuth = new System.Windows.Forms.CheckBox();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.MyCancelBtn = new System.Windows.Forms.Button();
            this.OkBtn = new System.Windows.Forms.Button();
            this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.flowLayoutPanel2 = new System.Windows.Forms.FlowLayoutPanel();
            this.MoveUpBtn = new System.Windows.Forms.Button();
            this.MoveDownBtn = new System.Windows.Forms.Button();
            this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
            this.AddBtn = new System.Windows.Forms.Button();
            this.DeleteBtn = new System.Windows.Forms.Button();
            this.DuplicateBtn = new System.Windows.Forms.Button();
            this.tableLayoutPanel1.SuspendLayout();
            this.ServerGroupBox.SuspendLayout();
            this.tableLayoutPanel2.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.tableLayoutPanel3.SuspendLayout();
            this.flowLayoutPanel2.SuspendLayout();
            this.tableLayoutPanel4.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.serversListBox, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.ServerGroupBox, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel1, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel3, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel2, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.tableLayoutPanel4, 0, 1);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(15, 15);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(15, 3, 3, 15);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.Size = new System.Drawing.Size(688, 433);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // serversListBox
            // 
            this.serversListBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.serversListBox.FormattingEnabled = true;
            this.serversListBox.IntegralHeight = false;
            this.serversListBox.ItemHeight = 15;
            this.serversListBox.Location = new System.Drawing.Point(0, 0);
            this.serversListBox.Margin = new System.Windows.Forms.Padding(0);
            this.serversListBox.Name = "serversListBox";
            this.serversListBox.Size = new System.Drawing.Size(206, 248);
            this.serversListBox.TabIndex = 7;
            // 
            // ServerGroupBox
            // 
            this.ServerGroupBox.Controls.Add(this.tableLayoutPanel2);
            this.ServerGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ServerGroupBox.Location = new System.Drawing.Point(216, 0);
            this.ServerGroupBox.Margin = new System.Windows.Forms.Padding(10, 0, 0, 0);
            this.ServerGroupBox.Name = "ServerGroupBox";
            this.ServerGroupBox.Padding = new System.Windows.Forms.Padding(4);
            this.ServerGroupBox.Size = new System.Drawing.Size(472, 248);
            this.ServerGroupBox.TabIndex = 8;
            this.ServerGroupBox.TabStop = false;
            this.ServerGroupBox.Text = "Server";
            // 
            // tableLayoutPanel2
            // 
            this.tableLayoutPanel2.AutoSize = true;
            this.tableLayoutPanel2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.tableLayoutPanel2.ColumnCount = 2;
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel2.Controls.Add(this.label3, 0, 6);
            this.tableLayoutPanel2.Controls.Add(this.PortTextBox, 1, 1);
            this.tableLayoutPanel2.Controls.Add(this.IPLabel, 0, 0);
            this.tableLayoutPanel2.Controls.Add(this.IPTextBox, 1, 0);
            this.tableLayoutPanel2.Controls.Add(this.PortLabel, 0, 1);
            this.tableLayoutPanel2.Controls.Add(this.PwdLabel, 0, 2);
            this.tableLayoutPanel2.Controls.Add(this.label1, 0, 3);
            this.tableLayoutPanel2.Controls.Add(this.label2, 0, 4);
            this.tableLayoutPanel2.Controls.Add(this.PwdTextBox, 1, 2);
            this.tableLayoutPanel2.Controls.Add(this.RemarkTextBox, 1, 4);
            this.tableLayoutPanel2.Controls.Add(this.TimeoutTextBox, 1, 6);
            this.tableLayoutPanel2.Controls.Add(this.EncryptionSelect, 1, 3);
            this.tableLayoutPanel2.Controls.Add(this.OneTimeAuth, 1, 7);
            this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel2.Location = new System.Drawing.Point(4, 22);
            this.tableLayoutPanel2.Name = "tableLayoutPanel2";
            this.tableLayoutPanel2.RowCount = 8;
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel2.Size = new System.Drawing.Size(464, 222);
            this.tableLayoutPanel2.TabIndex = 0;
            // 
            // label3
            // 
            this.label3.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(4, 172);
            this.label3.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(103, 15);
            this.label3.TabIndex = 6;
            this.label3.Text = "Timeout(sec)";
            // 
            // PortTextBox
            // 
            this.PortTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.PortTextBox.Location = new System.Drawing.Point(115, 37);
            this.PortTextBox.Margin = new System.Windows.Forms.Padding(4);
            this.PortTextBox.MaxLength = 512;
            this.PortTextBox.Name = "PortTextBox";
            this.PortTextBox.Size = new System.Drawing.Size(345, 25);
            this.PortTextBox.TabIndex = 1;
            this.PortTextBox.WordWrap = false;
            // 
            // IPLabel
            // 
            this.IPLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.IPLabel.AutoSize = true;
            this.IPLabel.Location = new System.Drawing.Point(28, 9);
            this.IPLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.IPLabel.Name = "IPLabel";
            this.IPLabel.Size = new System.Drawing.Size(79, 15);
            this.IPLabel.TabIndex = 0;
            this.IPLabel.Text = "Server IP";
     
            // 
            // IPTextBox
            // 
            this.IPTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.IPTextBox.Location = new System.Drawing.Point(115, 4);
            this.IPTextBox.Margin = new System.Windows.Forms.Padding(4);
            this.IPTextBox.MaxLength = 512;
            this.IPTextBox.Name = "IPTextBox";
            this.IPTextBox.Size = new System.Drawing.Size(345, 25);
            this.IPTextBox.TabIndex = 0;
            this.IPTextBox.WordWrap = false;
            // 
            // PortLabel
            // 
            this.PortLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.PortLabel.AutoSize = true;
            this.PortLabel.Location = new System.Drawing.Point(13, 42);
            this.PortLabel.Name = "PortLabel";
            this.PortLabel.Size = new System.Drawing.Size(95, 15);
            this.PortLabel.TabIndex = 1;
            this.PortLabel.Text = "Server Port";
            // 
            // PwdLabel
            // 
            this.PwdLabel.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.PwdLabel.AutoSize = true;
            this.PwdLabel.Location = new System.Drawing.Point(36, 75);
            this.PwdLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.PwdLabel.Name = "PwdLabel";
            this.PwdLabel.Size = new System.Drawing.Size(71, 15);
            this.PwdLabel.TabIndex = 2;
            this.PwdLabel.Text = "Password";
            // 
            // label1
            // 
            this.label1.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(21, 107);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(87, 15);
            this.label1.TabIndex = 3;
            this.label1.Text = "Encryption";
            // 
            // label2
            // 
            this.label2.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(45, 139);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(63, 15);
            this.label2.TabIndex = 4;
            this.label2.Text = "Remarks";
            // 
            // PwdTextBox
            // 
            this.PwdTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.PwdTextBox.Location = new System.Drawing.Point(115, 70);
            this.PwdTextBox.Margin = new System.Windows.Forms.Padding(4);
            this.PwdTextBox.MaxLength = 512;
            this.PwdTextBox.Name = "PwdTextBox";
            this.PwdTextBox.Size = new System.Drawing.Size(345, 25);
            this.PwdTextBox.TabIndex = 2;
            this.PwdTextBox.WordWrap = false;
            // 
            // RemarkTextBox
            // 
            this.RemarkTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.RemarkTextBox.Location = new System.Drawing.Point(115, 134);
            this.RemarkTextBox.Margin = new System.Windows.Forms.Padding(4);
            this.RemarkTextBox.MaxLength = 512;
            this.RemarkTextBox.Name = "RemarkTextBox";
            this.RemarkTextBox.Size = new System.Drawing.Size(345, 25);
            this.RemarkTextBox.TabIndex = 4;
            this.RemarkTextBox.WordWrap = false;
            // 
            // TimeoutTextBox
            // 
            this.TimeoutTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.TimeoutTextBox.Location = new System.Drawing.Point(115, 167);
            this.TimeoutTextBox.Margin = new System.Windows.Forms.Padding(4);
            this.TimeoutTextBox.MaxLength = 512;
            this.TimeoutTextBox.Name = "TimeoutTextBox";
            this.TimeoutTextBox.Size = new System.Drawing.Size(345, 25);
            this.TimeoutTextBox.TabIndex = 5;
            this.TimeoutTextBox.WordWrap = false;
            // 
            // EncryptionSelect
            // 
            this.EncryptionSelect.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.EncryptionSelect.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.EncryptionSelect.FormattingEnabled = true;
            this.EncryptionSelect.Location = new System.Drawing.Point(115, 103);
            this.EncryptionSelect.Margin = new System.Windows.Forms.Padding(4);
            this.EncryptionSelect.Name = "EncryptionSelect";
            this.EncryptionSelect.Size = new System.Drawing.Size(345, 23);
            this.EncryptionSelect.TabIndex = 3;
            // 
            // OneTimeAuth
            // 
            this.OneTimeAuth.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.OneTimeAuth.AutoSize = true;
            this.OneTimeAuth.Location = new System.Drawing.Point(115, 200);
            this.OneTimeAuth.Margin = new System.Windows.Forms.Padding(4);
            this.OneTimeAuth.Name = "OneTimeAuth";
            this.OneTimeAuth.Size = new System.Drawing.Size(205, 19);
            this.OneTimeAuth.TabIndex = 6;
            this.OneTimeAuth.Text = "OneTime Authentication";
            this.OneTimeAuth.UseVisualStyleBackColor = true;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.flowLayoutPanel1.AutoSize = true;
            this.flowLayoutPanel1.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanel1.Controls.Add(this.MyCancelBtn);
            this.flowLayoutPanel1.Controls.Add(this.OkBtn);
            this.flowLayoutPanel1.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(209, 375);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(476, 37);
            this.flowLayoutPanel1.TabIndex = 9;
            // 
            // MyCancelBtn
            // 
            this.MyCancelBtn.Location = new System.Drawing.Point(378, 4);
            this.MyCancelBtn.Margin = new System.Windows.Forms.Padding(4);
            this.MyCancelBtn.Name = "MyCancelBtn";
            this.MyCancelBtn.Size = new System.Drawing.Size(94, 29);
            this.MyCancelBtn.TabIndex = 0;
            this.MyCancelBtn.Text = "Cancel";
            this.MyCancelBtn.UseVisualStyleBackColor = true;
            // 
            // OkBtn
            // 
            this.OkBtn.Location = new System.Drawing.Point(276, 4);
            this.OkBtn.Margin = new System.Windows.Forms.Padding(4);
            this.OkBtn.Name = "OkBtn";
            this.OkBtn.Size = new System.Drawing.Size(94, 29);
            this.OkBtn.TabIndex = 1;
            this.OkBtn.Text = "Ok";
            this.OkBtn.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel3
            // 
            this.tableLayoutPanel3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel3.ColumnCount = 2;
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.Controls.Add(this.textBox1, 1, 0);
            this.tableLayoutPanel3.Controls.Add(this.label4, 0, 0);
            this.tableLayoutPanel3.Location = new System.Drawing.Point(335, 251);
            this.tableLayoutPanel3.Name = "tableLayoutPanel3";
            this.tableLayoutPanel3.RowCount = 1;
            this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel3.Size = new System.Drawing.Size(350, 100);
            this.tableLayoutPanel3.TabIndex = 10;
            // 
            // textBox1
            // 
            this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox1.Location = new System.Drawing.Point(179, 37);
            this.textBox1.Margin = new System.Windows.Forms.Padding(4);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(167, 25);
            this.textBox1.TabIndex = 0;
            // 
            // label4
            // 
            this.label4.Anchor = System.Windows.Forms.AnchorStyles.Right;
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(85, 42);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(87, 15);
            this.label4.TabIndex = 1;
            this.label4.Text = "Proxy Port";
            // 
            // flowLayoutPanel2
            // 
            this.flowLayoutPanel2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right)));
            this.flowLayoutPanel2.AutoSize = true;
            this.flowLayoutPanel2.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.flowLayoutPanel2.Controls.Add(this.MoveUpBtn);
            this.flowLayoutPanel2.Controls.Add(this.MoveDownBtn);
            this.flowLayoutPanel2.Location = new System.Drawing.Point(3, 375);
            this.flowLayoutPanel2.Name = "flowLayoutPanel2";
            this.flowLayoutPanel2.Size = new System.Drawing.Size(200, 37);
            this.flowLayoutPanel2.TabIndex = 11;
            // 
            // MoveUpBtn
            // 
            this.MoveUpBtn.Anchor = System.Windows.Forms.AnchorStyles.Left;
            this.MoveUpBtn.Location = new System.Drawing.Point(0, 4);
            this.MoveUpBtn.Margin = new System.Windows.Forms.Padding(0, 4, 4, 4);
            this.MoveUpBtn.Name = "MoveUpBtn";
            this.MoveUpBtn.Size = new System.Drawing.Size(94, 29);
            this.MoveUpBtn.TabIndex = 0;
            this.MoveUpBtn.Text = "Move Up";
            this.MoveUpBtn.UseVisualStyleBackColor = true;
            // 
            // MoveDownBtn
            // 
            this.MoveDownBtn.Location = new System.Drawing.Point(102, 4);
            this.MoveDownBtn.Margin = new System.Windows.Forms.Padding(4, 4, 0, 4);
            this.MoveDownBtn.Name = "MoveDownBtn";
            this.MoveDownBtn.Size = new System.Drawing.Size(94, 29);
            this.MoveDownBtn.TabIndex = 1;
            this.MoveDownBtn.Text = "Move Down";
            this.MoveDownBtn.UseVisualStyleBackColor = true;
            // 
            // tableLayoutPanel4
            // 
            this.tableLayoutPanel4.ColumnCount = 2;
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel4.Controls.Add(this.AddBtn, 0, 0);
            this.tableLayoutPanel4.Controls.Add(this.DeleteBtn, 1, 0);
            this.tableLayoutPanel4.Controls.Add(this.DuplicateBtn, 0, 1);
            this.tableLayoutPanel4.Location = new System.Drawing.Point(3, 251);
            this.tableLayoutPanel4.Name = "tableLayoutPanel4";
            this.tableLayoutPanel4.RowCount = 2;
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel4.Size = new System.Drawing.Size(200, 100);
            this.tableLayoutPanel4.TabIndex = 12;
            // 
            // AddBtn
            // 
            this.AddBtn.Dock = System.Windows.Forms.DockStyle.Left;
            this.AddBtn.Location = new System.Drawing.Point(0, 8);
            this.AddBtn.Margin = new System.Windows.Forms.Padding(0, 8, 4, 4);
            this.AddBtn.Name = "AddBtn";
            this.AddBtn.Size = new System.Drawing.Size(94, 38);
            this.AddBtn.TabIndex = 0;
            this.AddBtn.Text = "Add";
            this.AddBtn.UseVisualStyleBackColor = true;
            // 
            // DeleteBtn
            // 
            this.DeleteBtn.Dock = System.Windows.Forms.DockStyle.Right;
            this.DeleteBtn.Location = new System.Drawing.Point(106, 8);
            this.DeleteBtn.Margin = new System.Windows.Forms.Padding(4, 8, 0, 4);
            this.DeleteBtn.Name = "DeleteBtn";
            this.DeleteBtn.Size = new System.Drawing.Size(94, 38);
            this.DeleteBtn.TabIndex = 1;
            this.DeleteBtn.Text = "Delete";
            this.DeleteBtn.UseVisualStyleBackColor = true;
            // 
            // DuplicateBtn
            // 
            this.DuplicateBtn.Dock = System.Windows.Forms.DockStyle.Left;
            this.DuplicateBtn.Location = new System.Drawing.Point(0, 54);
            this.DuplicateBtn.Margin = new System.Windows.Forms.Padding(0, 4, 4, 4);
            this.DuplicateBtn.Name = "DuplicateBtn";
            this.DuplicateBtn.Size = new System.Drawing.Size(94, 42);
            this.DuplicateBtn.TabIndex = 2;
            this.DuplicateBtn.Text = "Duplicate";
            this.DuplicateBtn.UseVisualStyleBackColor = true;
            // 
            // ConfigForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(120F, 120F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(718, 459);
            this.Controls.Add(this.tableLayoutPanel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "ConfigForm";
            this.Padding = new System.Windows.Forms.Padding(15, 15, 15, 11);
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "EditServers";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ServerGroupBox.ResumeLayout(false);
            this.ServerGroupBox.PerformLayout();
            this.tableLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel2.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel3.ResumeLayout(false);
            this.tableLayoutPanel3.PerformLayout();
            this.flowLayoutPanel2.ResumeLayout(false);
            this.tableLayoutPanel4.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.ListBox serversListBox;
        private System.Windows.Forms.GroupBox ServerGroupBox;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel2;
        private System.Windows.Forms.Label IPLabel;
        private System.Windows.Forms.TextBox IPTextBox;
        private System.Windows.Forms.TextBox PortTextBox;
        private System.Windows.Forms.Label PortLabel;
        private System.Windows.Forms.Label PwdLabel;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox PwdTextBox;
        private System.Windows.Forms.TextBox RemarkTextBox;
        private System.Windows.Forms.TextBox TimeoutTextBox;
        private System.Windows.Forms.ComboBox EncryptionSelect;
        private System.Windows.Forms.CheckBox OneTimeAuth;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Button MyCancelBtn;
        private System.Windows.Forms.Button OkBtn;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel3;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel2;
        private System.Windows.Forms.Button MoveUpBtn;
        private System.Windows.Forms.Button MoveDownBtn;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel4;
        private System.Windows.Forms.Button AddBtn;
        private System.Windows.Forms.Button DeleteBtn;
        private System.Windows.Forms.Button DuplicateBtn;
    }
}