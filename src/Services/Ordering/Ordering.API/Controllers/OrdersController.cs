

using System.ComponentModel.DataAnnotations;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Ordering.Application.Common.Models;
using Ordering.Application.Features.V1.Orders;

namespace Ordering.API.Controllers;


[Route("api/v1/[controller]")]
[ApiController]
public class OrdersController: ControllerBase
{
    private readonly IMediator _mediator;

    public OrdersController(IMediator mediator)
    {
        _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
    }

    private static class RouteName
    {
        public const string GetOrders = "GetOrders";
    }

    [HttpGet("{userName}", Name = RouteName.GetOrders)]
    [ProducesResponseType(typeof(IEnumerable<OrderDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetOrdersByUserName([Required] string userName)
    {
        // test build 1 1
        var query = new GetOrdersByUserNameQuery(userName);
        var result = await _mediator.Send(query);
        return Ok(result);
    }
}
