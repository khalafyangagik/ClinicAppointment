using AutoMapper;
using Domain.IServices;
using Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ClinicAppointment.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize] // default՝ պետք է լինի մուտքագրված
    public class PatientsController : ControllerBase
    {
        private readonly IPatientService _patientService;
        private readonly IMapper _mapper;

        public PatientsController(IPatientService patientService,IMapper mapper)
        {
            _patientService = patientService;
            _mapper = mapper;
        }


        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll()
        {
            var patients = await _patientService.GetAllAsync();
            return Ok(patients);
        }

        [HttpGet("{id:int}")]
        [Authorize(Roles = "Admin,Doctor,Patient")]
        public async Task<IActionResult> GetById(int id)
        {
            var patient = await _patientService.GetByIdAsync(id);
            if (patient == null)
                return NotFound($"Patient with ID {id} not found.");
            return Ok(patient);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] Domain.Models.Patient updated)
        {
            var existing = await _patientService.GetByIdAsync(id);
            if (existing == null)
                return NotFound($"Patient with ID {id} not found.");

            existing.FullName = updated.FullName;
            existing.Phone = updated.Phone;

            _patientService.Update(existing);
            return Ok(existing);
        }


        [HttpDelete("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var patient = await _patientService.GetByIdAsync(id);
            if (patient == null)
                return NotFound($"Patient with ID {id} not found.");

            _patientService.Delete(patient);
            return NoContent();
        }

        [HttpGet("{patientId:int}/appointments")]
        [Authorize(Roles = "Admin,Doctor,Patient")]
        public async Task<IActionResult> GetAppointments(
            int patientId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 5)
        {
            var appointments = await _patientService.GetAppointmentsAsync(patientId, page, pageSize);
            return Ok(appointments);
        }

        [HttpGet("{patientId:int}/notes")]
        [Authorize(Roles = "Admin,Doctor,Patient")]
        public async Task<IActionResult> GetDoctorNotes(int patientId)
        {
            var notes = await _patientService.GetDoctorNotesAsync(patientId);
            return Ok(notes);
        }
    }
}
