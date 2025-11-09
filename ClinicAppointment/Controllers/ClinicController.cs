using System.Security.Claims;
using Domain.DTOs;
using Domain.IServices;
using Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicAppointment.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClinicsController : ControllerBase
    {
        private readonly IClinicService _clinicService;

        public ClinicsController(IClinicService clinicService)
        {
            _clinicService = clinicService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var clinics = await _clinicService.GetAllAsync();
            return Ok(clinics);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var clinic = await _clinicService.GetByIdAsync(id);
            if (clinic == null) return NotFound();
            return Ok(clinic);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateClinicDto dto)
        {
            await _clinicService.AddAsync(dto);
            return Ok();
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] Clinic clinic)
        {
            if (id != clinic.Id) return BadRequest();
            await _clinicService.UpdateAsync(clinic);
            return NoContent();
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null)
                return Unauthorized("User not found.");

            var clinic = await _clinicService.GetByIdAsync(id);
            if (clinic == null) return NotFound();

            await _clinicService.DeleteAsync(clinic);
            return NoContent();
        }
    }

}
