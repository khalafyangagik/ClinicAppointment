using Domain.DTOs;
using Domain.Models;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Domain.Messages;


[ApiController]
[Route("api/notes")]
public class DoctorNotesController : ControllerBase
{
    private readonly IPublishEndpoint _publish;

    public DoctorNotesController(IPublishEndpoint publish)
    {
        _publish = publish;
    }


    [HttpPost]
    public async Task<IActionResult> Post([FromBody] CreateNoteDto note)
    {
        var message = new NoteCreatedMessage(note.AppointmentId, note.Text, DateTime.UtcNow);

        // ուղարկում ենք RabbitMQ-ին
        await _publish.Publish(message);

        return Accepted(new { message = "Note sent to RabbitMQ" });
    }
}
