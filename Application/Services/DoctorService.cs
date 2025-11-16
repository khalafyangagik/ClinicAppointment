using AutoMapper;
using Domain.DTOs;
using Domain.IRepository;
using Domain.IServices;
using Domain.Models;
using Infrastructure.DbContextFolder;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
    {
        public class DoctorService : IDoctorService
        {
            private readonly IDoctorRepository _doctorRepo;
            private readonly ISlotRepository _slotRepo;
            private readonly IRepository<Note> _noteRepo;
            private readonly ClinicDbContext _dbContext;
            private readonly IUnitOfWork _unitOfWork;
            private readonly IMapper _mapper;
            private readonly IDoctorRepository _doctorRepository;

            public DoctorService(IDoctorRepository doctorRepo,ISlotRepository slotRepo,IRepository<Note> noteRepo,ClinicDbContext dbContext,
                IUnitOfWork unitOfWork,IMapper mapper, IDoctorRepository doctorRepository)
            {
                _doctorRepo = doctorRepo;
                _slotRepo = slotRepo;
                _noteRepo = noteRepo;
                _dbContext = dbContext;
                _unitOfWork = unitOfWork;
                _mapper = mapper;
                _doctorRepository = doctorRepository;
            }


            public async Task AddAsync(CreateDoctorDto dto)
            {
            if (dto == null)
                throw new ArgumentNullException(nameof(dto));

            bool exists = await _dbContext.Users.AnyAsync(u => u.Email == dto.Email);
            if (exists)
                throw new InvalidOperationException("A user with this email already exists.");
            var clinicExists = await _dbContext.Clinics.AnyAsync(c => c.Id == dto.ClinicId);
            if (!clinicExists)
                throw new InvalidOperationException("Clinic not found. Please provide a valid clinic ID.");


            var doctor = _mapper.Map<Doctor>(dto);
            doctor.IsApproved = false;   // Admin can approve later if needed
            doctor.AppUserId = null;        // no user yet — will be filled after registration

            await _doctorRepo.AddAsync(doctor);
            await _dbContext.SaveChangesAsync();
        }

            public async Task<IEnumerable<Doctor>> GetAllAsync()
            {
                return await _doctorRepo.GetAllAsync();
            }

            public async Task<Doctor?> GetByIdAsync(int id)
            {
                return await _doctorRepo.GetByIdAsync(id);
            }

            public void Update(Doctor entity)
            {
                _doctorRepo.Update(entity);
                _dbContext.SaveChanges();
            }

            public void Delete(Doctor entity)
            {
                _doctorRepo.Delete(entity);
                _dbContext.SaveChanges();
            }

         

            public async Task AddAvailabilitySlot(AvailabilitySlot entity)
            {
                await _slotRepo.AddAsync(entity);
                await _dbContext.SaveChangesAsync();
            }

            public async Task CancelAvailibiltySlot(int id)
            {
                var slot = await _slotRepo.GetByIdAsync(id);
                if (slot == null)
                    throw new KeyNotFoundException("Slot not found.");

                if (slot.IsBooked)
                    throw new InvalidOperationException("Cannot cancel a booked slot.");

                _slotRepo.Delete(slot);
                await _dbContext.SaveChangesAsync();
            }

            public async Task AddNoteForPatient(Note note)
            {

                await _unitOfWork.BeginTransactionAsync();
                try
                {
                    var appointment = await _dbContext.Appointments
                        .FirstOrDefaultAsync(a => a.Id == note.AppointmentId);

                    if (appointment == null)
                        throw new KeyNotFoundException("Appointment not found.");

                    await _noteRepo.AddAsync(note);
                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitAsync();
                }
                catch
                {
                    await _unitOfWork.RollbackAsync();
                    throw;
                }
            }

        public async Task<IEnumerable<AvailabilitySlot>> GetScheduleAsync(
            int doctorId, DateTime? date = null, int page = 1, int pageSize = 5)
        {
            return await _slotRepo.GetScheduleAsync(doctorId, date, page, pageSize);
        }

        public async Task<IEnumerable<Doctor>> GetDoctorsBySpecialityAsync(int clinicId, string speciality)
        {
            return await _doctorRepository.GetByClinicAndSpecialityAsync(clinicId, speciality);
        }
    }
        }
    
