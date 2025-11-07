using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Models
{
    public class Doctor
    {
        public int Id { get; set; }
        public int? AppUserId { get; set; }             // FK դեպի AspNetUsers.Id
        public int ClinicId { get; set; }              // FK դեպի Clinic.Id
        public string FullName { get; set; } = string.Empty;
        public string Speciality { get; set; } = string.Empty;
        public bool IsApproved { get; set; } = false;

        // Navigation
        public ApplicationUser? AppUser { get; set; } = default!;
        public Clinic Clinic { get; set; } = default!;
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
        public ICollection<AvailabilitySlot> AvailabilitySlots { get; set; } = new List<AvailabilitySlot>();

        // Implicit many-to-many (no explicit join table)
        public ICollection<Patient> Patients { get; set; } = new List<Patient>();
    }
}
