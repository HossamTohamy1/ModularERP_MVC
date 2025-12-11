using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using ModularERP.Common.Models;
using ModulerERP_MVC_.Data;
using System.Linq.Expressions;

namespace ModulerERP_MVC_.Common.Repositories.Interfaces
{
    public class GeneralRepository<T> : IGeneralRepository<T> where T : BaseEntity
    {
        private readonly ModulesDbContext _context;
        private readonly DbSet<T> _dbSet;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string? _tenantId;

        public GeneralRepository(ModulesDbContext context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _dbSet = _context.Set<T>();
            _httpContextAccessor = httpContextAccessor;
            _tenantId = GetTenantId();
        }

        private string? GetTenantId()
        {
            var httpContext = _httpContextAccessor.HttpContext;

            if (httpContext?.Request.Headers.TryGetValue("X-Tenant-ID", out var tenantHeader) == true)
            {
                return tenantHeader.FirstOrDefault();
            }

            var tenantClaim = httpContext?.User?.FindFirst("tenant_id")?.Value;
            if (!string.IsNullOrEmpty(tenantClaim))
            {
                return tenantClaim;
            }

            var host = httpContext?.Request.Host.Host;
            if (!string.IsNullOrEmpty(host) && host.Contains('.'))
            {
                var subdomain = host.Split('.')[0];
                if (subdomain != "www" && subdomain != "api")
                {
                    return subdomain;
                }
            }

            return null;
        }

        public async Task AddAsync(T entity)
        {
            if (typeof(T).GetProperty("TenantId") != null && !string.IsNullOrEmpty(_tenantId))
            {
                if (Guid.TryParse(_tenantId, out var tenantId))
                {
                    var currentTenantId = (Guid?)entity.GetType().GetProperty("TenantId")?.GetValue(entity);
                    if (currentTenantId == Guid.Empty || currentTenantId == null)
                    {
                        entity.GetType().GetProperty("TenantId")?.SetValue(entity, tenantId);
                    }
                }
            }

            await _dbSet.AddAsync(entity);
        }

        public IQueryable<T> GetAll()
        {
            return _dbSet.Where(x => !x.IsDeleted);
        }

        public async Task<T?> GetByID(Guid id)
        {
            return await _dbSet
                .Where(c => c.Id == id && !c.IsDeleted)
                .FirstOrDefaultAsync();
        }

        public async Task<T?> GetByIDWithTracking(Guid id)
        {
            return await _dbSet
                .AsTracking()
                .Where(c => c.Id == id && !c.IsDeleted)
                .FirstOrDefaultAsync();
        }

        public IQueryable<T> Get(Expression<Func<T, bool>> expression)
        {
            return GetAll().Where(expression);
        }

        public IQueryable<T> GetByCompanyId(Guid companyId)
        {
            var query = GetAll();

            if (typeof(T).GetProperty("CompanyId") != null)
            {
                query = query.Where(e => EF.Property<Guid>(e, "CompanyId") == companyId);
            }

            return query;
        }

        public async Task Update(T entity)
        {
            var existingEntity = await GetByIDWithTracking(entity.Id);
            if (existingEntity == null)
            {
                throw new KeyNotFoundException($"Entity with ID {entity.Id} not found");
            }

            _context.Entry(existingEntity).CurrentValues.SetValues(entity);
            await _context.SaveChangesAsync();
        }

        public void UpdateInclude(T entity, params string[] modifiedParams)
        {
            if (!_dbSet.Any(x => x.Id.Equals(entity.Id)))
                return;

            var local = _dbSet.Local.FirstOrDefault(x => x.Id.Equals(entity.Id));
            EntityEntry entityEntry = local == null
                ? _context.Entry(entity)
                : _context.ChangeTracker.Entries<T>().FirstOrDefault(x => x.Entity.Id.Equals(entity.Id));

            if (entityEntry != null)
            {
                foreach (var prop in entityEntry.Properties)
                {
                    if (modifiedParams.Contains(prop.Metadata.Name))
                    {
                        prop.CurrentValue = entity.GetType().GetProperty(prop.Metadata.Name)?.GetValue(entity);
                        prop.IsModified = true;
                    }
                }
                _context.SaveChanges();
            }
        }

        public async Task Delete(Guid id)
        {
            var entity = await GetByIDWithTracking(id);
            if (entity != null)
            {
                entity.IsDeleted = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task SaveChanges()
        {
            await _context.SaveChangesAsync();
        }

        public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate)
        {
            return await _dbSet.Where(predicate).AnyAsync();
        }
        public async Task<bool> AnyAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await _dbSet.Where(predicate).AnyAsync(cancellationToken);
        }

        public async Task AddRangeAsync(IEnumerable<T> entities)
        {
            foreach (var entity in entities)
            {
                if (typeof(T).GetProperty("TenantId") != null && !string.IsNullOrEmpty(_tenantId))
                {
                    if (Guid.TryParse(_tenantId, out var tenantId))
                    {
                        var currentTenantId = (Guid?)entity.GetType().GetProperty("TenantId")?.GetValue(entity);
                        if (currentTenantId == Guid.Empty || currentTenantId == null)
                        {
                            entity.GetType().GetProperty("TenantId")?.SetValue(entity, tenantId);
                        }
                    }
                }
            }

            await _dbSet.AddRangeAsync(entities);
        }

    }
}