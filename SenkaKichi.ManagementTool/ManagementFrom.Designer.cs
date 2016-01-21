namespace SenkaKichi.ManagementTool
{
    partial class ManagementFrom
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.textBoxLog = new System.Windows.Forms.TextBox();
            this.flowLayoutPanel1 = new System.Windows.Forms.FlowLayoutPanel();
            this.buttonUpdateIp = new System.Windows.Forms.Button();
            this.buttonForceUpdateData = new System.Windows.Forms.Button();
            this.buttonTest = new System.Windows.Forms.Button();
            this.buttonImportData = new System.Windows.Forms.Button();
            this.buttonCalcRanking = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.buttonPostTwitter = new System.Windows.Forms.Button();
            this.tableLayoutPanel1.SuspendLayout();
            this.flowLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 1;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Controls.Add(this.textBoxLog, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.flowLayoutPanel1, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 2;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 200F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(484, 362);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // textBoxLog
            // 
            this.textBoxLog.BackColor = System.Drawing.SystemColors.Window;
            this.textBoxLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.textBoxLog.Location = new System.Drawing.Point(3, 165);
            this.textBoxLog.Multiline = true;
            this.textBoxLog.Name = "textBoxLog";
            this.textBoxLog.ReadOnly = true;
            this.textBoxLog.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxLog.Size = new System.Drawing.Size(478, 194);
            this.textBoxLog.TabIndex = 0;
            // 
            // flowLayoutPanel1
            // 
            this.flowLayoutPanel1.Controls.Add(this.buttonUpdateIp);
            this.flowLayoutPanel1.Controls.Add(this.buttonForceUpdateData);
            this.flowLayoutPanel1.Controls.Add(this.buttonTest);
            this.flowLayoutPanel1.Controls.Add(this.buttonImportData);
            this.flowLayoutPanel1.Controls.Add(this.buttonCalcRanking);
            this.flowLayoutPanel1.Controls.Add(this.buttonPostTwitter);
            this.flowLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowLayoutPanel1.Location = new System.Drawing.Point(3, 3);
            this.flowLayoutPanel1.Name = "flowLayoutPanel1";
            this.flowLayoutPanel1.Size = new System.Drawing.Size(478, 156);
            this.flowLayoutPanel1.TabIndex = 1;
            // 
            // buttonUpdateIp
            // 
            this.buttonUpdateIp.Location = new System.Drawing.Point(5, 5);
            this.buttonUpdateIp.Margin = new System.Windows.Forms.Padding(5);
            this.buttonUpdateIp.Name = "buttonUpdateIp";
            this.buttonUpdateIp.Size = new System.Drawing.Size(149, 30);
            this.buttonUpdateIp.TabIndex = 1;
            this.buttonUpdateIp.Text = "Update IP";
            this.buttonUpdateIp.UseVisualStyleBackColor = true;
            this.buttonUpdateIp.Click += new System.EventHandler(this.buttonUpdateIp_Click);
            // 
            // buttonForceUpdateData
            // 
            this.buttonForceUpdateData.Location = new System.Drawing.Point(164, 5);
            this.buttonForceUpdateData.Margin = new System.Windows.Forms.Padding(5);
            this.buttonForceUpdateData.Name = "buttonForceUpdateData";
            this.buttonForceUpdateData.Size = new System.Drawing.Size(149, 30);
            this.buttonForceUpdateData.TabIndex = 2;
            this.buttonForceUpdateData.Text = "Force Update Data";
            this.buttonForceUpdateData.UseVisualStyleBackColor = true;
            this.buttonForceUpdateData.Click += new System.EventHandler(this.buttonForceUpdateData_Click);
            // 
            // buttonTest
            // 
            this.buttonTest.Location = new System.Drawing.Point(323, 5);
            this.buttonTest.Margin = new System.Windows.Forms.Padding(5);
            this.buttonTest.Name = "buttonTest";
            this.buttonTest.Size = new System.Drawing.Size(149, 30);
            this.buttonTest.TabIndex = 2;
            this.buttonTest.Text = "Test";
            this.buttonTest.UseVisualStyleBackColor = true;
            this.buttonTest.Click += new System.EventHandler(this.buttonTest_Click);
            // 
            // buttonImportData
            // 
            this.buttonImportData.Location = new System.Drawing.Point(5, 45);
            this.buttonImportData.Margin = new System.Windows.Forms.Padding(5);
            this.buttonImportData.Name = "buttonImportData";
            this.buttonImportData.Size = new System.Drawing.Size(149, 30);
            this.buttonImportData.TabIndex = 3;
            this.buttonImportData.Text = "Import Data";
            this.buttonImportData.UseVisualStyleBackColor = true;
            this.buttonImportData.Click += new System.EventHandler(this.buttonImportData_Click);
            // 
            // buttonCalcRanking
            // 
            this.buttonCalcRanking.Location = new System.Drawing.Point(164, 45);
            this.buttonCalcRanking.Margin = new System.Windows.Forms.Padding(5);
            this.buttonCalcRanking.Name = "buttonCalcRanking";
            this.buttonCalcRanking.Size = new System.Drawing.Size(149, 30);
            this.buttonCalcRanking.TabIndex = 4;
            this.buttonCalcRanking.Text = "Calc Ranking";
            this.buttonCalcRanking.UseVisualStyleBackColor = true;
            this.buttonCalcRanking.Click += new System.EventHandler(this.buttonCalcRanking_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            this.openFileDialog1.Filter = "SQLiteDatabase|*.db";
            // 
            // buttonPostTwitter
            // 
            this.buttonPostTwitter.Location = new System.Drawing.Point(323, 45);
            this.buttonPostTwitter.Margin = new System.Windows.Forms.Padding(5);
            this.buttonPostTwitter.Name = "buttonPostTwitter";
            this.buttonPostTwitter.Size = new System.Drawing.Size(149, 30);
            this.buttonPostTwitter.TabIndex = 5;
            this.buttonPostTwitter.Text = "Post Twitter";
            this.buttonPostTwitter.UseVisualStyleBackColor = true;
            this.buttonPostTwitter.Click += new System.EventHandler(this.buttonPostTwitter_Click);
            // 
            // ManagementFrom
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(484, 362);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "ManagementFrom";
            this.Text = "Senka Kichi Management Tool";
            this.Load += new System.EventHandler(this.ManagementFrom_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.flowLayoutPanel1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.TextBox textBoxLog;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.FlowLayoutPanel flowLayoutPanel1;
        private System.Windows.Forms.Button buttonForceUpdateData;
        private System.Windows.Forms.Button buttonUpdateIp;
        private System.Windows.Forms.Button buttonTest;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.Button buttonImportData;
        private System.Windows.Forms.Button buttonCalcRanking;
        private System.Windows.Forms.Button buttonPostTwitter;
    }
}

