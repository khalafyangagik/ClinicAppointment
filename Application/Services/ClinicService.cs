using Domain.DTOs;
using Domain.IRepository;
using Domain.IServices;
using Domain.Models;
using Infrastructure.DbContextFolder;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
namespace Application.Services
{
    public class ClinicService : IClinicService
    {
        private readonly ClinicDbContext _dbContext;
        private readonly IRepository<Clinic> _clinicRepo;
        private readonly IMapper _mapper;

        public ClinicService(ClinicDbContext dbContext, IRepository<Clinic> clinicRepo, IMapper mapper)
        {
            _dbContext = dbContext;
            _clinicRepo = clinicRepo;
            _mapper = mapper;
        }

        public async Task AddAsync(CreateClinicDto dto)
        {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));
            var clinic = _mapper.Map<Clinic>(dto);
            await _clinicRepo.AddAsync(clinic);
            await _dbContext.SaveChangesAsync();
        }

        public async Task DeleteAsync(Clinic entity)
        {
            _clinicRepo.Delete(entity);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<IEnumerable<Clinic>> GetAllAsync()
        {
            return await _clinicRepo.GetAllAsync();
        }

        public async Task<Clinic?> GetByIdAsync(int id)
        {
            return await _clinicRepo.GetByIdAsync(id);
        }

        public async Task<IEnumerable<Doctor>> GetDoctorsBySpecialityAsync(int clinicId, string speciality)
        {
            return await _dbContext.Doctors
                                         .Include(d => d.AppUser) // որ բերի նաև UserName, Email, և այլն
                                         .Where(d => d.ClinicId == clinicId && d.Speciality == speciality)
                                         .ToListAsync();

        }

        public async Task UpdateAsync(Clinic entity)
        {
             _clinicRepo.Update(entity);
            await _dbContext.SaveChangesAsync();

        }
    }
}
