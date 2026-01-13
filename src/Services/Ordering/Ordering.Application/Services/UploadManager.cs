
using Microsoft.EntityFrameworkCore;
using Ordering.Application.Common.Interfaces;
using Ordering.Application.Common.Models.Upload;
using Ordering.Domain.Entities;

namespace Ordering.Application.Services;

// Interface nghiệp vụ
public interface IUploadManager
{
    Task<InitSessionResponse> InitializeSessionAsync(InitSessionRequest request);
    Task UploadChunkAsync(Guid sessionId, int chunkIndex, Stream content);
    Task<FinalizeResponse> FinalizeSessionAsync(Guid sessionId);
}


public class UploadManager : IUploadManager
{
    private readonly IUploadSessionRepository _uploadSessionRepository;
    private readonly IBlobStorageService _blobService;

    public UploadManager(IUploadSessionRepository uploadSessionRepository, IBlobStorageService blobService)
    {
        _uploadSessionRepository = uploadSessionRepository;
        _blobService = blobService;
    }

    public async Task<InitSessionResponse> InitializeSessionAsync(InitSessionRequest request)
    {
        // 1. Kiểm tra xem user này đã từng upload file này chưa (Deduplication Logic)
        // Giả định: 1 User không upload cùng 1 file 2 lần trong thời gian ngắn
        var existingSession = await  (_uploadSessionRepository.FindByCondition(x =>
            x.UserName == request.UserName &&
            x.FileName == request.FileName &&
            !x.IsCompleted)).FirstOrDefaultAsync();

        if (existingSession != null)
        {
            // Resume Logic: Hỏi Azure xem đã có những chunk nào rồi

            var uploadedChunks = await _blobService.GetUploadedChunksAsync(existingSession.Id.ToString());

            var allChunks = Enumerable.Range(0, request.TotalChunks);
            var missingChunks = allChunks.Except(uploadedChunks).ToList();

            return new InitSessionResponse
            {
                SessionId = existingSession.Id,
                IsResumed = true,
                MissingChunks = missingChunks
            };
        }

        // New Session Logic
        var newSession = new UploadSession
        {
            Id = Guid.NewGuid(),
            FileName = request.FileName,
            FileSize = request.FileSize,
            TotalChunks = request.TotalChunks,
            UserName = request.UserName
        };

        await _uploadSessionRepository.CreateAsync(newSession);
        return new InitSessionResponse
        {
            SessionId = newSession.Id,
            IsResumed = false,
            MissingChunks = [.. Enumerable.Range(0, request.TotalChunks)]
        };
    }

    public async Task UploadChunkAsync(Guid sessionId, int chunkIndex, Stream content)
    {
        // Validate Session tồn tại (Optional: Caching session này để đỡ query DB)
        var sessionExists = await _uploadSessionRepository.FindByCondition(x => x.Id == sessionId).FirstOrDefaultAsync();
        if (sessionExists == null) throw new KeyNotFoundException("Session not found");
        var blobName = sessionId.ToString() + sessionExists.FileName;
        // Upload thẳng lên Azure
        await _blobService.StageBlockAsync(blobName, chunkIndex, content);
    }

    public async Task<FinalizeResponse> FinalizeSessionAsync(Guid sessionId)
    {
        var session = await _uploadSessionRepository.FindByCondition(x => x.Id == sessionId).FirstOrDefaultAsync();
        if (session == null) throw new KeyNotFoundException("Session not found");

        if (session.IsCompleted)
            return new FinalizeResponse { FileUrl = session.FinalUrl!, CompletedAt = session.CompletedAt.Value, Size = session.FileSize };

        // 1. Trigger Azure Merge (Commit)
        // Đây là bước quan trọng nhất: chuyển Uncommitted Blocks thành Real Blob
        var blobName = sessionId.ToString() + session.FileName;
        var fileUrl = await _blobService.CommitBlocksAsync(blobName, session.TotalChunks);

        // 2. Update DB
        session.IsCompleted = true;
        session.FinalUrl = fileUrl;
        session.CompletedAt = DateTime.UtcNow;

        await _uploadSessionRepository.UpdateAsync(session);

        return new FinalizeResponse
        {
            FileUrl = fileUrl,
            Size = session.FileSize,
            CompletedAt = session.CompletedAt.Value
        };
    }
}