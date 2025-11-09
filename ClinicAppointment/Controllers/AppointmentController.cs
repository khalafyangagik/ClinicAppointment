using System.Security.Claims;
using Application.Services;
using Domain.DTOs;
using Domain.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize(Roles = "Doctor,Patient")]
[ApiController]
[Route("api/[controller]")]
public class AppointmentController : ControllerBase
{

    private readonly IAppointmentService _service;

    public AppointmentController(IAppointmentService appointmentService)
    {
        _service = appointmentService;
    }

    [Authorize(Roles = "Patient")]
    [HttpPost("create")]
    public async Task<IActionResult> CreateAppointment([FromBody] CreateAppointmentDto dto)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null)
            return Unauthorized("User not found in token.");

        int userId = int.Parse(userIdClaim);

        var result = await _service.CreateAppointmentAsync(dto, userId);
        if (!result.Success)
            return BadRequest(new { result.Message });

        return Ok(new
        {
            result.Message,
            AppointmentId = result.Appointment!.Id,
            result.Appointment.StartUtc,
            result.Appointment.EndUtc
        });
    }

    // ✅ DOCTOR — view appointments
    [Authorize(Roles = "Doctor")]
    [HttpGet("doctor")]
    public async Task<IActionResult> GetDoctorAppointments([FromQuery] DateTime? date = null)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null)
            return Unauthorized("User ID not found in token.");

        int userId = int.Parse(userIdClaim);

        var list = await _service.GetDoctorAppointmentsAsync(userId, date);
        return Ok(list);
    }

    // ✅ PATIENT — view appointments
    [Authorize(Roles = "Patient")]
    [HttpGet("my")]
    public async Task<IActionResult> GetPatientAppointments()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null)
            return Unauthorized("User ID not found in token.");

        int userId = int.Parse(userIdClaim);

        var list = await _service.GetPatientAppointmentsAsync(userId);
        return Ok(list);
    }

    // ✅ CANCEL appointment
    [Authorize(Roles = "Patient")]
    [HttpDelete("{id}/cancel")]
    public async Task<IActionResult> CancelAppointment(int id)
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userIdClaim == null)
            return Unauthorized("User ID not found in token.");

        int userId = int.Parse(userIdClaim);

        var (success, message) = await _service.CancelAppointmentAsync(id, userId);
        return success ? Ok(new { message }) : BadRequest(new { message });
    }

    [Authorize(Roles = "Patient,Doctor,Admin")]
    [HttpPut("{appointmentId}/move-to-slot/{slotId}")]
    public async Task<IActionResult> MoveAppointmentToSlot(int appointmentId, int slotId)
    {
        var result = await _service.UpdateAppointmentBySlotAsync(appointmentId, slotId);

        if (!result.Success)
            return BadRequest(new { result.Message });

        return Ok(new
        {
            result.Message,
            AppointmentId = result.Updated!.Id,
            NewSlot = new
            {
                result.Updated.StartUtc,
                result.Updated.EndUtc
            }
        });
    }
}
    
