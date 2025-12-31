

using System.ComponentModel.DataAnnotations;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Ordering.Application.Common.Models;
using Ordering.Application.Features.V1.Orders;
using Shared.DTOs.Order;

namespace Ordering.API.Controllers;


[Route("api/v1/[controller]")]
[ApiController]
public class OrdersController: ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;


    public OrdersController(IMediator mediator, IMapper mapper)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    private static class RouteNames
    {
        public const string GetOrders = "GetOrders";
        public const string CreateOrder = nameof(CreateOrder);

    }

    [HttpGet("{userName}", Name = RouteNames.GetOrders)]
    [ProducesResponseType(typeof(IEnumerable<OrderDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrdersByUserName([Required] string userName)
    {
        // test build 1 1 q1 1
        var query = new GetOrdersByUserNameQuery(userName);
        var result = await _mediator.Send(query);
        return Ok(result);
    }

    [HttpPost(Name = RouteNames.CreateOrder)]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderDto model)
    {
        var command = _mapper.Map<CreateOrderCommand>(model);
        var result = await _mediator.Send(command);
        return Ok(result);
    }
}
