using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Domain.IServices;
using Domain.Models;
using Infrastructure.DbContextFolder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Application.Services
{
    public class SlotGeneratorService : ISlotGeneratorService
    {
        private readonly ClinicDbContext _dbContext;
        private readonly ILogger<SlotGeneratorService> _logger;


        public SlotGeneratorService(ClinicDbContext dbContext, ILogger<SlotGeneratorService> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        public async Task GenerateSlotsAsync(int doctorId, DateTime startUtc, DateTime endUtc)
        {
            var slots = new List<AvailabilitySlot>();

            DateTime current = startUtc;
            while (current < endUtc)
            {
                var next = current.AddMinutes(30); // 30 րոպեանոց կտոր
                slots.Add(new AvailabilitySlot
                {
                    DoctorId = doctorId,
                    StartUtc = current,
                    EndUtc = next,
                    IsBooked = false
                });
                current = next;
            }

            await _dbContext.AvailabilitySlots.AddRangeAsync(slots);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<(bool Success, string Message, object? Data)>
            GetAvailableSlotsAsync(int doctorId, DateTime date)
        {
            try
            {
                // 🔹 Վերցնում ենք բժիշկը՝ որպեսզի ունենանք անունը
                var doctor = await _dbContext.Doctors
                    .Include(d => d.AppUser)
                    .FirstOrDefaultAsync(d => d.Id == doctorId);

                if (doctor == null)
                    return (false, "Doctor not found.", null);

                // 🔹 Բոլոր slot-երը տվյալ բժշկի համար
                var slots = await _dbContext.AvailabilitySlots
                    .Where(s => s.DoctorId == doctorId && s.StartUtc.Date == date.Date)
                    .OrderBy(s => s.StartUtc)
                    .ToListAsync();

                if (!slots.Any())
                    return (false, "Այս բժշկի համար ազատ ժամեր չեն գտնվել տվյալ օրը։", null);

                // 🔹 Բոլոր արդեն զբաղված ժամերը
                var appointments = await _dbContext.Appointments
                    .Where(a => a.DoctorId == doctorId && a.StartUtc.Date == date.Date)
                    .Select(a => new { a.StartUtc, a.EndUtc })
                    .ToListAsync();

                var availableTimes = new List<object>();

                foreach (var slot in slots)
                {
                    DateTime current = slot.StartUtc;
                    while (current < slot.EndUtc)
                    {
                        var next = current.AddMinutes(30);

                        bool isBusy = appointments.Any(a =>
                            (current >= a.StartUtc && current < a.EndUtc) ||
                            (next > a.StartUtc && next <= a.EndUtc));

                        if (!isBusy)
                        {
                            availableTimes.Add(new
                            {
                                SlotId = slot.Id,
                                TimeRange = $"{current:HH\\:mm} - {next:HH\\:mm}"
                            });
                        }

                        current = next;
                    }
                }

                return (true, "Available slots fetched successfully.", new
                {
                    DoctorId = doctor.Id,
                    DoctorName = doctor.AppUser?.UserName ?? "Unknown",
                    Speciality = doctor.Speciality,
                    Date = date.ToString("dd.MM.yyyy"),
                    AvailableSlots = availableTimes
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while fetching available slots.");
                return (false, "An error occurred while fetching available slots.", null);
            }
        }
    }


}

