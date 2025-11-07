using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.IRepository;
using Domain.Models;
using Infrastructure.DbContextFolder;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class ClinicRepository : IRepository<Clinic>
    {
        private readonly ClinicDbContext _dbcontext;
        public ClinicRepository(ClinicDbContext dbcontext)
        {
            _dbcontext = dbcontext;
        }

        public async Task AddAsync(Clinic entity)
        {
            await _dbcontext.AddAsync(entity);
        }

        public void Delete(Clinic entity)
        {
            _dbcontext.Remove(entity);
        }

        public async Task<IEnumerable<Clinic>> GetAllAsync()
        {
            return await _dbcontext.Clinics.ToListAsync();
        }

        public void Update(Clinic entity)
        {
            _dbcontext.Update(entity);
        }

        public async Task<Clinic?> GetByIdAsync(int id)
        {
            return await _dbcontext.Clinics.FindAsync(id);
        }

    }
}
