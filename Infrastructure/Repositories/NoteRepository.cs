using Domain.IRepository;
using Domain.Models;
using Infrastructure.DbContextFolder;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Repositories
{

    public class NoteRepository : GenericRepository<Note>,INoteRepository
    {
        public NoteRepository(ClinicDbContext context)
            : base(context)
        {
        }

        public async Task<IEnumerable<Note>> GetNotesByPatientAsync(int patientId)
        {
            return await _context.Notes
                .Include(n => n.Appointment)
                .ThenInclude(a => a.Doctor)
                .Where(n => n.Appointment.PatientId == patientId)
                .OrderByDescending(n => n.Appointment.StartUtc)
                .ToListAsync();
        }
    }
}

