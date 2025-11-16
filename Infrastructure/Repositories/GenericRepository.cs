using Domain.IRepository;
using Infrastructure.DbContextFolder;
using Microsoft.EntityFrameworkCore;

public class GenericRepository<T> : IRepository<T> where T : class
{
    protected readonly ClinicDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public GenericRepository(ClinicDbContext context)
    {
        _context = context;
        _dbSet = _context.Set<T>();  
    }

    public async Task AddAsync(T entity)
    {
        await _dbSet.AddAsync(entity);
    }

    public void Delete(T entity)
    {
        _dbSet.Remove(entity);
    }

    public async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _dbSet.ToListAsync();
    }

    public async Task<T?> GetByIdAsync(int id)
    {
        return await _dbSet.FindAsync(id);
    }

    public void Update(T entity)
    {
        _dbSet.Update(entity);
    }
}
