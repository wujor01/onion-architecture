using BHX.Sale.Domain.Core.Models;
using BHX.Sale.Domain.Core.Repositories;

namespace BHX.Sale.Infrastructure.Repositories
{
    public class BaseRepositoryAsync<T> : IBaseRepositoryAsync<T> where T : BaseEntity
    {
    }
}
