
using Ordering.Application.Common.Models.Upload;

namespace Ordering.Application.Services;

    // Interface nghiệp vụ
    public interface IUploadManager
    {
        Task<InitSessionResponse> InitializeSessionAsync(InitSessionRequest request);
        Task UploadChunkAsync(Guid sessionId, int chunkIndex, Stream content);
        Task<FinalizeResponse> FinalizeSessionAsync(Guid sessionId);
    }


public class UploadManager 
    {
        
    }