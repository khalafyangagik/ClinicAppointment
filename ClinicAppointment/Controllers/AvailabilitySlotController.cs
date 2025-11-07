using Domain.DTOs;
using Domain.Models;
using Infrastructure.DbContextFolder;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ClinicAppointment.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AvailabilitySlotController : ControllerBase
    {
        private readonly ClinicDbContext _dbContext;

        public AvailabilitySlotController(ClinicDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        // ✅ Բժիշկը ավելացնում է իր ազատ ժամերը
        [Authorize(Roles = "Doctor")]
        [HttpPost("create")]
        public async Task<IActionResult> CreateSlot([FromBody] CreateSlotDto dto)
        {
            // JWT-ից doctor-ի userId
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
                return Unauthorized("User ID not found in token.");

            int appUserId = int.Parse(userIdClaim);

            // Գտնում ենք տվյալ doctor-ը ըստ userId-ի
            var doctor = await _dbContext.Doctors
                .FirstOrDefaultAsync(d => d.AppUserId == appUserId);

            if (doctor == null)
                return BadRequest("Doctor profile not found for this account.");

            // Ստուգում ենք՝ արդյոք նման ժամով slot արդեն կա
            bool exists = await _dbContext.AvailabilitySlots.AnyAsync(s =>
                s.DoctorId == doctor.Id &&
                s.StartUtc == dto.StartUtc &&
                s.EndUtc == dto.EndUtc);

            if (exists)
                return BadRequest("A slot already exists for this time range.");

            // Ստեղծում ենք նոր slot
            var slot = new AvailabilitySlot
            {
                DoctorId = doctor.Id,
                StartUtc = dto.StartUtc,
                EndUtc = dto.EndUtc,
                IsBooked = false,
                CreatedAtUtc = DateTime.UtcNow
            };

            await _dbContext.AvailabilitySlots.AddAsync(slot);
            await _dbContext.SaveChangesAsync();

            return Ok(new
            {
                Message = "Slot created successfully",
                SlotId = slot.Id,       // 👈 սա ավտոմատ գեներացվում ա EF-ի կողմից
                Doctor = doctor.FullName,
                slot.StartUtc,
                slot.EndUtc
            });
        }

        [Authorize(Roles = "Patient,Admin")]
        [HttpGet("doctor/{doctorId}/available")]
        public async Task<IActionResult> GetAvailableSlots(int doctorId, [FromQuery] DateTime date)
        {
            var slots = await _dbContext.AvailabilitySlots
                .Where(s => s.DoctorId == doctorId && s.StartUtc.Date == date.Date)
                .OrderBy(s => s.StartUtc)
                .ToListAsync();

            if (!slots.Any())
                return NotFound("Այս բժշկի համար ազատ ժամեր չեն գտնվել տվյալ օրը։");

            var appointments = await _dbContext.Appointments
                .Where(a => a.DoctorId == doctorId && a.StartUtc.Date == date.Date)
                .Select(a => new { a.StartUtc, a.EndUtc })
                .ToListAsync();

            var availableTimes = new List<object>();

            foreach (var slot in slots)
            {
                DateTime current = slot.StartUtc;

                while (current < slot.EndUtc)
                {
                    var next = current.AddMinutes(30); // 30 րոպեանոց միջակայք

                    bool isBusy = appointments.Any(a =>
                        (current >= a.StartUtc && current < a.EndUtc) ||
                        (next > a.StartUtc && next <= a.EndUtc));

                    if (!isBusy)
                    {
                        availableTimes.Add(new
                        {
                            TimeRange = $"{current:HH\\:mm} - {next:HH\\:mm}" // 👈 dd:mm ձևաչափով
                        });
                    }

                    current = next;
                }
            }

            return Ok(new
            {
                DoctorId = doctorId,
                Date = date.ToString("dd.MM.yyyy"), // 👈 օրը նույնպես ֆորմատավորված
                AvailableSlots = availableTimes
            });
        }

    }
    }


