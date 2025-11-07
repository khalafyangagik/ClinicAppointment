using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
   public class Appointment
    {
        public int Id { get; set; }
        public int DoctorId { get; set; }              // FK → Doctor
        public int PatientId { get; set; }             // FK → Patient
        public int? SlotId { get; set; }               // Optional FK → AvailabilitySlot
        public DateTime StartUtc { get; set; }
        public DateTime EndUtc { get; set; }
        public string Status { get; set; } = "Reserved";
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        // Navigation
        public Doctor Doctor { get; set; } = default!;
        public Patient Patient { get; set; } = default!;
        public AvailabilitySlot? Slot { get; set; }
        public Note? Note { get; set; }
        public ICollection<Reminder> Reminders { get; set; } = new List<Reminder>();
    }
}
