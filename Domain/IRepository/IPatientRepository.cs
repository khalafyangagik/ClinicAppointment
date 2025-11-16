using Domain.Models;

namespace Domain.IRepository
{
    public interface IPatientRepository : IRepository<Patient>
    {
        Task<Patient?> GetByUserIdAsync(int userId);

    }
}
