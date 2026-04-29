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
    public DbSet<EventRegistration> EventRegistrations => Set<EventRegistration>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(DefaultSchema);
        modelBuilder.ApplyConfiguration(new EventConfiguration());
        modelBuilder.ApplyConfiguration(new EventRegistrationConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}