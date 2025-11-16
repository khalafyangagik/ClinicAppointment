using Domain.Models;

namespace Domain.IRepository
{
    public interface IDoctorRepository : IRepository<Doctor>
    {
        Task<IEnumerable<Doctor>> GetDoctorsWithClinicAsync();
        Task<Doctor?> GetByIdWithClinicAsync(int id);
        Task<Doctor?> GetByUserIdAsync(int userId);
        public Task<IEnumerable<Doctor>> GetByClinicAndSpecialityAsync(int clinicId, string speciality);
        Task<Doctor?> GetUnassignedDoctorByClinicAsync(int clinicId);

    }
}
