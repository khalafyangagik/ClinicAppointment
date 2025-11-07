using Domain.IRepository;
using Domain.Models;
using Infrastructure.DbContextFolder;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class AppointmentRepository : IRepository<Appointment>
    {
        private readonly ClinicDbContext _dbcontext;
        public AppointmentRepository(ClinicDbContext dbcontext)
        {
            _dbcontext = dbcontext;
        }

        public async Task AddAsync(Appointment entity)
        {
            await _dbcontext.AddAsync(entity);
        }

        public void Delete(Appointment entity)
        {
            _dbcontext.Remove(entity);
        }

        public async Task<IEnumerable<Appointment>> GetAllAsync()
        {
            return await _dbcontext.Appointments.ToListAsync();
        }

        public void Update(Appointment entity)
        {
            _dbcontext.Update(entity);
        }

        public async Task<Appointment?> GetByIdAsync(int id)
        {
            return await _dbcontext.Appointments.FindAsync(id);
        }

    }
}
