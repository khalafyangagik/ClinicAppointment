using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.Models;

namespace Domain.IRepository
{
    public interface IAppointmentRepository : IRepository<Appointment>
    {
        Task<Appointment?> GetWithDetailsAsync(int id);
        Task<IEnumerable<Appointment>> GetAllWithDetailsAsync();
        Task<IEnumerable<Appointment>> GetDoctorAppointmentsAsync(int doctorId, DateTime? date = null);
        Task<IEnumerable<Appointment>> GetPatientAppointmentsAsync(int patientId);
        Task<Appointment?> GetByIdAndPatientAsync(int appointmentId, int patientId);
        Task<Appointment?> GetWithSlotAndDoctorAsync(int appointmentId);
        Task<bool> DoctorHasOverlapAsync(int doctorId, DateTime startUtc, DateTime endUtc, int excludingAppointmentId = 0);
        Task<IEnumerable<Appointment>> GetAppointmentsByPatientPagedAsync(int patientId, int page = 1, int pageSize = 5);


    }
}
