using Domain.DTOs;
using Domain.Models;

namespace Domain.IServices
{
   
    public interface IPatientService
    {
        // --- CRUD ---
        Task AddAsync(Patient patient);
        Task<Patient?> GetByIdAsync(int id);
        Task<IEnumerable<PatientDto>> GetAllAsync();
        void Update(Patient patient);
        void Delete(Patient patient);


        Task<IEnumerable<Appointment>> GetAppointmentsAsync(int patientId, int page = 1, int pageSize = 5);

        Task<IEnumerable<Note>> GetDoctorNotesAsync(int patientId);
    }
}

