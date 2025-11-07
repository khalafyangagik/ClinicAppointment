using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class AvailabilitySlot
    {
        public int Id { get; set; }
        public int DoctorId { get; set; }              // FK → Doctor
        public DateTime StartUtc { get; set; }
        public DateTime EndUtc { get; set; }
        public bool IsBooked { get; set; }
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        // Navigation
        public Doctor Doctor { get; set; } = default!;
    }
}
