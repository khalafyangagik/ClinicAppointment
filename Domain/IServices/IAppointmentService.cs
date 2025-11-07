using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Models;

namespace Domain.IServices
{
    public interface IAppointmentService
    {
        // --- CRUD ---
        Task AddAsync(Appointment appointment);
        Task<Appointment?> GetAsync(int id);
        Task<IEnumerable<Appointment>> GetAllAsync();
        void Update(Appointment appointment);
        void Delete(Appointment appointment);

    
        Task<Appointment> BookAppointmentAsync(int doctorId, int patientId, int slotId);

        Task CancelAppointmentAsync(int appointmentId);

        Task<IEnumerable<Appointment>> GetByDoctorAsync(int doctorId, DateTime? date = null);
        Task<IEnumerable<Appointment>> GetByPatientAsync(int patientId);
    }
}
