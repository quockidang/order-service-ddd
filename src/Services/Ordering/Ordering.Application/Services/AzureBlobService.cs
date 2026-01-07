


using System.Text;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Microsoft.Extensions.Configuration;

namespace Ordering.Application.Services;

public interface ILargeFileService
{
    Task<List<int>> InitOrGetStatusAsync(string blobName);
  //  Task UploadChunkAsync(string blobName, int chunkIndex, Stream content);
  //  Task<string> CommitSessionAsync(string blobName, List<int> allChunkIndices);
}


public class AzureBlobService : ILargeFileService
{
    private readonly BlobContainerClient _containerClient;

    public AzureBlobService(IConfiguration configuration)
    {
        string connectionString = configuration.GetConnectionString("AzureStorage")
                                  ?? throw new ArgumentNullException("AzureStorage connection string missing");
        string containerName = "large-uploads";

        var blobServiceClient = new BlobServiceClient(connectionString);
        _containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        _containerClient.CreateIfNotExists();
    }


    // Helper: Chuyển đổi index số nguyên thành BlockID Base64 (độ dài cố định)
    // Ví dụ: 1 -> "000001" -> Base64String
    private static string GenerateBlockId(int index)
    {
        var rawId = index.ToString("D6"); // Đảm bảo độ dài 6 ký tự: 000001
        return Convert.ToBase64String(Encoding.UTF8.GetBytes(rawId));
    }

            // Helper ngược lại: Base64 -> int
        private static int DecodeBlockId(string base64Id)
        {
            var bytes = Convert.FromBase64String(base64Id);
            var rawId = Encoding.UTF8.GetString(bytes);
            return int.Parse(rawId);
        }

        public async Task<List<int>> InitOrGetStatusAsync(string blobName)
        {
            var blockBlobClient = _containerClient.GetBlockBlobClient(blobName);
            var missingChunks = new List<int>();

            // Nếu blob đã tồn tại (đã hoàn tất), trả về rỗng
            if (await blockBlobClient.ExistsAsync())
            {
                return new List<int>(); // Đã xong hết
            }

            try
            {
                // Lấy danh sách các block đã upload lên Azure nhưng chưa commit (Uncommitted)
                var blockList = await blockBlobClient.GetBlockListAsync(BlockListTypes.Uncommitted);
                var uploadedIndices = blockList.Value.UncommittedBlocks
                                        .Select(b => DecodeBlockId(b.Name))
                                        .ToHashSet();

                // Logic này trả về danh sách ĐÃ upload. 
                // Ở tầng Controller ta sẽ đảo ngược lại để tìm danh sách MISSING nếu cần,
                // hoặc trả về danh sách uploaded để Client tự tính.
                // Để đơn giản cho demo, tôi trả về danh sách index ĐÃ CÓ trên server.
                return uploadedIndices.ToList();
            }
            catch
            {
                // Chưa có block nào
                return new List<int>();
            }
        }
}