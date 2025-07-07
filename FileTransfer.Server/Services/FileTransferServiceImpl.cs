using FileTransfer.Shared;
using Grpc.Core;
using System.Security.Cryptography;

namespace FileTransfer.Server.Services
{
    public class FileTransferServiceImpl : FileTransferService.FileTransferServiceBase
    {
        private readonly string _uploadPath = Path.Combine(System.AppContext.BaseDirectory, "UploadedFiles");
        private static readonly object _lock = new();

        public FileTransferServiceImpl()
        {
            Directory.CreateDirectory(_uploadPath);
        }

        /// <summary>
        /// 上传
        /// </summary>
        /// <param name="requestStream"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override async Task<UploadStatus> Upload(IAsyncStreamReader<FileChunk> requestStream,
            ServerCallContext context)
        {
            FileStream? fs = null;
            try
            {
                await foreach (var chunk in requestStream.ReadAllAsync())
                {
                    var filePath = Path.Combine(_uploadPath, chunk.FileName);
                    lock (_lock)
                    {
                        if (fs == null)
                        {
                            fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
                        }

                        fs.Seek(chunk.Offset, SeekOrigin.Begin);
                        fs.Write(chunk.Data.ToByteArray(), 0, chunk.Data.Length);
                        fs.Flush();
                    }
                }

                return new UploadStatus { Success = true, Message = "Upload completed." };
            }
            catch (Exception ex)
            {
                return new UploadStatus { Success = false, Message = ex.Message };
            }
            finally
            {
                fs?.Dispose();
            }
        }


        public override Task<FileCheckResponse> CheckFileExists(FileCheckRequest request, ServerCallContext context)
        {
            var path = Path.Combine(_uploadPath, request.FileName);
            bool exists = File.Exists(path);
            if (exists)
            {
                var existingMd5 = CalculateFileMd5(path);
                return Task.FromResult(new FileCheckResponse { Exists = existingMd5 == request.Md5 });
            }

            return Task.FromResult(new FileCheckResponse { Exists = false });
        }

        /// <summary>
        /// 获取文件偏移的接口
        /// </summary>
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task<FileOffsetResponse> GetUploadedOffset(FileQueryRequest request, ServerCallContext context)
        {
            var path = Path.Combine(_uploadPath, request.FileName);
            long offset = File.Exists(path) ? new FileInfo(path).Length : 0;
            return Task.FromResult(new FileOffsetResponse { Offset = offset });
        }

        #region 私有方法

        private static string CalculateFileMd5(string path)
        {
            using var md5 = MD5.Create();
            using var stream = File.OpenRead(path);
            return BitConverter.ToString(md5.ComputeHash(stream)).Replace("-", "").ToLowerInvariant();
        }

        #endregion
    }
}