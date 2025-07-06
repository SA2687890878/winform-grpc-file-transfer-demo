# WinForm gRPC大文件断点续传工具

## 功能特性
- 支持最大100GB单个文件传输
- 可靠的断点续传功能
- 文件分块传输（2MB/块）
- 实时传输进度显示

## 运行环境
- .NET 8.0 或更高版本
- Windows 10/11

## 使用步骤

### 1. 启动服务端
```bash
cd FileTransfer.Server
dotnet run
```

### 2.启动客户端

```
cd FileTransfer.Client.WinForms
dotnet run
```

###	 3.使用客户端

1. 点击"选择文件"按钮选择文件
2. 等待文件哈希计算完成
3. 点击"开始上传"按钮

### 4.技术说明

- 传输协议：gRPC (HTTP/2)
- 分块大小：2MB
- 断点续传：服务端记录已接收的块索引
- 临时文件：服务端使用系统临时目录存储中间文件