


using Contracts.Domains.Interfaces;
using Infrastructure.Common;
using Microsoft.EntityFrameworkCore;
using Ordering.Application.Common.Interfaces;
using Ordering.Domain.Entities;
using Ordering.Infrastructure.Persistence;

namespace Ordering.Infrastructure.Repositories;

public class UploadSessionRepository : RepositoryBase<UploadSession, Guid, OrderContext>, IUploadSessionRepository
{
    public UploadSessionRepository(
        OrderContext dbContext,
        IUnitOfWork<OrderContext> unitOfWork
    ) : base(dbContext, unitOfWork)
    {
    }

}