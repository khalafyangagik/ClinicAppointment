using Domain.IRepository;
using Domain.IServices;
using Domain.Models;
using Infrastructure.DbContextFolder;
using Microsoft.EntityFrameworkCore;


namespace Application.Services
    {
        public class AppointmentService : IAppointmentService
        {
            private readonly IUnitOfWork _unitOfWork;
            private readonly IRepository<Appointment> _appointmentRepo;
            private readonly IRepository<AvailabilitySlot> _slotRepo;
            private readonly ClinicDbContext _dbContext;

            public AppointmentService(
                IUnitOfWork unitOfWork,
                IRepository<Appointment> appointmentRepo,
                IRepository<AvailabilitySlot> slotRepo,
                ClinicDbContext context)
            {
                _unitOfWork = unitOfWork;
                _appointmentRepo = appointmentRepo;
                _slotRepo = slotRepo;
                _dbContext = context;
            }

            // ----------------- CRUD -----------------

            public async Task AddAsync(Appointment appointment)
            {
                // ✅ Ստուգում ենք՝ արդյոք slot-ը ազատ է
                var slot = await _slotRepo.GetByIdAsync(appointment.SlotId ?? 0);
                if (slot == null)
                    throw new KeyNotFoundException("Slot not found.");

                if (slot.IsBooked)
                    throw new InvalidOperationException("This slot is already booked.");

                slot.IsBooked = true;
                await _appointmentRepo.AddAsync(appointment);
                await _unitOfWork.SaveChangesAsync();
            }

            public async Task<Appointment?> GetAsync(int id)
            {
                return await _appointmentRepo.GetByIdAsync(id);
            }

            public async Task<IEnumerable<Appointment>> GetAllAsync()
            {
                return await _appointmentRepo.GetAllAsync();
            }

            public void Update(Appointment appointment)
            {
                _appointmentRepo.Update(appointment);
            }

            public void Delete(Appointment appointment)
            {
                _appointmentRepo.Delete(appointment);
            }

            // ----------------- ԲԻԶՆԵՍ ԳՈՐԾՈՂՈՒԹՅՈՒՆՆԵՐ -----------------

            public async Task<Appointment> BookAppointmentAsync(int doctorId, int patientId, int slotId)
            {
                await _unitOfWork.BeginTransactionAsync();
                try
                {
                    // 1️⃣ Ստուգում ենք՝ slot-ը գոյություն ունի՞
                    var slot = await _slotRepo.GetByIdAsync(slotId);
                    if (slot == null)
                        throw new KeyNotFoundException("Slot not found.");

                    // 2️⃣ Ստուգում ենք՝ արդյոք զբաղված է
                    if (slot.IsBooked)
                        throw new InvalidOperationException("This slot is already booked.");

                    // 3️⃣ Ստեղծում ենք appointment
                    var appointment = new Appointment
                    {
                        DoctorId = doctorId,
                        PatientId = patientId,
                        SlotId = slotId,
                        StartUtc = slot.StartUtc,
                        EndUtc = slot.EndUtc,
                        Status = "Reserved",
                        CreatedAtUtc = DateTime.UtcNow
                    };

                    await _appointmentRepo.AddAsync(appointment);

                    // 4️⃣ Թարմացնում ենք slot-ը
                    slot.IsBooked = true;
                    _slotRepo.Update(slot);

                    // 5️⃣ Պահպանում ենք ամեն ինչ մեկ transaction-ի մեջ
                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitAsync();

                    return appointment;
                }
                catch
                {
                    await _unitOfWork.RollbackAsync();
                    throw;
                }
            }

            public async Task CancelAppointmentAsync(int appointmentId)
            {
                await _unitOfWork.BeginTransactionAsync();
                try
                {
                    // 1️⃣ Գտնում ենք appointment-ը
                    var appointment = await _appointmentRepo.GetByIdAsync(appointmentId);
                    if (appointment == null)
                        throw new KeyNotFoundException("Appointment not found.");

                    // 2️⃣ Գտնում ենք slot-ը
                    if (appointment.SlotId.HasValue)
                    {
                        var slot = await _slotRepo.GetByIdAsync(appointment.SlotId.Value);
                        if (slot != null)
                        {
                            slot.IsBooked = false;
                            _slotRepo.Update(slot);
                        }
                    }

                    // 3️⃣ Ջնջում ենք appointment-ը
                    _appointmentRepo.Delete(appointment);

                    // 4️⃣ Պահպանում ենք transaction-ը
                    await _unitOfWork.SaveChangesAsync();
                    await _unitOfWork.CommitAsync();
                }
                catch
                {
                    await _unitOfWork.RollbackAsync();
                    throw;
                }
            }

            public async Task<IEnumerable<Appointment>> GetByDoctorAsync(int doctorId, DateTime? date = null)
            {
                var query = _dbContext.Appointments
                                      .Where(a => a.DoctorId == doctorId);

                if (date.HasValue)
                    query = query.Where(a => a.StartUtc.Date == date.Value.Date);

                return await query
                             .Include(a => a.Patient)
                             .Include(a => a.Slot)
                             .OrderBy(a => a.StartUtc)
                             .ToListAsync();
            }

            public async Task<IEnumerable<Appointment>> GetByPatientAsync(int patientId)
            {
                var query = _dbContext.Appointments
                                      .Where(a => a.PatientId == patientId)
                                      .Include(a => a.Doctor)
                                      .Include(a => a.Slot)
                                      .OrderByDescending(a => a.StartUtc);

                return await query.ToListAsync();
            }
        }
    }
