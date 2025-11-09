using Domain.DTOs;
using Domain.IServices;
using Domain.Models;
using Infrastructure.DbContextFolder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly ClinicDbContext _dbContext;
        private readonly ILogger<AppointmentService> _logger;

        public AppointmentService(ClinicDbContext dbContext, ILogger<AppointmentService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        // --- CRUD ---
        public async Task AddAsync(Appointment appointment)
        {
            await _dbContext.Appointments.AddAsync(appointment);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<Appointment?> GetAsync(int id) =>
            await _dbContext.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .Include(a => a.Slot)
                .FirstOrDefaultAsync(a => a.Id == id);

        public async Task<IEnumerable<Appointment>> GetAllAsync() =>
            await _dbContext.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .Include(a => a.Slot)
                .OrderByDescending(a => a.CreatedAtUtc)
                .ToListAsync();

        public void Update(Appointment appointment)
        {
            _dbContext.Appointments.Update(appointment);
            _dbContext.SaveChanges();
        }

        public void Delete(Appointment appointment)
        {
            _dbContext.Appointments.Remove(appointment);
            _dbContext.SaveChanges();
        }

        // ✅ BOOK appointment
        public async Task<(bool Success, string Message, Appointment? Appointment)>
     CreateAppointmentAsync(CreateAppointmentDto dto, int userId)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var patient = await _dbContext.Patients
                    .FirstOrDefaultAsync(p => p.AppUserId == userId);

                if (patient == null)
                    return (false, "Patient profile not found.", null);

                var slot = await _dbContext.AvailabilitySlots
                    .Include(s => s.Doctor)
                    .FirstOrDefaultAsync(s => s.Id == dto.SlotId);

                if (slot == null)
                    return (false, "Selected slot not found.", null);

                if (slot.IsBooked)
                    return (false, "This slot is already booked.", null);

                var appointment = new Appointment
                {
                    DoctorId = slot.DoctorId,
                    PatientId = patient.Id,
                    SlotId = slot.Id,
                    StartUtc = slot.StartUtc,
                    EndUtc = slot.EndUtc,
                    Status = "Reserved",
                    CreatedAtUtc = DateTime.UtcNow
                };

                await _dbContext.Appointments.AddAsync(appointment);
                slot.IsBooked = true;

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return (true, "Appointment booked successfully!", appointment);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error while booking appointment");
                return (false, "Failed to book appointment.", null);
            }
        }
   

        // ✅ CANCEL appointment
        public async Task<(bool Success, string Message)>
            CancelAppointmentAsync(int appointmentId, int userId)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var patient = await _dbContext.Patients
                    .FirstOrDefaultAsync(p => p.AppUserId == userId);
                if (patient == null)
                    return (false, "Patient not found.");

                var appointment = await _dbContext.Appointments
                    .Include(a => a.Slot)
                    .FirstOrDefaultAsync(a => a.Id == appointmentId && a.PatientId == patient.Id);

                if (appointment == null)
                    return (false, "Appointment not found.");
                if (appointment.Status == "Cancelled")
                    return (false, "Already cancelled.");

                appointment.Status = "Cancelled";
                if (appointment.Slot != null)
                    appointment.Slot.IsBooked = false;

                _dbContext.Appointments.Update(appointment);
                await _dbContext.SaveChangesAsync();

                await transaction.CommitAsync();
                return (true, "Appointment cancelled successfully.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error cancelling appointment");
                return (false, "Failed to cancel appointment.");
            }
        }

        // ✅ Doctor appointments
        public async Task<IEnumerable<Appointment>> GetDoctorAppointmentsAsync(int userId, DateTime? date = null)
        {
            var doctor = await _dbContext.Doctors.FirstOrDefaultAsync(d => d.AppUserId == userId);
            if (doctor == null)
                return Enumerable.Empty<Appointment>();

            var query = _dbContext.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Slot)
                .Where(a => a.DoctorId == doctor.Id);

            if (date.HasValue)
                query = query.Where(a => a.StartUtc.Date == date.Value.Date);

            return await query.OrderBy(a => a.StartUtc).ToListAsync();
        }

        // ✅ Patient appointments
        public async Task<IEnumerable<Appointment>> GetPatientAppointmentsAsync(int userId)
        {
            var patient = await _dbContext.Patients.FirstOrDefaultAsync(p => p.AppUserId == userId);
            if (patient == null)
                return Enumerable.Empty<Appointment>();

            return await _dbContext.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Slot)
                .Where(a => a.PatientId == patient.Id)
                .OrderByDescending(a => a.StartUtc)
                .ToListAsync();
        }

        public async Task<(bool Success, string Message, Appointment? Updated)>
     UpdateAppointmentBySlotAsync(int appointmentId, int newSlotId)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var appointment = await _dbContext.Appointments
                    .Include(a => a.Slot)
                    .Include(a => a.Doctor)
                    .FirstOrDefaultAsync(a => a.Id == appointmentId);

                if (appointment == null)
                    return (false, "Appointment not found.", null);

                if (appointment.Status == "Cancelled")
                    return (false, "Cannot update a cancelled appointment.", null);

                var newSlot = await _dbContext.AvailabilitySlots
                    .Include(s => s.Doctor)
                    .FirstOrDefaultAsync(s => s.Id == newSlotId);

                if (newSlot == null)
                    return (false, "New slot not found.", null);

                if (newSlot.IsBooked)
                    return (false, "This slot is already booked.", null);

                bool overlaps = await _dbContext.Appointments.AnyAsync(a =>
                    a.DoctorId == newSlot.DoctorId &&
                    a.Id != appointment.Id &&
                    a.Status != "Cancelled" &&
                    ((newSlot.StartUtc >= a.StartUtc && newSlot.StartUtc < a.EndUtc) ||
                     (newSlot.EndUtc > a.StartUtc && newSlot.EndUtc <= a.EndUtc)));

                if (overlaps)
                    return (false, "Doctor already has an appointment at that time.", null);

                if (appointment.Slot != null)
                {
                    appointment.Slot.IsBooked = false;
                    _dbContext.AvailabilitySlots.Update(appointment.Slot);
                }

                newSlot.IsBooked = true;
                _dbContext.AvailabilitySlots.Update(newSlot);

               
                appointment.SlotId = newSlot.Id;
                appointment.DoctorId = newSlot.DoctorId;
                appointment.StartUtc = newSlot.StartUtc;
                appointment.EndUtc = newSlot.EndUtc;
                appointment.Status = "Updated";

                _dbContext.Appointments.Update(appointment);

                await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return (true, $"Appointment moved to new slot ({newSlot.StartUtc:HH:mm}) with Dr. {newSlot.Doctor.FullName}", appointment);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error while updating appointment slot");
                return (false, "Failed to update appointment slot.", null);
            }
        }

    }
}
