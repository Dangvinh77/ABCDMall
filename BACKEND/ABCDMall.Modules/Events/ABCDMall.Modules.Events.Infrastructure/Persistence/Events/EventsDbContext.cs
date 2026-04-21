using ABCDMall.Modules.Events.Domain.Entities;
using ABCDMall.Modules.Events.Infrastructure.Persistence.Events.Configurations;
using Microsoft.EntityFrameworkCore;

namespace ABCDMall.Modules.Events.Infrastructure.Persistence.Events;

public class EventsDbContext : DbContext
{
    public const string DefaultSchema = "events";

    public EventsDbContext(DbContextOptions<EventsDbContext> options) : base(options)
    {
    }

    public DbSet<Event> Events => Set<Event>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(DefaultSchema);
        modelBuilder.ApplyConfiguration(new EventConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}