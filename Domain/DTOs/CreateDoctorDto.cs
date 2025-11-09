using System.ComponentModel.DataAnnotations;

namespace Domain.DTOs
{
    public class CreateDoctorDto
    {
        public string FullName { get; set; } = string.Empty;
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        public string Speciality { get; set; } = string.Empty;
        public int ClinicId { get; set; }
    }
}
