using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTOs
{
    public class CreateNoteDto
    {
        public int AppointmentId { get; set; }      
        public string Text { get; set; } = string.Empty; 
    }
}
