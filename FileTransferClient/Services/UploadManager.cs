using FileTransfer.Shared;
using Google.Protobuf;
using Grpc.Net.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileTransfer.Client.WinForms.Services
{
    public class UploadManager
    {
        private readonly string _serverAddress;
        private readonly int _chunkSize = 2 * 1024 * 1024; // 每块2MB
        private readonly int _concurrency = 4;

        public UploadManager(string serverAddress)
        {
            _serverAddress = serverAddress;
        }

        public async Task UploadFileAsync(string filePath, IProgress<double>? progress = null, CancellationToken cancellationToken = default)
        {
            var fileName = Path.GetFileName(filePath);
            var fileSize = new FileInfo(filePath).Length;

            using var channel = GrpcChannel.ForAddress(_serverAddress);
            var client = new FileTransferService.FileTransferServiceClient(channel);

            // 获取已上传偏移量（用于断点续传）
            var offsetResponse = await client.GetUploadedOffsetAsync(new FileQueryRequest { FileName = fileName }, cancellationToken: cancellationToken);
            long uploadedOffset = offsetResponse.Offset;

            var remainingSize = fileSize - uploadedOffset;
            var totalChunks = (int)Math.Ceiling(remainingSize / (double)_chunkSize);

            var tasks = new List<Task>();
            var completedChunks = 0;
            var progressLock = new object();

            var chunksToUpload = GetChunks(uploadedOffset, fileSize);
            var throttler = new SemaphoreSlim(_concurrency);

            foreach (var (offset, size) in chunksToUpload)
            {
                await throttler.WaitAsync(cancellationToken);

                tasks.Add(Task.Run(async () =>
                {
                    try
                    {
                        byte[] buffer = new byte[size];
                        using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                        {
                            fs.Seek(offset, SeekOrigin.Begin);
                            await fs.ReadAsync(buffer, 0, size, cancellationToken);
                        }

                        using var chunkChannel = GrpcChannel.ForAddress(_serverAddress);
                        var chunkClient = new FileTransferService.FileTransferServiceClient(chunkChannel);
                        using var call = chunkClient.Upload(cancellationToken: cancellationToken);

                        await call.RequestStream.WriteAsync(new FileChunk
                        {
                            FileName = fileName,
                            Offset = offset,
                            Data = ByteString.CopyFrom(buffer),
                            IsLast = false
                        });
                        await call.RequestStream.CompleteAsync();

                        lock (progressLock)
                        {
                            completedChunks++;
                            progress?.Report((double)completedChunks / totalChunks);
                        }
                    }
                    finally
                    {
                        throttler.Release();
                    }
                }, cancellationToken));
            }

            await Task.WhenAll(tasks);
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
    }
}
