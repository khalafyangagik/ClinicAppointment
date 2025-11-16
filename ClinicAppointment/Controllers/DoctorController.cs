using AutoMapper;
using Domain.DTOs;
using Domain.IServices;
using Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicAppointment.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DoctorsController : ControllerBase
    {
        private readonly IDoctorService _doctorService;
        private readonly ILogger<DoctorsController> _logger;
        private readonly IMapper _mapper;

        public DoctorsController(IDoctorService doctorService, ILogger<DoctorsController> logger,IMapper mapper)
        {
            _doctorService = doctorService;
            _logger = logger;
            _mapper = mapper;
        }
        [Authorize]
        [HttpPost("create")]
        public async Task<IActionResult> CreateDoctor([FromBody] CreateDoctorDto dto)
        {
            try
            {
                await _doctorService.AddAsync(dto);
                return Ok(new { Message = "Doctor created successfully." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [Authorize(Roles = "Admin,Patient")]
        [HttpGet]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var doctors = await _doctorService.GetAllAsync();
            var result = _mapper.Map<IEnumerable<DoctorDto>>(doctors);
            return Ok(result);
        }

        // ✅ 2. Գտնել կոնկրետ բժշկին ըստ Id-ի
        [Authorize(Roles = "Admin,Patient")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var doctor = await _doctorService.GetByIdAsync(id);
            if (doctor == null) return NotFound("Doctor not found.");

            var result = _mapper.Map<DoctorDto>(doctor);
            return Ok(result);
        }

        // ✅ 3. Թարմացնել բժշկի տվյալները (Admin-only)
        [Authorize(Roles = "Admin")]
        [HttpPut("{id:int}")]
        public IActionResult Update(int id, [FromBody] Doctor updatedDoctor)
        {
            if (id != updatedDoctor.Id)
                return BadRequest(new { Message = "Doctor ID mismatch." });

            try
            {
                _doctorService.Update(updatedDoctor);
                return Ok(new { Message = "Doctor updated successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while updating doctor.");
                return StatusCode(500, new { Error = "Failed to update doctor." });
            }
        }

        // ✅ 4. Ջնջել բժշկին (Admin-only)
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var doctor = await _doctorService.GetByIdAsync(id);
            if (doctor == null)
                return NotFound(new { Message = $"Doctor with id {id} not found." });

            try
            {
                _doctorService.Delete(doctor);
                return Ok(new { Message = "Doctor deleted successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while deleting doctor.");
                return StatusCode(500, new { Error = "Failed to delete doctor." });
            }
        }
        [HttpGet("by-speciality")]
        public async Task<IActionResult> GetBySpeciality(int clinicId, string speciality)
        {
            var doctors = await _doctorService.GetDoctorsBySpecialityAsync(clinicId, speciality);

            return Ok(doctors);
        }
    }
}
