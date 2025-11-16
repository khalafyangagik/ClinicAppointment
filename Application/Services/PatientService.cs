using AutoMapper;
using Domain.DTOs;
using Domain.IRepository;
using Domain.IServices;
using Domain.Models;
using Infrastructure.DbContextFolder;

namespace Application.Services
{
    public class PatientService : IPatientService
    {
        private readonly IRepository<Patient> _patientRepo;
        private readonly ClinicDbContext _dbContext;
        private readonly IMapper _mapper;
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly INoteRepository _noteRepository;

        public PatientService(
            IRepository<Patient> patientRepo,
            ClinicDbContext dbContext,IMapper mapper, IAppointmentRepository appointmentRepository, INoteRepository noteRepository)
        {
            _patientRepo = patientRepo;
            _dbContext = dbContext;
            _mapper = mapper;
            _appointmentRepository = appointmentRepository;
            _noteRepository = noteRepository;
        }

        // ---------------- CRUD ----------------
        public async Task AddAsync(Patient patient)
        {
            await _patientRepo.AddAsync(patient);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<Patient?> GetByIdAsync(int id)
        {
    
            return await _patientRepo.GetByIdAsync(id);
        }

        public async Task<IEnumerable<PatientDto>> GetAllAsync()
        {
            
            var patients =  await _patientRepo.GetAllAsync();
            return _mapper.Map<IEnumerable<PatientDto>>(patients);
        }

        public void Update(Patient patient)
        {
            _patientRepo.Update(patient);
            _dbContext.SaveChanges();
        }

        public void Delete(Patient patient)
        {
            _patientRepo.Delete(patient);
            _dbContext.SaveChanges();
        }


        public async Task<IEnumerable<Appointment>> GetAppointmentsAsync(int patientId, int page = 1, int pageSize = 5)
        {
            return await _appointmentRepository.GetAppointmentsByPatientPagedAsync(patientId, page, pageSize);
        }

        public async Task<IEnumerable<Note>> GetDoctorNotesAsync(int patientId)
        {
            return await _noteRepository.GetNotesByPatientAsync(patientId);
        }
    }
}
