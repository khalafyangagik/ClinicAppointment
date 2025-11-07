using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class Note
    {
        public int AppointmentId { get; set; }         // PK + FK → Appointment
        public string Text { get; set; } = string.Empty;
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        // Navigation
        public Appointment Appointment { get; set; } = default!;
    }
}
