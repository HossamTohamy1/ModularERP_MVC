using ModularERP.Common.Models;
using System.Linq.Expressions;

namespace ModulerERP_MVC_.Common.Repositories.Interfaces
{
    public interface IGeneralRepository<T> where T : BaseEntity
    {
        Task AddAsync(T entity);
        IQueryable<T> GetAll();
        Task<T?> GetByID(Guid id);
        Task<T?> GetByIDWithTracking(Guid id);
        IQueryable<T> Get(Expression<Func<T, bool>> expression);
        IQueryable<T> GetByCompanyId(Guid companyId);
        Task Update(T entity);
        void UpdateInclude(T entity, params string[] modifiedParams);
        Task Delete(Guid id);
        Task SaveChanges();
        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);
        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);
        Task AddRangeAsync(IEnumerable<T> entities);
    }
}