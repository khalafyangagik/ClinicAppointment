using Domain.DTOs;
using Domain.IRepository;
using Domain.IServices;
using Domain.Models;
using Infrastructure.DbContextFolder;
using Microsoft.AspNetCore.Identity;

namespace Application.Services
{
    public class RegistrationService : IRegistrationService
    {
        private readonly ClinicDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IClinicRepository _clinicRepository;
        private readonly IDoctorRepository _doctorRepository;
        private readonly IPatientRepository _patientRepository;
        public RegistrationService(
            ClinicDbContext dbContext,
            UserManager<ApplicationUser> userManager, IClinicRepository clinicRepository, IDoctorRepository doctorRepository, IPatientRepository petientRepository)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _clinicRepository = clinicRepository;
            _doctorRepository = doctorRepository;
            _patientRepository = petientRepository;
        }

        // ✅ Register Doctor
        public async Task<(bool Success, string Message)> RegisterDoctorAsync(RegisterDoctorDto dto)
        {
            if (dto == null)
                return (false, "Invalid registration data.");

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var clinic = await _clinicRepository.GetByIdAsync(dto.ClinicId);
                if (clinic == null)
                    return (false, "Clinic not found.");

                var existingUser = await _userManager.FindByEmailAsync(dto.Email);
                if (existingUser != null)
                    return (false, "This email is already used.");

                var doctor = await _doctorRepository.GetUnassignedDoctorByClinicAsync(dto.ClinicId);

                if (doctor == null)
                    return (false, "Doctor not found for this clinic or already registered.");

                // Create AspNetUser
                var user = new ApplicationUser
                {
                    UserName = dto.Email,
                    Email = dto.Email,
                    EmailConfirmed = true
                };

                var createResult = await _userManager.CreateAsync(user, dto.Password);
                if (!createResult.Succeeded)
                    return (false, string.Join(", ", createResult.Errors.Select(e => e.Description)));

                // Add to role
                await _userManager.AddToRoleAsync(user, "Doctor");

                // Update doctor entity
                doctor.AppUserId = user.Id;
                doctor.IsApproved = true;
                await _dbContext.SaveChangesAsync();

                await transaction.CommitAsync();

                return (true, $"Doctor registered successfully at clinic '{clinic.Name}'.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, $"Error: {ex.Message}");
            }
        }

        // ✅ Register Patient
        public async Task<(bool Success, string Message)> RegisterPatientAsync(RegisterPatientDto dto)
        {
            if (dto == null)
                return (false, "Invalid registration data.");

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var existingUser = await _userManager.FindByEmailAsync(dto.Email);
                if (existingUser != null)
                    return (false, "A user with this email already exists.");

                var user = new ApplicationUser
                {
                    UserName = dto.Email,
                    Email = dto.Email,
                    EmailConfirmed = true
                };

                var createResult = await _userManager.CreateAsync(user, dto.Password);
                if (!createResult.Succeeded)
                    return (false, string.Join(", ", createResult.Errors.Select(e => e.Description)));

                await _userManager.AddToRoleAsync(user, "Patient");

                var patient = new Patient
                {
                    AppUserId = user.Id,
                    FullName = dto.FullName,
                    BirthDate = dto.BirthDate,
                    Phone = dto.Phone
                };

                await _patientRepository.AddAsync(patient);
                await _dbContext.SaveChangesAsync();

                await transaction.CommitAsync();

                return (true, "Patient registered successfully.");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return (false, $"Error: {ex.Message}");
            }
        }
    }
}
