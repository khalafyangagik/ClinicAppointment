using AutoMapper;
using Domain.DTOs;
using Domain.IRepository;
using Domain.Messages;
using Domain.Models;
using Infrastructure.DbContextFolder;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


[ApiController]
[Route("api/notes")]
public class DoctorNotesController : ControllerBase
{
    private readonly IPublishEndpoint _publish;
    private readonly IRepository<Note> _notes;
    private readonly IMapper _mapper;
    private readonly ClinicDbContext _context;
    private readonly ILogger<DoctorNotesController> _logger;


    public DoctorNotesController(IPublishEndpoint publish, IRepository<Note> notes,IMapper mapper, ClinicDbContext context, ILogger<DoctorNotesController> logger)
    {
        _publish = publish;
        _notes = notes;
        _mapper = mapper;
        _context = context;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] CreateNoteDto noteDto)
    {
        // Validate Appointment existence
        var appointmentExists = await _context.Appointments.AnyAsync(a => a.Id == noteDto.AppointmentId);
        if (!appointmentExists)
            return BadRequest("Appointment not found.");

        // Map + save
        var note = _mapper.Map<Note>(noteDto);
        note.CreatedAtUtc = DateTime.UtcNow;
        await _notes.AddAsync(note);

        // Publish
        var message = new NoteCreatedMessage(note.AppointmentId, note.Text, note.CreatedAtUtc);
        try
        {
            await _publish.Publish(message);
            _logger.LogInformation("✅ Published NoteCreatedMessage");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Failed to publish note message");
        }

        return Accepted(new { message = "✅ Note saved and published to RabbitMQ." });
    }
}
