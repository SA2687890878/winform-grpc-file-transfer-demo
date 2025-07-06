using FileTransfer.Client.WinForms.Services;
using FileTransfer.Shared;
using Google.Protobuf;
using Grpc.Net.Client;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FileTransfer.Client.WinForms
{
    public partial class MainForm : Form
    {
        private readonly UploadManager _uploadManager;

        public MainForm()
        {
            InitializeComponent();
            _uploadManager = new UploadManager("https://localhost:5121");
        }

        private async void btnSelect_Click(object sender, EventArgs e)
        {
            using var ofd = new OpenFileDialog();
            if (ofd.ShowDialog() == DialogResult.OK)
            {
                txtFilePath.Text = ofd.FileName;
            }
        }

        private async void btnUpload_Click(object sender, EventArgs e)
        {
            var filePath = txtFilePath.Text;
            if (!File.Exists(filePath))
            {
                MessageBox.Show("文件不存在。");
                return;
            }

            progressBar.Value = 0;
            btnUpload.Enabled = false;

            var progress = new Progress<double>(v => progressBar.Value = (int)(v * 100));

            try
            {
                await _uploadManager.UploadFileAsync(filePath, progress);
                MessageBox.Show("上传完成");
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                MessageBox.Show("上传失败: " + ex.Message);
            }
            finally
            {
                btnUpload.Enabled = true;
            }
        }
    }
}
