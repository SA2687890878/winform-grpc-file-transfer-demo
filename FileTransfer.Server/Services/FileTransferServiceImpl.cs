using FileTransfer.Shared;
using Grpc.Core;

namespace FileTransfer.Server.Services
{
    public class FileTransferServiceImpl : FileTransferService.FileTransferServiceBase
    {
        private readonly string _uploadPath = Path.Combine(System.AppContext.BaseDirectory, "UploadedFiles");

        /// <summary>
        /// 上传
        /// </summary>
        /// <param name="requestStream"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override async Task<UploadStatus> Upload(IAsyncStreamReader<FileChunk> requestStream,
            ServerCallContext context)
        {
            Directory.CreateDirectory(_uploadPath);
            string? filePath = null;
            FileStream? fs = null;
            try
            {
                await foreach (var chunk in requestStream.ReadAllAsync())
                {
                    if (filePath == null)
                    {
                        filePath = Path.Combine(_uploadPath, chunk.FileName);
                        fs = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None);
                    }

                    fs.Seek(chunk.Offset, SeekOrigin.Begin);
                    await fs.WriteAsync(chunk.Data.ToByteArray());
                    if (chunk.IsLast) break;
                }

                fs?.Close();
                return new UploadStatus { Success = true, Message = "上传成功" };
            }
            catch (System.Exception ex)
            {
                fs?.Close();
                return new UploadStatus { Success = false, Message = $"上传失败: {ex.Message}" };
            }
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

    }
}