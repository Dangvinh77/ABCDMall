using ABCDMall.Modules.Movies.Domain.Entities;
using Microsoft.EntityFrameworkCore;


namespace ABCDMall.Modules.Movies.Infrastructure.Persistence.Booking
{
    public class MoviesBookingDbContext:DbContext
    {
        public const string DefaultSchema = "movies";// Đặt tên schema mặc định cho tất cả các bảng trong DbContext này
        public MoviesBookingDbContext(DbContextOptions<MoviesBookingDbContext> options) : base(options)
        {
        }
        public DbSet<GuestCustomer> GuestCustomers => Set<GuestCustomer>();
        public DbSet<BookingHold> BookingHolds => Set<BookingHold>();
        public DbSet<BookingHoldSeat> BookingHoldSeats => Set<BookingHoldSeat>();
        public DbSet<Bookingg> Bookings => Set<Bookingg>();
        public DbSet<BookingItem> BookingItems => Set<BookingItem>();
        public DbSet<Ticket> Tickets => Set<Ticket>();
        public DbSet<Promotion> Promotions => Set<Promotion>();
        public DbSet<PromotionRule> PromotionRules => Set<PromotionRule>();
        public DbSet<PromotionRedemption> PromotionRedemptions => Set<PromotionRedemption>();
        public DbSet<SnackCombo> SnackCombos => Set<SnackCombo>();
        public DbSet<Payment> Payments => Set<Payment>();
        public DbSet<OutboxEvent> OutboxEvents => Set<OutboxEvent>();
        public DbSet<AuditLog> AuditLogs => Set<AuditLog>();


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema(DefaultSchema); // Áp dụng schema mặc định cho tất cả các bảng
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(MoviesBookingDbContext).Assembly); // Tự động áp dụng tất cả các cấu hình từ assembly
            base.OnModelCreating(modelBuilder);// Gọi phương thức gốc để đảm bảo các cấu hình mặc định của EF Core được áp dụng
        }
        }
}
