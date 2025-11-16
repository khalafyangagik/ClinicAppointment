using Domain.IRepository;
using Domain.Models;
using Infrastructure.DbContextFolder;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{
    public class AppointmentRepository
        : GenericRepository<Appointment>, IAppointmentRepository
    {
        public AppointmentRepository(ClinicDbContext context)
            : base(context)
        {
        }

        public async Task<Appointment?> GetWithDetailsAsync(int id)
        {
            return await _context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .Include(a => a.Slot)
                .FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<IEnumerable<Appointment>> GetAllWithDetailsAsync()
        {
            return await _context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Patient)
                .Include(a => a.Slot)
                .OrderByDescending(a => a.CreatedAtUtc)
                .ToListAsync();
        }

        public async Task<IEnumerable<Appointment>> GetDoctorAppointmentsAsync(int doctorId, DateTime? date = null)
        {
            var query = _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Slot)
                .Where(a => a.DoctorId == doctorId);

            if (date.HasValue)
                query = query.Where(a => a.StartUtc.Date == date.Value.Date);

            return await query.OrderBy(a => a.StartUtc).ToListAsync();
        }

        public async Task<IEnumerable<Appointment>> GetPatientAppointmentsAsync(int patientId)
        {
            return await _context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Slot)
                .Where(a => a.PatientId == patientId)
                .OrderByDescending(a => a.StartUtc)
                .ToListAsync();
        }
        public async Task<Appointment?> GetByIdAndPatientAsync(int appointmentId, int patientId)
        {
            return await _context.Appointments
                .Include(a => a.Slot)
                .FirstOrDefaultAsync(a =>
                    a.Id == appointmentId &&
                    a.PatientId == patientId);
        }

        public async Task<Appointment?> GetWithSlotAndDoctorAsync(int appointmentId)
        {
            return await _context.Appointments
                .Include(a => a.Slot)
                .Include(a => a.Doctor)
                .FirstOrDefaultAsync(a => a.Id == appointmentId);
        }
        public async Task<bool> DoctorHasOverlapAsync(int doctorId, DateTime startUtc, DateTime endUtc, int excludingAppointmentId = 0)
        {
            return await _context.Appointments.AnyAsync(a =>
                a.DoctorId == doctorId &&
                a.Id != excludingAppointmentId &&
                a.Status != "Cancelled" &&
                (
                    (startUtc >= a.StartUtc && startUtc < a.EndUtc) ||
                    (endUtc > a.StartUtc && endUtc <= a.EndUtc)
                ));
        }

        public async Task<IEnumerable<Appointment>> GetAppointmentsByPatientPagedAsync(int patientId, int page = 1, int pageSize = 5)
        {
            var query = _context.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Slot)
                .Where(a => a.PatientId == patientId)
                .OrderByDescending(a => a.StartUtc);

            return await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }
    }
}
