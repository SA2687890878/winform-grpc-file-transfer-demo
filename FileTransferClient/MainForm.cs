using FileTransfer.Client.WinForms.Services;
using FileTransfer.Shared;
using Google.Protobuf;
using Grpc.Net.Client;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;
using FileTransfer.Client.WinForms.Enum;

namespace FileTransfer.Client.WinForms
{
    public partial class MainForm : Form
    {
        private readonly UploadManager _uploadManager;
        private string _filePath = string.Empty;
        private readonly string _serverAddress = "https://localhost:5121";

        public MainForm()
        {
            InitializeComponent();
            _uploadManager = new UploadManager(_serverAddress);
        }

        /// <summary>
        /// 选择文件按钮点击事件处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btnSelect_Click(object sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                _filePath = ofd.FileName;
                txtFilePath.Text = _filePath;
                progressBar.Value = 0; // 新增：重置进度条
            }
        }

        /// <summary>
        /// 上传按钮点击事件处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btnUpload_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(_filePath))
            {
                MessageBox.Show("请先选择文件。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            // 新增：确保每次点击“上传”都恢复上传状态
            _uploadManager.ResumeUpload();

            btnUpload.Enabled = false;
            btnPause.Enabled = true;
            btnResume.Enabled = false;

            try
            {
                var result = await _uploadManager.UploadFileAsync(_filePath, new Progress<double>(p => progressBar.Value = (int)(p * 100)));
                if (result == UploadResult.Success)
                {
                    MessageBox.Show("上传完成！", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else if (result == UploadResult.Cancelled)
                {
                    MessageBox.Show("上传已暂停。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("上传失败。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"上传失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnUpload.Enabled = true;
                btnPause.Enabled = false;
                btnResume.Enabled = true;
            }
        }

        /// <summary>
        /// 暂停上传按钮点击事件处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnPause_Click(object sender, EventArgs e)
        {
            _uploadManager?.PauseUpload();
            btnUpload.Enabled = false;
            btnPause.Enabled = false;
            btnResume.Enabled = true;
        }

        /// <summary>
        /// 恢复上传按钮点击事件处理
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void btnResume_Click(object sender, EventArgs e)
        {
            if (_uploadManager == null || string.IsNullOrWhiteSpace(_filePath)) return;

            _uploadManager.ResumeUpload();

            btnUpload.Enabled = false;
            btnPause.Enabled = true;
            btnResume.Enabled = false;

            try
            {
                var result = await _uploadManager.UploadFileAsync(_filePath, new Progress<double>(p => progressBar.Value = (int)(p * 100)));
                if (result == UploadResult.Success)
                {
                    MessageBox.Show("上传完成！", "成功", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else if (result == UploadResult.Cancelled)
                {
                    MessageBox.Show("上传已暂停。", "提示", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("上传失败。", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"恢复上传失败: {ex.Message}", "错误", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnUpload.Enabled = true;
                btnPause.Enabled = false;
                btnResume.Enabled = true;
            }
        }
    }
}
