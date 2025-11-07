using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.IRepository;
using Domain.IServices;
using Domain.Models;
using Infrastructure.DbContextFolder;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public class PatientService : IPatientService
    {
        private readonly IRepository<Patient> _patientRepo;
        private readonly IRepository<Appointment> _appointmentRepo;
        private readonly IRepository<Note> _noteRepo;
        private readonly ClinicDbContext _dbContext;

        public PatientService(
            IRepository<Patient> patientRepo,
            IRepository<Appointment> appointmentRepo,
            IRepository<Note> noteRepo,
            ClinicDbContext dbContext)
        {
            _patientRepo = patientRepo;
            _appointmentRepo = appointmentRepo;
            _noteRepo = noteRepo;
            _dbContext = dbContext;
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

        public async Task<IEnumerable<Patient>> GetAllAsync()
        {
            return await _patientRepo.GetAllAsync();
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

        // ---------------- Business Logic ----------------

        /// <summary>
        /// Բերում է հիվանդի բոլոր appointments-ները էջավորված ձևով։
        /// </summary>
        public async Task<IEnumerable<Appointment>> GetAppointmentsAsync(int patientId, int page = 1, int pageSize = 5)
        {
            var query = _dbContext.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Slot)
                .Where(a => a.PatientId == patientId)
                .OrderByDescending(a => a.StartUtc);

            // pagination logic
            return await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        /// <summary>
        /// Բերում է բոլոր note-երը, որոնք բժիշկները գրել են տվյալ հիվանդի appointment-ների համար։
        /// </summary>
        public async Task<IEnumerable<Note>> GetDoctorNotesAsync(int patientId)
        {
            var notes = await _dbContext.Notes
                .Include(n => n.Appointment)
                .ThenInclude(a => a.Doctor)
                .Where(n => n.Appointment.PatientId == patientId)
                .OrderByDescending(n => n.Appointment.StartUtc)
                .ToListAsync();

            return notes;
        }
    }
}
