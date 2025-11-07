using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.DTOs
{
    public class CreateAppointmentDto
    {
        public int SlotId { get; set; }    
        public DateTime StartUtc { get; set; }      // patient-ի ընտրած սկիզբ
        public DateTime EndUtc { get; set; }        // patient-ի ընտրած վերջ
    }
}
