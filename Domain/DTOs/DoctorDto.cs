using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTOs
{

    public class DoctorDto
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Speciality { get; set; } = string.Empty;
        public string ClinicName { get; set; } = string.Empty;
    }
}
