using Application.Helpers;
using Application.Services;
using Domain.DTOs;
using Domain.IServices;
using Domain.Models;
using Infrastructure.DbContextFolder;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ClinicAppointment.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _config;
        private readonly IRegistrationService _registrationService;
        public AuthController(UserManager<ApplicationUser> userManager,IConfiguration config, IRegistrationService doctorService)
        {
            _userManager = userManager;
            _config = config;
            _registrationService = doctorService;
        }

        // LOGIN
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (dto == null)
                return BadRequest("Invalid request data.");

            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
                return Unauthorized("Invalid credentials");

            var roles = await _userManager.GetRolesAsync(user);
            var token = JwtHelper.CreateToken(user, roles, _config);

            return Ok(new
            {
                token,
                user = user.Email,
                roles
            });
        }


        [HttpPost("register-patient")]
        public async Task<IActionResult> RegisterPatient([FromBody] RegisterPatientDto dto)
        {
            var (success, message) = await _registrationService.RegisterPatientAsync(dto);
            if (!success) return BadRequest(message);
            return Ok(new { Message = message });
        }

        [HttpPost("register-doctor")]
        public async Task<IActionResult> RegisterDoctor([FromBody] RegisterDoctorDto dto)
        {
            var (success, message) = await _registrationService.RegisterDoctorAsync(dto);
            if (!success) return BadRequest(message);
            return Ok(new { Message = message });
        }
    }
}
