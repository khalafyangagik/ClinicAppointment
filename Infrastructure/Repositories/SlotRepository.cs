using Domain.IRepository;
using Domain.Models;
using Infrastructure.DbContextFolder;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class SlotRepository : IRepository<AvailabilitySlot>
    {
        private readonly ClinicDbContext _dbContext;
        public SlotRepository(ClinicDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task AddAsync(AvailabilitySlot entity)
        {
            await _dbContext.AvailabilitySlots.AddAsync(entity);
        }

        public void Delete(AvailabilitySlot entity)
        {
            _dbContext.AvailabilitySlots.Remove(entity);
        }

        public async Task<IEnumerable<AvailabilitySlot>> GetAllAsync()
        {
            return await _dbContext.AvailabilitySlots
                                  .Include(s => s.Doctor) 
                                  .ToListAsync();
        }

        public async Task<AvailabilitySlot?> GetByIdAsync(int id)
        {
            return await _dbContext.AvailabilitySlots
                                   .Include(s => s.Doctor)
                                   .FirstOrDefaultAsync(s => s.Id == id);
        }

        public void Update(AvailabilitySlot entity)
        {
            _dbContext.AvailabilitySlots.Update(entity);
        }
    }
}
