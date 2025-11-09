using Domain.DTOs;

namespace Domain.IServices
{
    public interface IRegistrationService
    {
        Task<(bool Success, string Message)> RegisterDoctorAsync(RegisterDoctorDto dto);
        Task<(bool Success,string Message)> RegisterPatientAsync(RegisterPatientDto dto);
    }
}
