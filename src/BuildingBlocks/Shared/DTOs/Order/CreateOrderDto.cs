namespace Shared.DTOs.Order;

public class CreateOrderDto
{
    public required string UserName { get; set; }
    public decimal TotalPrice { get; set; }

    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public required string EmailAddress { get; set; }

    public required string ShippingAddress { get; set; }

    private string? _invoiceAddress;
    public string? InvoiceAddress
    {
        get => _invoiceAddress;
        set => _invoiceAddress = value ?? ShippingAddress;
    }
}