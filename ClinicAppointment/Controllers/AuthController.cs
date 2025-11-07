using Application.Helpers;
using Domain.DTOs;
using Domain.Models;
using Infrastructure.DbContextFolder;
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
        private readonly RoleManager<IdentityRole<int>> _roleManager;
        private readonly ClinicDbContext _dbContext;
        private readonly IConfiguration _config;

        public AuthController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole<int>> roleManager,
            ClinicDbContext dbContext,
            IConfiguration config)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _dbContext = dbContext;
            _config = config;
        }

        // ✅ LOGIN
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (dto == null)
                return BadRequest("Invalid request data.");

            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
                return Unauthorized("Invalid credentials");

            // բերում ենք roles-ը
            var roles = await _userManager.GetRolesAsync(user);

            // Եթե բժիշկ է՝ ստուգում ենք approval-ը
          /*  if (roles.Contains("Doctor"))
            {
                var doctor = await _dbContext.Doctors.FirstOrDefaultAsync(d => d.AppUserId == user.Id);
                if (doctor is not null && !doctor.IsApproved)
                    return Unauthorized("Your account is pending admin approval.");
            }*/

            // ստեղծում ենք JWT token
            var token = JwtHelper.CreateToken(user, roles, _config);

            return Ok(new
            {
                token,
                user = user.Email,
                roles
            });
        }

        // ✅ DOCTOR REGISTRATION
        [HttpPost("register-doctor")]
        public async Task<IActionResult> RegisterDoctor([FromBody] RegisterDoctorDto dto)
        {
            if (dto == null)
                return BadRequest("Invalid registration data.");

            // 1️⃣ Ստուգում ենք Clinic-ը
            var clinic = await _dbContext.Clinics.FindAsync(dto.ClinicId);
            if (clinic == null)
                return BadRequest("Clinic not found.");

            var appUser = await _userManager.FindByEmailAsync(dto.Email);
            if (appUser != null)
                return BadRequest("This email is already used.");

            var doctor = await _dbContext.Doctors
                .Include(d => d.AppUser)
                .FirstOrDefaultAsync(d => d.AppUserId == null && d.ClinicId == dto.ClinicId);

            if (doctor == null)
                return BadRequest("Doctor not found for this clinic or already registered.");

            if (doctor.AppUserId != null)
                return BadRequest("This doctor is already registered.");

            // 3️⃣ Ստեղծում ենք AspNetUser
            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
                return BadRequest(result.Errors);

            // 4️⃣ Ավելացնում ենք Doctor role-ը
            await _userManager.AddToRoleAsync(user, "Doctor");

            // 5️⃣ Թարմացնում ենք Doctor աղյուսակը
            doctor.AppUserId = user.Id;
            doctor.IsApproved = true;
            await _dbContext.SaveChangesAsync();

            return Ok(new
            {
                Message = "Doctor registered successfully..",
                Clinic = clinic.Name,
                Speciality = doctor.Speciality
            });
        }

            // ✅ PATIENT REGISTRATION
            [HttpPost("register-patient")]
            public async Task<IActionResult> RegisterPatient([FromBody] RegisterPatientDto dto)
            {
                if (dto == null)
                    return BadRequest("Invalid registration data.");

                // 1️⃣ Ստուգում ենք՝ արդյոք user արդեն գոյություն ունի
                var existingUser = await _userManager.FindByEmailAsync(dto.Email);
                if (existingUser != null)
                    return BadRequest("A user with this email already exists.");

                // 2️⃣ Ստեղծում ենք AspNetUser
                var user = new ApplicationUser
                {
                    UserName = dto.Email,
                    Email = dto.Email,
                    EmailConfirmed = true
                };

                var result = await _userManager.CreateAsync(user, dto.Password);
                if (!result.Succeeded)
                    return BadRequest(result.Errors);

                // 3️⃣ Ավելացնում ենք Patient role-ը
                await _userManager.AddToRoleAsync(user, "Patient");
             

                // 4️⃣ Ավելացնում ենք Patient աղյուսակում
                var patient = new Patient
                {
                    AppUserId = user.Id,
                    FullName = dto.FullName,
                    BirthDate = dto.BirthDate,
                    Phone = dto.Phone
                };

                _dbContext.Patients.Add(patient);
                await _dbContext.SaveChangesAsync();

                // 5️⃣ Ստեղծում ենք JWT Token
                var roles = await _userManager.GetRolesAsync(user);
                var token = JwtHelper.CreateToken(user, roles, _config);

                return Ok(new
                {
                    Message = "Patient registered successfully.",
                    Token = token
                });
            }

        }
    }

