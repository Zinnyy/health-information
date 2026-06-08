using System.Linq.Expressions;

namespace health_information.Services
{
    public interface IHealthReposiotory<T> where T : class
    {
        Task<T> GetAsync(Expression<Func<T, bool>> filter = null, bool useNoTracking = false, string? IncludeProperties = null);
        Task<List<T>> GetAllByFilterAsync(Expression<Func<T, bool>> filter, string? IncludeProperties = null);

        Task CreateAsync(T entity);
        Task<T> UpdateAsync(T dbRecord);
        Task<bool> DeleteAsync(T dbRecord);
        Task<List<T>> GetAllAsync();
    }
}
