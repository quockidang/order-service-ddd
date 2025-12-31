using Ordering.Application.Common.Mappings;
using Ordering.Application.Features.V1.Orders;
using Ordering.Domain.Entities;
using Shared.Enums.Order;

namespace Ordering.Application.Common.Models;

public class OrderDto : IMapFrom<Order>
{
    public Guid Id { get; set; }
    public required string DocumentNo { get; set; }
    public required string UserName { get; set; }
    public decimal TotalPrice { get; set; }

    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string EmailAddress { get; set; }

    //Address
    public required string ShippingAddress { get; set; }
    public required string InvoiceAddress { get; set; }

    public EOrderStatus Status { get; set; }
}
