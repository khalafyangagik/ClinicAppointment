using Domain.DTOs;
using Domain.IRepository;
using Domain.IServices;
using Domain.Models;
using Infrastructure.DbContextFolder;
using Microsoft.Extensions.Logging;

namespace Application.Services
{
    public class AppointmentService : IAppointmentService
    {
        private readonly ClinicDbContext _dbContext;
        private readonly ILogger<AppointmentService> _logger;
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly IDoctorRepository _doctorRepository;
        private readonly IPatientRepository _patientRepository;
        private readonly ISlotRepository _slotRepository;

        public AppointmentService(ClinicDbContext dbContext, ILogger<AppointmentService> logger, IAppointmentRepository repository, IDoctorRepository doctorRepository, IPatientRepository patientRepository, ISlotRepository slotRepository)
        {
            _dbContext = dbContext;
            _logger = logger;
            _appointmentRepository = repository;
            _doctorRepository = doctorRepository;
            _patientRepository = patientRepository;
            _slotRepository = slotRepository;
        }

        public async Task AddAsync(Appointment appointment)
        {
            await _appointmentRepository.AddAsync(appointment);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<Appointment?> GetAsync(int id)
        {
            return await _appointmentRepository.GetWithDetailsAsync(id);
        }


        public async Task<IEnumerable<Appointment>> GetAllAsync()
        {
            return await _appointmentRepository.GetAllWithDetailsAsync();
        }

        public void Update(Appointment appointment)
        {
            _dbContext.Appointments.Update(appointment);
            _dbContext.SaveChanges();
        }

        public void Delete(Appointment appointment)
        {
            _appointmentRepository.Delete(appointment);
            _dbContext.SaveChanges();
        }

        // ✅ BOOK appointment
        public async Task<(bool Success, string Message, Appointment? Appointment)>
     CreateAppointmentAsync(CreateAppointmentDto dto, int userId)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var patient = _patientRepository.GetByUserIdAsync(userId);

                if (patient == null)
                    return (false, "Patient profile not found.", null);

                var slot = await _slotRepository.GetByIdWithDoctorAsync(dto.SlotId);

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

                await _appointmentRepository.AddAsync(appointment);
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
                var patient = _patientRepository.GetByUserIdAsync(userId);

                if (patient == null)
                    return (false, "Patient not found.");

                var appointment = await _appointmentRepository.GetByIdAndPatientAsync(appointmentId, patient.Id);

                if (appointment == null)
                    return (false, "Appointment not found.");
                if (appointment.Status == "Cancelled")
                    return (false, "Already cancelled.");

                appointment.Status = "Cancelled";
                if (appointment.Slot != null)
                    appointment.Slot.IsBooked = false;

                _appointmentRepository.Update(appointment);
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

        public async Task<IEnumerable<Appointment>> GetDoctorAppointmentsAsync(int userId, DateTime? date = null)
        {
            var doctor = await _doctorRepository.GetByUserIdAsync(userId);
            if (doctor == null)
                return Enumerable.Empty<Appointment>();

            return await _appointmentRepository.GetDoctorAppointmentsAsync(doctor.Id, date);
        }

        public async Task<IEnumerable<Appointment>> GetPatientAppointmentsAsync(int userId)
        {
            
            var patient = await _patientRepository.GetByUserIdAsync(userId);

            if (patient == null)
                return Enumerable.Empty<Appointment>();

            return await _appointmentRepository.GetPatientAppointmentsAsync(patient.Id);
        }

        public async Task<(bool Success, string Message, Appointment? Updated)>
     UpdateAppointmentBySlotAsync(int appointmentId, int newSlotId)
        {
            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var appointment = await _appointmentRepository.GetWithSlotAndDoctorAsync(appointmentId);

                if (appointment == null)
                    return (false, "Appointment not found.", null);

                if (appointment.Status == "Cancelled")
                    return (false, "Cannot update a cancelled appointment.", null);

                var newSlot = await _slotRepository.GetByIdWithDoctorAsync(newSlotId);

                if (newSlot == null)
                    return (false, "New slot not found.", null);

                if (newSlot.IsBooked)
                    return (false, "This slot is already booked.", null);
                
                bool overlaps = await _appointmentRepository.DoctorHasOverlapAsync(
                    newSlot.DoctorId,
                    newSlot.StartUtc,
                    newSlot.EndUtc,
                    appointment.Id
                );


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

                _appointmentRepository.Update(appointment);

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
