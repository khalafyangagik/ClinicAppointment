using Domain.Models;

namespace Domain.IRepository
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<Doctor> Doctors { get; }
        IRepository<Clinic> Clinics { get; }
        IRepository<Patient> Patients { get; }
        IRepository<Appointment> Appointments { get; }
        IRepository<AvailabilitySlot> Slots { get; }

        Task<int> SaveChangesAsync();
        Task BeginTransactionAsync();
        Task CommitAsync();
        Task RollbackAsync();
    }
}
