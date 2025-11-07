using Microsoft.EntityFrameworkCore;
using Domain.Models; // սա քո Note class-ի namespace-ն է

namespace Healthcare.NoteConsumer;

public class AppDbContext : DbContext
{
    public DbSet<Note> Notes => Set<Note>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
}
