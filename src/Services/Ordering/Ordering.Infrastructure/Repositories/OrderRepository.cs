

using Contracts.Domains.Interfaces;
using Infrastructure.Common;
using Microsoft.EntityFrameworkCore;
using Ordering.Application.Common.Interfaces;
using Ordering.Domain.Entities;
using Ordering.Infrastructure.Persistence;

namespace Ordering.Infrastructure.Repositories;

public class OrderRepository : RepositoryBase<Order, Guid, OrderContext>, IOrderRepository
{
    public OrderRepository(
        OrderContext dbContext,
        IUnitOfWork<OrderContext> unitOfWork
    ) : base(dbContext, unitOfWork)
    {
    }

    public async Task<IEnumerable<Order>> GetOrdersByUserNameAsync(string userName)
         => await FindByCondition(o => o.UserName == userName).ToListAsync();
}