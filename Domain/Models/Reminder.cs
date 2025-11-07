using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class Reminder
    {
        public int Id { get; set; }
        public int AppointmentId { get; set; }         // FK → Appointment
        public DateTime SendAtUtc { get; set; }
        public string Status { get; set; } = "Pending";
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

        // Navigation
        public Appointment Appointment { get; set; } = default!;
    }
}
