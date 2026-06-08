using health_information.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace health_information.Services
{
    public class HealthRepository<T> : IHealthReposiotory<T> where T : class
    {
        private readonly HealthDbContext _context;
        private readonly DbSet<T> _dbSet;

        public HealthRepository(HealthDbContext context)
        {
            _context = context;
            _dbSet = _context.Set<T>();
        }
        public async Task CreateAsync(T entity)
        {
           await _dbSet.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(T dbRecord)
        {
            _dbSet.Remove(dbRecord);
             await _context.SaveChangesAsync();

            return true; 
        }

        public async Task<List<T>> GetAllAsync()
        {
            return await _dbSet.ToListAsync();
        }

        public async Task<List<T>> GetAllByFilterAsync(Expression<Func<T, bool>> filter, string? IncludeProperties = null)
        {
            IQueryable<T> query = _dbSet;
            if(!string.IsNullOrEmpty(IncludeProperties))
            {
                foreach (var includeProp in IncludeProperties.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProp.Trim());
                }
            }
            return await query.Where(filter).ToListAsync();
        }

        public async Task<T> GetAsync(Expression<Func<T, bool>> filter = null, bool useNoTracking = false, string? IncludeProperties = null)
        {
           IQueryable<T> query = _dbSet;

            if (useNoTracking)
            {
                query = query.AsNoTracking();
            }

            if (!string.IsNullOrEmpty(IncludeProperties))
            {
                foreach (var includeProp in IncludeProperties.Split(new[]{ ','}, StringSplitOptions.RemoveEmptyEntries))
                {
                    query = query.Include(includeProp.Trim());
                }
            }

            if(filter != null)
            {
                return await query.FirstOrDefaultAsync(filter);
            }
                
            return await query.FirstOrDefaultAsync();
        }

        public async Task<T> UpdateAsync(T dbRecord)
        {
            _dbSet.Update(dbRecord);
            await _context.SaveChangesAsync();
            return dbRecord;
        }
    }
}
