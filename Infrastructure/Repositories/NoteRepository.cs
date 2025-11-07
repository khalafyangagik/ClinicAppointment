using Domain.IRepository;
using Domain.Models;
using Infrastructure.DbContextFolder;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{

    public class NoteRepository : IRepository<Note>
    {
        private readonly ClinicDbContext _dbContext;
        public NoteRepository(ClinicDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task AddAsync(Note entity)
        {
            await _dbContext.Notes.AddAsync(entity);
        }

        public void Delete(Note entity)
        {
            _dbContext.Notes.Remove(entity);
        }

        public async Task<IEnumerable<Note>> GetAllAsync()
        {
            return await _dbContext.Notes.ToListAsync();
        }

        public async Task<Note?> GetByIdAsync(int id)
        {
            return await _dbContext.Notes.FindAsync(id);
        }

        public void Update(Note entity)
        {
            _dbContext.Notes.Update(entity);
        }
    }
}
