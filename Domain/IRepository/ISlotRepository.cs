using Domain.Models;

namespace Domain.IRepository
{
    public interface ISlotRepository : IRepository<AvailabilitySlot>
    {
        Task<IEnumerable<AvailabilitySlot>> GetAllWithDoctorAsync();
        Task<AvailabilitySlot?> GetByIdWithDoctorAsync(int id);
        Task<IEnumerable<AvailabilitySlot>> GetScheduleAsync(
        int doctorId,
        DateTime? date = null,
        int page = 1,
        int pageSize = 5);

    }
}
