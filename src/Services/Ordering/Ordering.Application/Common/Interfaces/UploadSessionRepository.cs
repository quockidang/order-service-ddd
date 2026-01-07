
using Contracts.Domains.Interfaces;
using Ordering.Domain.Entities;

namespace Ordering.Application.Common.Interfaces;


public interface IUploadSessionRepository : IRepositoryBase<UploadSession, Guid>
{
    
}