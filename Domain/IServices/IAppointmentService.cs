using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.DTOs;
using Domain.Models;

namespace Domain.IServices
{
    public interface IAppointmentService
    {
        Task AddAsync(Appointment appointment);
        Task<Appointment?> GetAsync(int id);
        Task<IEnumerable<Appointment>> GetAllAsync();
        void Update(Appointment appointment);
        void Delete(Appointment appointment);

        // --- Business ---
        Task<(bool Success, string Message, Appointment? Appointment)> CreateAppointmentAsync(CreateAppointmentDto dto, int userId);
        Task<(bool Success, string Message)> CancelAppointmentAsync(int appointmentId, int userId);
        Task<IEnumerable<Appointment>> GetDoctorAppointmentsAsync(int userId, DateTime? date = null);
        Task<IEnumerable<Appointment>> GetPatientAppointmentsAsync(int userId);
        Task<(bool Success, string Message, Appointment? Updated)> UpdateAppointmentBySlotAsync(int appointmentId, int newSlotId);
}
}