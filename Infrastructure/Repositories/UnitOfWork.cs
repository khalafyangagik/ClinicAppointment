using Domain.IRepository;
using Domain.Models;
using Infrastructure.DbContextFolder;
using Microsoft.EntityFrameworkCore.Storage;

namespace Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ClinicDbContext _context;
        private IDbContextTransaction? _transaction;

        public IRepository<Doctor> Doctors { get; }
        public IRepository<Clinic> Clinics { get; }
        public IRepository<Patient> Patients { get; }
        public IRepository<Appointment> Appointments { get; }
        public IRepository<AvailabilitySlot> Slots { get; }

        public UnitOfWork(
            ClinicDbContext context,
            IRepository<Doctor> doctorRepo,
            IRepository<Clinic> clinicRepo,
            IRepository<Patient> patientRepo,
            IRepository<Appointment> appointmentRepo,
            IRepository<AvailabilitySlot> slotRepo)
        {
            _context = context;
            Doctors = doctorRepo;
            Clinics = clinicRepo;
            Patients = patientRepo;
            Appointments = appointmentRepo;
            Slots = slotRepo;
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task BeginTransactionAsync()
        {
            _transaction = await _context.Database.BeginTransactionAsync();
        }

        public async Task CommitAsync()
        {
            if (_transaction != null)
                await _transaction.CommitAsync();
        }

        public async Task RollbackAsync()
        {
            if (_transaction != null)
                await _transaction.RollbackAsync();
        }

        public void Dispose()
        {
            _transaction?.Dispose();
            _context.Dispose();
        }
    }
}

