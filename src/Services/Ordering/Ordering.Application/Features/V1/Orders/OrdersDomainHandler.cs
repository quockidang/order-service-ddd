
using Serilog;

namespace Ordering.Application.Features.V1.Orders;

public class OrdersDomainHandler 
   
{
    private readonly ILogger _logger;
    public OrdersDomainHandler(ILogger logger)
    {
        _logger = logger;
    }
}