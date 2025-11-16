using Domain.Models;

namespace Domain.IRepository
{
    public interface INoteRepository : IRepository<Note>
    {
        Task<IEnumerable<Note>> GetNotesByPatientAsync(int patientId);

    }
}
