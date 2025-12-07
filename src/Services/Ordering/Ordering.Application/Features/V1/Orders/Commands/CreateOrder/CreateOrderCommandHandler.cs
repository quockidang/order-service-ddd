using AutoMapper;
using MediatR;
using Ordering.Application.Common.Interfaces;
using Ordering.Domain.Entities;
using Serilog;
using Shared.SeedWork;

namespace Ordering.Application.Features.V1.Orders;

public class CreateOrderCommandHandler : IRequestHandler<CreateOrderCommand, ApiResult<long>>
{
   // private readonly IOrderRepository _orderRepository;
   // private readonly IMapper _mapper;
    private readonly ILogger _logger;

    public CreateOrderCommandHandler(
       // IMapper mapper,
        ILogger logger)
    {
        //_orderRepository = orderRepository ?? throw new ArgumentNullException(nameof(orderRepository));
       // _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    private const string MethodName = "CreateOrderCommandHandler";

    public async Task<ApiResult<long>> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
       throw new NotImplementedException();
    }
}