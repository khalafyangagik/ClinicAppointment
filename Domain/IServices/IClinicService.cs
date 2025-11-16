using Domain.DTOs;
using Domain.Models;

namespace Domain.IServices
{
    public interface IClinicService
    {
        Task AddAsync(CreateClinicDto entity);
        Task<Clinic?> GetByIdAsync(int id);
        Task<IEnumerable<Clinic>> GetAllAsync();
        Task UpdateAsync(Clinic entity);
        Task DeleteAsync(Clinic entity);

    }
}
