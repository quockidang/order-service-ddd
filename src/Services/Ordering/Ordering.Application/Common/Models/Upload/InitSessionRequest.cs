using Ordering.Domain.Entities;
using Shared.Enums.Order;

namespace Ordering.Application.Common.Models.Upload;

    public class InitSessionRequest
    {
        public required string FileName { get; set; }
        public long FileSize { get; set; }
        public int TotalChunks { get; set; }
        public required string UserName { get; set; } // Giả lập, thực tế lấy từ JWT Token
    }

    public class InitSessionResponse
    {
        public Guid SessionId { get; set; }
        public bool IsResumed { get; set; }
        public List<int> MissingChunks { get; set; } = new List<int>();
    }

    public class FinalizeRequest
    {
        public Guid SessionId { get; set; }
        public string? FileHash { get; set; } // Optional: Checksum toàn bộ file
    }

    public class FinalizeResponse
    {
        public required string FileUrl { get; set; }
        public long Size { get; set; }
        public DateTime CompletedAt { get; set; }
    }