using MassTransit;
using Microsoft.EntityFrameworkCore;
using Healthcare.NoteConsumer;

IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        // ⛳️ կարդում ենք connection string-ը ENV-ից
        var conn = Environment.GetEnvironmentVariable("ConnectionStrings__Default")
                   ?? "Server=localhost;Database=HealthcareDB;Trusted_Connection=True;TrustServerCertificate=True;";

        // ⛳️ կարդում ենք RabbitMQ-ի host-ը ENV-ից
        var rabbitHost = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost";

        services.AddDbContext<AppDbContext>(opt => opt.UseSqlServer(conn));

        services.AddMassTransit(x =>
        {
            x.AddConsumer<NoteCreatedConsumer>();

            x.UsingRabbitMq((ctx, cfg) =>
            {
                cfg.Host(rabbitHost, "/", h =>
                {
                    h.Username("guest");
                    h.Password("guest");
                });

                cfg.ReceiveEndpoint("note-created-queue", e =>
                {
                    // 👇 Ասում ենք MassTransit-ին՝ կապիր այս queue-ն publisher-ի exchange-ին
                    e.Bind("Domain.Messages:NoteCreatedMessage");

                    // 👇 Ասում ենք՝ այս consumer-ը թող լսի տվյալ queue-ից եկող մեսիջները
                    e.ConfigureConsumer<NoteCreatedConsumer>(ctx);
                });
            });
        });
    })
    .Build();

await host.RunAsync();
