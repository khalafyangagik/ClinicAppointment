using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Messages
{
    public record NoteCreatedMessage(
     int AppointmentId,
     string Text,
     DateTime CreatedAtUtc
 );

}
