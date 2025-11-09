using Domain.IRepository;
using Domain.Models;
using Infrastructure.DbContextFolder;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class DoctorRepository : IRepository<Doctor>
    {
        private readonly ClinicDbContext _dbcontext;
        public DoctorRepository(ClinicDbContext dbcontext)
        {
            _dbcontext = dbcontext;
        }

        public async Task AddAsync(Doctor entity)
        {
            await _dbcontext.Doctors.AddAsync(entity);
        }

        public void Delete(Doctor entity)
        {
            _dbcontext.Doctors.Remove(entity);
        }

        public async Task<IEnumerable<Doctor>> GetAllAsync()
        {
            return await _dbcontext.Doctors
                                   .Include(d => d.Clinic)
                                   .ToListAsync();
        }

        public async Task<Doctor?> GetByIdAsync(int id)
        {
            return await _dbcontext.Doctors
                                   .Include(d => d.Clinic)
                                   .FirstOrDefaultAsync(d => d.Id == id);
        }

        public void Update(Doctor entity)
        {
            _dbcontext.Doctors.Update(entity);
        }
    }
}
