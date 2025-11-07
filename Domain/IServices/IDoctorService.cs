using Domain.DTOs;
using Domain.Models;

namespace Domain.IServices
{
    public interface IDoctorService
    {
        Task AddAsync(CreateDoctorDto entity);
        Task<Doctor?> GetByIdAsync(int id);
        Task<IEnumerable<Doctor>> GetAllAsync();
        void Update(Doctor entity);
        void Delete(Doctor entity);
        Task AddAvailabilitySlot(AvailabilitySlot entity);
        Task CancelAvailibiltySlot(int id);
        Task AddNoteForPatient(Note note);
        Task<IEnumerable<AvailabilitySlot>> GetScheduleAsync(int doctorId,DateTime? date = null,int page = 1,int pageSize = 5);

    }
}
