using System.Security.Claims;
using Domain.DTOs;
using Domain.Models;
using Infrastructure.DbContextFolder;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicAppointment.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AppointmentController : ControllerBase
    {
        private readonly ClinicDbContext _dbContext;

        public AppointmentController(ClinicDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // ✅ BOOK APPOINTMENT (Transactional)
        [Authorize(Roles = "Patient")]
        [HttpPost("create")]
        public async Task<IActionResult> CreateAppointment([FromBody] CreateAppointmentDto dto)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null) return Unauthorized("User ID not found in token.");

            int appUserId = int.Parse(userId);
            var patient = await _dbContext.Patients.FirstOrDefaultAsync(p => p.AppUserId == appUserId);
            if (patient == null)
                return BadRequest("Patient profile not found.");

            // Ստուգում ենք slot-ը
            var slot = await _dbContext.AvailabilitySlots.FirstOrDefaultAsync(s => s.Id == dto.SlotId);
            if (slot == null)
                return BadRequest("Selected slot not found.");

            // Ստուգում ենք՝ ընտրված ժամերը ընկնում են slot-ի մեջ
            if (dto.StartUtc < slot.StartUtc || dto.EndUtc > slot.EndUtc)
                return BadRequest("Selected time is outside the doctor's available hours.");

            // Ստուգում ենք՝ արդյոք տվյալ բժիշկը արդեն ունի appointment այդ ժամին
            bool overlaps = await _dbContext.Appointments.AnyAsync(a =>
                a.DoctorId == slot.DoctorId &&
                ((dto.StartUtc >= a.StartUtc && dto.StartUtc < a.EndUtc) ||
                 (dto.EndUtc > a.StartUtc && dto.EndUtc <= a.EndUtc)));

            if (overlaps)
                return BadRequest("This doctor already has an appointment at the selected time.");

            // Ստեղծում ենք appointment
            var appointment = new Appointment
            {
                DoctorId = slot.DoctorId,
                PatientId = patient.Id,
                SlotId = slot.Id,
                StartUtc = dto.StartUtc,
                EndUtc = dto.EndUtc,
                Status = "Reserved",
                CreatedAtUtc = DateTime.UtcNow
            };

            await _dbContext.Appointments.AddAsync(appointment);
            await _dbContext.SaveChangesAsync();

            return Ok(new
            {
                Message = "Appointment booked successfully!",
                DoctorId = slot.DoctorId,
                PatientId = patient.Id,
                StartUtc = dto.StartUtc,
                EndUtc = dto.EndUtc
            });
        }
 // ✅ CANCEL APPOINTMENT
        [Authorize(Roles = "Patient")]
        [HttpDelete("{id}/cancel")]
        public async Task<IActionResult> CancelAppointment(int id)
        {
            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                var appointment = await _dbContext.Appointments
                    .Include(a => a.Slot)
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (appointment == null)
                    return NotFound("Appointment not found.");

                if (appointment.Status == "Cancelled")
                    return BadRequest("Appointment is already cancelled.");

                // Cancel appointment
                appointment.Status = "Cancelled";
                appointment.Slot!.IsBooked = false;

                _dbContext.Appointments.Update(appointment);
                _dbContext.AvailabilitySlots.Update(appointment.Slot);
                await _dbContext.SaveChangesAsync();

                await transaction.CommitAsync();
                return Ok(new { Message = "Appointment cancelled successfully." });
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { Error = "Failed to cancel appointment.", ex.Message });
            }
        }

        // ✅ DOCTOR: View their appointments
        [Authorize(Roles = "Doctor")]
        [HttpGet("doctor")]
        public async Task<IActionResult> GetDoctorAppointments([FromQuery] DateTime? date = null)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
                return Unauthorized("User ID not found in token.");

            var doctor = await _dbContext.Doctors
                .FirstOrDefaultAsync(d => d.AppUserId == int.Parse(userIdClaim));

            if (doctor == null)
                return BadRequest("Doctor profile not found.");

            IQueryable<Appointment> query = _dbContext.Appointments
                .Include(a => a.Patient)
                .Where(a => a.DoctorId == doctor.Id)
                .OrderBy(a => a.StartUtc);

            if (date.HasValue)
                query = query.Where(a => a.StartUtc.Date == date.Value.Date);

            var list = await query.ToListAsync();
            return Ok(list);
        }

        // ✅ PATIENT: View their appointments
        [Authorize(Roles = "Patient")]
        [HttpGet("my")]
        public async Task<IActionResult> GetMyAppointments()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
                return Unauthorized("User ID not found in token.");

            var patient = await _dbContext.Patients
                .FirstOrDefaultAsync(p => p.AppUserId == int.Parse(userIdClaim));

            if (patient == null)
                return BadRequest("Patient profile not found.");

            var list = await _dbContext.Appointments
                .Include(a => a.Doctor)
                .Include(a => a.Slot)
                .Where(a => a.PatientId == patient.Id)
                .OrderByDescending(a => a.StartUtc)
                .ToListAsync();

            return Ok(list);
        }
    }
}
