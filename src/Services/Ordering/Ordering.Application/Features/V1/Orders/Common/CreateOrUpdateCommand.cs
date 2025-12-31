using AutoMapper;
using Ordering.Application.Common.Mappings;
using Ordering.Domain.Entities;

namespace Ordering.Application.Features.V1.Orders;

public class CreateOrUpdateCommand : IMapFrom<Order>
{
    public decimal TotalPrice { get; set; }

    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string EmailAddress { get; set; }

    public required string ShippingAddress { get; set; }

    private  string? _invoiceAddress;
    public required string? InvoiceAddress
    {
        get => _invoiceAddress;
        set => _invoiceAddress = value ?? ShippingAddress;
    }
    
    public void Mapping(Profile profile)
    {
        profile.CreateMap<CreateOrUpdateCommand, Order>();
    }
}