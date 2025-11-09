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


        [Authorize(Roles = "Doctor")]
        [HttpPost("generate-slots")]
        public async Task<IActionResult> GenerateSlots([FromBody] CreateSlotDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
                return Unauthorized("User not found.");

            int appUserId = int.Parse(userIdClaim);

            var (success, message) = await _slotGenerator.GenerateSlotsForCurrentDoctorAsync(
                appUserId,
                dto.StartUtc,
                dto.EndUtc);

            if (!success)
                return BadRequest(new { message });

            return Ok(new { message });
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


