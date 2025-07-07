namespace FileTransfer.Client.WinForms
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Button btnSelect;
        private System.Windows.Forms.TextBox txtFilePath;
        /// <summary>
        /// 上传按钮
        /// </summary>
        private System.Windows.Forms.Button btnUpload;
        /// <summary>
        /// 暂停按钮
        /// </summary>
        private System.Windows.Forms.Button btnPause;
        /// <summary>
        /// 恢复按钮
        /// </summary>
        private System.Windows.Forms.Button btnResume;
        private System.Windows.Forms.ProgressBar progressBar;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.btnSelect = new System.Windows.Forms.Button();
            this.txtFilePath = new System.Windows.Forms.TextBox();
            this.btnUpload = new System.Windows.Forms.Button();
            this.btnPause = new System.Windows.Forms.Button();
            this.btnResume = new System.Windows.Forms.Button();
            this.progressBar = new System.Windows.Forms.ProgressBar();
            this.SuspendLayout();

            // btnSelect
            this.btnSelect.Location = new System.Drawing.Point(400, 20);
            this.btnSelect.Name = "btnSelect";
            this.btnSelect.Size = new System.Drawing.Size(75, 23);
            this.btnSelect.TabIndex = 0;
            this.btnSelect.Text = "选择文件";
            this.btnSelect.UseVisualStyleBackColor = true;
            this.btnSelect.Click += new System.EventHandler(this.btnSelect_Click);

            // txtFilePath
            this.txtFilePath.Location = new System.Drawing.Point(20, 20);
            this.txtFilePath.Name = "txtFilePath";
            this.txtFilePath.Size = new System.Drawing.Size(370, 21);
            this.txtFilePath.TabIndex = 1;

            // btnUpload
            this.btnUpload.Location = new System.Drawing.Point(20, 60);
            this.btnUpload.Name = "btnUpload";
            this.btnUpload.Size = new System.Drawing.Size(120, 30);
            this.btnUpload.TabIndex = 2;
            this.btnUpload.Text = "开始上传";
            this.btnUpload.UseVisualStyleBackColor = true;
            this.btnUpload.Click += new System.EventHandler(this.btnUpload_Click);

            // 
            // btnPause
            // 
            this.btnPause.Enabled = false;
            this.btnPause.Location = new System.Drawing.Point(160, 60);
            this.btnPause.Name = "btnPause";
            this.btnPause.Size = new System.Drawing.Size(120, 30);
            this.btnPause.TabIndex = 2;
            this.btnPause.Text = "暂停上传";
            this.btnPause.UseVisualStyleBackColor = true;
            this.btnPause.Click += new System.EventHandler(this.btnPause_Click);

            // 
            // btnResume
            // 
            this.btnResume.Enabled = false;
            this.btnResume.Location = new System.Drawing.Point(300, 60);
            this.btnResume.Name = "btnResume";
            this.btnResume.Size = new System.Drawing.Size(120, 30);
            this.btnResume.TabIndex = 3;
            this.btnResume.Text = "恢复上传";
            this.btnResume.UseVisualStyleBackColor = true;
            this.btnResume.Click += new System.EventHandler(this.btnResume_Click);

            // progressBar
            this.progressBar.Location = new System.Drawing.Point(20, 110);
            this.progressBar.Name = "progressBar";
            this.progressBar.Size = new System.Drawing.Size(455, 23);
            this.progressBar.TabIndex = 3;

            // MainForm
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(500, 160);
            this.Controls.Add(this.progressBar);
            this.Controls.Add(this.btnUpload);
            this.Controls.Add(this.txtFilePath);
            this.Controls.Add(this.btnSelect);
            this.Controls.Add(this.btnPause);
            this.Controls.Add(this.btnResume);
            this.Name = "MainForm";
            this.Text = "文件上传工具";
            this.ResumeLayout(false);
            this.PerformLayout();
        }
    }
}
