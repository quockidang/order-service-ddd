

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
public class UploadController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;

    private readonly ILargeFileService _fileService;


    public UploadController(IMediator mediator, IMapper mapper, ILargeFileService fileService)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
    }

    private static class RouteNames
    {
        public const string GetOrders = "GetOrders";
        public const string CreateOrder = nameof(CreateOrder);

    }


    [HttpPost("init")]
    public async Task<IActionResult> InitSession([FromBody] InitSessionRequest request)
    {
        // Sử dụng tên file làm SessionID (hoặc kết hợp với UserID để unique)
        // Ví dụ: user123/video_holiday.mp4
        var sessionId = Guid.NewGuid();

        var uploadedChunks = await _fileService.InitOrGetStatusAsync(sessionId.ToString());

        // Tính toán missing chunks
        var allChunks = Enumerable.Range(0, request.TotalChunks).ToList();
        var missingChunks = allChunks.Except(uploadedChunks).ToList();

        return Ok(new InitSessionResponse
        {
            SessionId = sessionId,
            MissingChunks = missingChunks
        });
    }

}