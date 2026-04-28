using ABCDMall.Modules.Movies.Domain.Entities;
using ABCDMall.Modules.Movies.Infrastructure.Persistence.Booking.Configurations;
using Microsoft.EntityFrameworkCore;

namespace ABCDMall.Modules.Movies.Infrastructure.Persistence.Booking;

public class MoviesBookingDbContext : DbContext
{
    public MoviesBookingDbContext(DbContextOptions<MoviesBookingDbContext> options) : base(options)
    {
    }

    public DbSet<GuestCustomer> GuestCustomers => Set<GuestCustomer>();
    public DbSet<BookingHold> BookingHolds => Set<BookingHold>();
    public DbSet<BookingHoldSeat> BookingHoldSeats => Set<BookingHoldSeat>();
    public DbSet<Booking> Bookings => Set<Booking>();
    public DbSet<BookingItem> BookingItems => Set<BookingItem>();
    public DbSet<Ticket> Tickets => Set<Ticket>();
    public DbSet<Promotion> Promotions => Set<Promotion>();
    public DbSet<PromotionRule> PromotionRules => Set<PromotionRule>();
    public DbSet<PromotionRedemption> PromotionRedemptions => Set<PromotionRedemption>();
    public DbSet<SnackCombo> SnackCombos => Set<SnackCombo>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<OutboxEvent> OutboxEvents => Set<OutboxEvent>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<MovieFeedbackRequest> MovieFeedbackRequests => Set<MovieFeedbackRequest>();
    public DbSet<MovieFeedback> MovieFeedbacks => Set<MovieFeedback>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new GuestCustomerConfiguration());
        modelBuilder.ApplyConfiguration(new BookingHoldConfiguration());
        modelBuilder.ApplyConfiguration(new BookingHoldSeatConfiguration());
        modelBuilder.ApplyConfiguration(new BookingConfiguration());
        modelBuilder.ApplyConfiguration(new BookingItemConfiguration());
        modelBuilder.ApplyConfiguration(new TicketConfiguration());
        modelBuilder.ApplyConfiguration(new PromotionConfiguration());
        modelBuilder.ApplyConfiguration(new PromotionRuleConfiguration());
        modelBuilder.ApplyConfiguration(new PromotionRedemptionConfiguration());
        modelBuilder.ApplyConfiguration(new SnackComboConfiguration());
        modelBuilder.ApplyConfiguration(new PaymentConfiguration());
        modelBuilder.ApplyConfiguration(new OutboxEventConfiguration());
        modelBuilder.ApplyConfiguration(new AuditLogConfiguration());
        modelBuilder.ApplyConfiguration(new MovieFeedbackRequestConfiguration());
        modelBuilder.ApplyConfiguration(new MovieFeedbackConfiguration());
        base.OnModelCreating(modelBuilder);
    }
}
