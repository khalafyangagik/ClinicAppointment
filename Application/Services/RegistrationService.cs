using Domain.IServices;
using Domain.DTOs;
using Domain.Models;
using Infrastructure.DbContextFolder;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public class RegistrationService : IRegistrationService
    {
        private readonly ClinicDbContext _dbContext;
        private readonly UserManager<ApplicationUser> _userManager;

        public RegistrationService(
            ClinicDbContext dbContext,
            UserManager<ApplicationUser> userManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
        }

        // ✅ Register Doctor
        public async Task<(bool Success, string Message)> RegisterDoctorAsync(RegisterDoctorDto dto)
        {
            if (dto == null)
                return (false, "Invalid registration data.");

            await using var transaction = await _dbContext.Database.BeginTransactionAsync();

            try
            {
                var clinic = await _dbContext.Clinics.FindAsync(dto.ClinicId);
                if (clinic == null)
                    return (false, "Clinic not found.");

                var existingUser = await _userManager.FindByEmailAsync(dto.Email);
                if (existingUser != null)
                    return (false, "This email is already used.");

                var doctor = await _dbContext.Doctors
                    .FirstOrDefaultAsync(d => d.AppUserId == null && d.ClinicId == dto.ClinicId);

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

                _dbContext.Patients.Add(patient);
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
