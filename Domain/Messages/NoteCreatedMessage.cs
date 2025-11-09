namespace Domain.Messages
{
    public record NoteCreatedMessage(
     int AppointmentId,
     string Text,
     DateTime CreatedAtUtc
 );

}
