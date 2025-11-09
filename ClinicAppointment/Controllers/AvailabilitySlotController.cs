using System.Security.Claims;
using Domain.DTOs;
using Domain.IServices;
using Infrastructure.DbContextFolder;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicAppointment.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AvailabilitySlotController : ControllerBase
    {
        private readonly ClinicDbContext _dbContext;
        private readonly ISlotGeneratorService _slotGenerator;

        public AvailabilitySlotController(ClinicDbContext dbContext, ISlotGeneratorService slotGenerator)
        {
            _dbContext = dbContext;
            _slotGenerator = slotGenerator;

        }

        // ✅ Բժիշկը ավելացնում է իր ազատ ժամերը
        [Authorize(Roles = "Doctor")]
        [HttpPost("generate-slots")]
        public async Task<IActionResult> GenerateSlots([FromBody] CreateSlotDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
                return Unauthorized("User not found.");

            var doctor = await _dbContext.Doctors.FirstOrDefaultAsync(d => d.AppUserId == int.Parse(userIdClaim));
            if (doctor == null)
                return BadRequest("Doctor profile not found.");

            await _slotGenerator.GenerateSlotsAsync(doctor.Id, dto.StartUtc, dto.EndUtc);
            return Ok(new { Message = "Slots generated successfully." });
        }


        [Authorize(Roles = "Patient,Admin")]
        [HttpGet("doctor/{doctorId}/available")]
        public async Task<IActionResult> GetAvailableSlots(int doctorId, [FromQuery] DateTime date)
        {
            var result = await _slotGenerator.GetAvailableSlotsAsync(doctorId, date);

            if (!result.Success)
                return NotFound(new { result.Message });

            return Ok(result.Data);
        }

    }
    }


