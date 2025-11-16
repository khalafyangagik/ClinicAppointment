using Domain.IRepository;
using Domain.Models;
using Infrastructure.DbContextFolder;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class DoctorRepository
        : GenericRepository<Doctor>, IDoctorRepository
    {
        public DoctorRepository(ClinicDbContext context)
            : base(context) 
        {
        }

        public async Task<IEnumerable<Doctor>> GetDoctorsWithClinicAsync()
        {
            return await _context.Doctors
                                 .Include(d => d.Clinic)
                                 .ToListAsync();
        }

        public async Task<Doctor?> GetByIdWithClinicAsync(int id)
        {
            return await _context.Doctors
                                 .Include(d => d.Clinic)
                                 .FirstOrDefaultAsync(d => d.Id == id);
        }

        public async Task<Doctor?> GetByUserIdAsync(int userId)
        {
            return await _context.Doctors
                .Include(d => d.Clinic)
                .FirstOrDefaultAsync(d => d.AppUserId == userId);
        }

        public async Task<IEnumerable<Doctor>> GetByClinicAndSpecialityAsync(int clinicId, string speciality)
        {
            return await _context.Doctors
                .Include(d => d.AppUser) 
                .Where(d => d.ClinicId == clinicId && d.Speciality == speciality)
                .ToListAsync();
        }

        public async Task<Doctor?> GetUnassignedDoctorByClinicAsync(int clinicId)
        {
            return await _context.Doctors
                .FirstOrDefaultAsync(d => d.AppUserId == null && d.ClinicId == clinicId);
        }
    }

}
