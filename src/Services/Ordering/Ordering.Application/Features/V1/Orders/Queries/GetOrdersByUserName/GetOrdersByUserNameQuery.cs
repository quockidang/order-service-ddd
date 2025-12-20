using MediatR;
using Ordering.Application.Common.Models;
using Shared.SeedWork;

namespace Ordering.Application.Features.V1.Orders;

public class GetOrdersByUserNameQuery(string userName) : IRequest<ApiResult<List<OrderDto>>>
{
    public string UserName { get; set; } = userName ?? throw new ArgumentNullException(nameof(userName));
}