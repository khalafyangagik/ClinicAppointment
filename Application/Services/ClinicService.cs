using AutoMapper;
using Domain.DTOs;
using Domain.IRepository;
using Domain.IServices;
using Domain.Models;
using Infrastructure.DbContextFolder;
namespace Application.Services
{
    public class ClinicService : IClinicService
    {
        private readonly ClinicDbContext _dbContext;
        private readonly IRepository<Clinic> _clinicRepo;
        private readonly IMapper _mapper;
        private readonly IDoctorRepository _doctorRepository;

        public ClinicService(ClinicDbContext dbContext, IRepository<Clinic> clinicRepo, IMapper mapper, IDoctorRepository doctorRepository)
        {
            _dbContext = dbContext;
            _clinicRepo = clinicRepo;
            _mapper = mapper;
            _doctorRepository = doctorRepository;
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


        public async Task UpdateAsync(Clinic entity)
        {
             _clinicRepo.Update(entity);
            await _dbContext.SaveChangesAsync();

        }
    }
}
