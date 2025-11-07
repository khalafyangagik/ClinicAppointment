using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class Patient
    {
        public int Id { get; set; }
        public int AppUserId { get; set; }             // FK դեպի AspNetUsers.Id
        public string FullName { get; set; } = string.Empty;
        public DateTime BirthDate { get; set; }
        public string Phone { get; set; } = string.Empty;

        // Navigation
        public ApplicationUser AppUser { get; set; } = default!;
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();

        // Implicit many-to-many (no explicit join table)
        public ICollection<Doctor> Doctors { get; set; } = new List<Doctor>();
    }
}
