

using System.ComponentModel.DataAnnotations;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Ordering.Application.Common.Models;
using Ordering.Application.Common.Models.Upload;
using Ordering.Application.Features.V1.Orders;
using Ordering.Application.Services;
using Shared.DTOs.Order;

namespace Ordering.API.Controllers;


[Route("api/v1/[controller]")]
[ApiController]
public class UploadsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;

    private readonly ILogger<UploadsController> _logger;
    private readonly IUploadManager _uploadManager;


    public UploadsController(IMediator mediator, IMapper mapper, IUploadManager uploadManager, ILogger<UploadsController> logger)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _uploadManager = uploadManager ?? throw new ArgumentNullException(nameof(uploadManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    private static class RouteNames
    {
        public const string InitSession = "InitSession";

    }


    [HttpPost("init")]
        public async Task<ActionResult<InitSessionResponse>> InitSession([FromBody] InitSessionRequest request)
        {
            try
            {
                var result = await _uploadManager.InitializeSessionAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing session");
                return StatusCode(500, "Internal Server Error");
            }
        }

        [HttpPost("{sessionId:guid}/chunk")]
        [DisableRequestSizeLimit] // Quan trọng cho file lớn
        [RequestFormLimits(MultipartBodyLengthLimit = 104_857_600)] // Limit 100MB per chunk
        public async Task<IActionResult> UploadChunk(Guid sessionId, [FromForm] int index, IFormFile file)
        {
            if (file == null || file.Length == 0) return BadRequest("Empty file");

            try
            {
                using var stream = file.OpenReadStream();
                await _uploadManager.UploadChunkAsync(sessionId, index, stream);
                return Ok(new { Message = "Chunk uploaded", Index = index });
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Session not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error uploading chunk {index} for session {sessionId}");
                return StatusCode(500, "Upload failed");
            }
        }

        [HttpPost("{sessionId:guid}/finalize")]
        public async Task<ActionResult<FinalizeResponse>> Finalize(Guid sessionId)
        {
            try
            {
                var result = await _uploadManager.FinalizeSessionAsync(sessionId);
                return Ok(result);
            }
            catch (Azure.RequestFailedException ex)
            {
                // Thường xảy ra nếu client gọi finalize nhưng chưa upload đủ chunk
                _logger.LogError(ex, "Azure Commit Failed");
                return BadRequest("Merge failed. Check if all chunks are uploaded.");
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Session not found");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Finalize error");
                return StatusCode(500, "Finalize failed");
            }
        }

}