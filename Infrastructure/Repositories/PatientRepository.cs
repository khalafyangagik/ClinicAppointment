using Domain.IRepository;
using Domain.Models;
using Infrastructure.DbContextFolder;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class PatientRepository : IRepository<Patient>
    {
        private readonly ClinicDbContext _dbcontext;
        public PatientRepository(ClinicDbContext dbcontext)
        {
            _dbcontext = dbcontext;
        }

        public async Task AddAsync(Patient entity)
        {
            await _dbcontext.AddAsync(entity);
        }

        public void Delete(Patient entity)
        {
            _dbcontext.Patients.Remove(entity);
        }

        public async Task<IEnumerable<Patient>> GetAllAsync()
        {
            return await _dbcontext.Patients.ToListAsync();
        }

        public async Task<Patient?> GetByIdAsync(int id)
        {
            return await _dbcontext.Patients.FindAsync(id);
        }

        public void Update(Patient entity)
        {
            _dbcontext.Patients.Update(entity);
        }
    }
}
