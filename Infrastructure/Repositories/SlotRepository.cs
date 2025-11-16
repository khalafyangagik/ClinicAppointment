using Domain.IRepository;
using Domain.Models;
using Infrastructure.DbContextFolder;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class SlotRepository : GenericRepository<AvailabilitySlot>, ISlotRepository
    {
        public SlotRepository(ClinicDbContext context)
            : base(context)
        {
        }

        public async Task<IEnumerable<AvailabilitySlot>> GetAllWithDoctorAsync()
        {
            return await _context.AvailabilitySlots
                                 .Include(s => s.Doctor)
                                 .ToListAsync();
        }

        public async Task<AvailabilitySlot?> GetByIdWithDoctorAsync(int id)
        {
            return await _context.AvailabilitySlots
                                 .Include(s => s.Doctor)
                                 .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<IEnumerable<AvailabilitySlot>> GetScheduleAsync(int doctorId,DateTime? date = null,int page = 1,int pageSize = 5)
        {
            var query = _context.AvailabilitySlots
                .Where(s => s.DoctorId == doctorId);

            // Date filter
            if (date.HasValue)
            {
                query = query.Where(s => s.StartUtc.Date == date.Value.Date);
            }

            // Pagination
            query = query
                .OrderBy(s => s.StartUtc)
                .Skip((page - 1) * pageSize)
                .Take(pageSize);

            return await query.ToListAsync();
        }
    }

}

