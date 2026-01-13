


using System.Text;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Microsoft.Extensions.Configuration;

namespace Ordering.Application.Services;

public interface IBlobStorageService
{
    Task StageBlockAsync(string blobName, int chunkIndex, Stream content);
    Task<List<int>> GetUploadedChunksAsync(string blobName);
    Task<string> CommitBlocksAsync(string blobName, int totalChunks);
    string GetBlobUrl(string blobName);
}


public class AzureBlobService : IBlobStorageService
{
    private readonly BlobContainerClient _containerClient;

    private const int BLOCK_ID_LENGTH = 6; // Đủ cho 999,999 chunks

    public AzureBlobService(IConfiguration configuration)
    {
        string connectionString = configuration.GetConnectionString("AzureStorage")
                                  ?? throw new ArgumentNullException("AzureStorage connection string missing");
        string containerName = "large-uploads";

        var blobServiceClient = new BlobServiceClient(connectionString);
        _containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        _containerClient.CreateIfNotExists();
    }

    // Helper: 1 -> "000001" -> Base64
    private string IntToBase64BlockId(int index)
    {
        var rawId = index.ToString($"D{BLOCK_ID_LENGTH}");
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(rawId));
    }

    // Helper: Base64 -> "000001" -> 1
    private int Base64ToIntBlockId(string base64)
    {
        var bytes = Convert.FromBase64String(base64);
        var rawId = Encoding.UTF8.GetString(bytes);
        return int.Parse(rawId);
    }

    public async Task StageBlockAsync(string blobName, int chunkIndex, Stream content)
    {
        var blobClient = _containerClient.GetBlockBlobClient(blobName);
        var blockId = IntToBase64BlockId(chunkIndex);

        // Stage block lên Azure (chưa tạo thành file, chỉ nằm ở staging area)
        await blobClient.StageBlockAsync(blockId, content);
    }


    public async Task<List<int>> GetUploadedChunksAsync(string blobName)
    {
        var blobClient = _containerClient.GetBlockBlobClient(blobName);
        if (!await blobClient.ExistsAsync()) return new List<int>();

        try
        {
            // Lấy danh sách các block đang chờ commit (Uncommitted)
            var blockList = await blobClient.GetBlockListAsync(BlockListTypes.Uncommitted);

            return blockList.Value.UncommittedBlocks
                .Select(b => Base64ToIntBlockId(b.Name))
                .ToList();
        }
        catch
        {
            return new List<int>();
        }
    }


    public async Task<string> CommitBlocksAsync(string blobName, int totalChunks)
    {
        var blobClient = _containerClient.GetBlockBlobClient(blobName);

        // Tạo danh sách BlockID tuần tự từ 0 -> Total-1
        // Azure sẽ ghép file theo thứ tự của List này
        var blockIds = Enumerable.Range(0, totalChunks)
                                 .Select(IntToBase64BlockId)
                                 .ToList();

        await blobClient.CommitBlockListAsync(blockIds);
        return blobClient.Uri.ToString();
    }

    public string GetBlobUrl(string blobName) =>
        $"{_containerClient.Uri}/{blobName}";
}