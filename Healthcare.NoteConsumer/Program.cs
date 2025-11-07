using MassTransit;
using Microsoft.EntityFrameworkCore;
using Healthcare.NoteConsumer;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        // SQL Server connection string (փոխիր քո տվյալներով)
        var conn = "Server=localhost;Database=HealthcareDB;Trusted_Connection=True;TrustServerCertificate=True;";
        services.AddDbContext<AppDbContext>(opt => opt.UseSqlServer(conn));

        services.AddMassTransit(x =>
        {
            x.AddConsumer<NoteCreatedConsumer>();

            x.UsingRabbitMq((ctx, cfg) =>
            {
                cfg.Host("localhost", "/", h =>
                {
                    h.Username("guest");
                    h.Password("guest");
                });

                cfg.ReceiveEndpoint("note-created-queue", e =>
                {
                    e.ConfigureConsumer<NoteCreatedConsumer>(ctx);
                });
            });
        });
    })
    .Build();

await host.RunAsync();