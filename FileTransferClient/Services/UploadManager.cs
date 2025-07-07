using FileTransfer.Shared;
using Google.Protobuf;
using Grpc.Net.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using FileTransfer.Client.WinForms.Enum;

namespace FileTransfer.Client.WinForms.Services
{
    public class UploadManager
    {
        private readonly string _serverAddress;
        private CancellationTokenSource? _cts;
        private bool _paused = false;
        private readonly int _chunkSize = 2 * 1024 * 1024; // 每块2MB
        private readonly int _concurrency = 4;
        private const int MaxRetries = 3;//重试次数

        public UploadManager(string serverAddress)
        {
            _serverAddress = serverAddress;
        }

        /// <summary>
        /// 暂停上传
        /// </summary>
        public void PauseUpload()
        {
            _paused = true;
            _cts?.Cancel();
        }

        /// <summary>
        /// 恢复上传
        /// </summary>
        public void ResumeUpload()
        {
            _paused = false;
        }

        public async Task<UploadResult> UploadFileAsync(string filePath, IProgress<double>? progress = null)
        {
            if (_paused) return UploadResult.Cancelled;

            _cts = new CancellationTokenSource();
            var cancellationToken = _cts.Token;

            string fileName = Path.GetFileName(filePath);
            string fileMd5 = CalculateFileMd5(filePath);

            using var channel = GrpcChannel.ForAddress(_serverAddress);
            var client = new FileTransferService.FileTransferServiceClient(channel);

            try
            {
                // 秒传检测
                var checkResp = await client.CheckFileExistsAsync(new FileCheckRequest
                {
                    FileName = fileName,
                    Md5 = fileMd5
                }, cancellationToken: cancellationToken);

                if (checkResp.Exists)
                {
                    progress?.Report(1.0);
                    return UploadResult.Success; // 秒传，文件已上传
                }

                // 断点续传获取已上传偏移
                var offsetResp = await client.GetUploadedOffsetAsync(new FileQueryRequest { FileName = fileName }, cancellationToken: cancellationToken);
                long uploadedOffset = offsetResp.Offset;

                using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);

                var totalSize = fs.Length;
                if (uploadedOffset > totalSize) uploadedOffset = 0;

                fs.Seek(uploadedOffset, SeekOrigin.Begin);

                var call = client.Upload(cancellationToken: cancellationToken);

                byte[] buffer = new byte[_chunkSize];
                int bytesRead;
                long totalSent = uploadedOffset;

                while ((bytesRead = await fs.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) > 0)
                {
                    if (_paused)
                    {
                        await call.RequestStream.CompleteAsync();
                        return UploadResult.Cancelled;
                    }

                    int retryCount = 0;
                    bool success = false;
                    while (retryCount < MaxRetries && !success)
                    {
                        try
                        {
                            await call.RequestStream.WriteAsync(new FileChunk
                            {
                                FileName = fileName,
                                Offset = totalSent,
                                Data = ByteString.CopyFrom(buffer, 0, bytesRead),
                                IsLast = totalSent + bytesRead == totalSize
                            });

                            success = true;
                        }
                        catch (Exception ex)
                        {
                            retryCount++;
                            if (retryCount == MaxRetries) throw;
                            await Task.Delay(500); // 重试延迟
                        }
                    }

                    totalSent += bytesRead;
                    progress?.Report((double)totalSent / totalSize);
                }

                await call.RequestStream.CompleteAsync();
                var response = await call.ResponseAsync;

                if (!response.Success)
                {
                    throw new Exception(response.Message);
                }
                return UploadResult.Success;
            }
            catch (Grpc.Core.RpcException ex) when (ex.StatusCode == Grpc.Core.StatusCode.Cancelled)
            {
                return UploadResult.Cancelled;
            }
            catch (OperationCanceledException)
            {
                return UploadResult.Cancelled;
            }
            catch
            {
                return UploadResult.Failed;
            }
        }

        #region 私有方法

        private string CalculateFileMd5(string filePath)
        {
            using var md5 = MD5.Create();
            using var stream = File.OpenRead(filePath);
            return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLowerInvariant();
        }

        private List<(long Offset, int Size)> GetChunks(long startOffset, long fileSize)
        {
            var list = new List<(long, int)>();
            for (long offset = startOffset; offset < fileSize; offset += _chunkSize)
            {
                int size = (int)Math.Min(_chunkSize, fileSize - offset);
                list.Add((offset, size));
            }
            return list;
        }

        #endregion
    }

}
