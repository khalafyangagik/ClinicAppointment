using Domain.Models; // քո Note model-ը
using MassTransit;

namespace Healthcare.NoteConsumer;

public class NoteCreatedConsumer : IConsumer<NoteCreatedMessage>
{
    private readonly AppDbContext _db;

    public NoteCreatedConsumer(AppDbContext db)
    {
        _db = db;
    }

    public async Task Consume(ConsumeContext<NoteCreatedMessage> context)
    {
        var msg = context.Message;
        Console.WriteLine($"Received note for appointment {msg.AppointmentId}: {msg.Text}");

        var note = new Note
        {
            AppointmentId = msg.AppointmentId,
            Text = msg.Text,
            CreatedAtUtc = msg.CreatedAtUtc
        };

        _db.Notes.Add(note);
        await _db.SaveChangesAsync();

        Console.WriteLine("✅ Note saved to DB");
    }
}
