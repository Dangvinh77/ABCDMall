using ABCDMall.Modules.Users.Infrastructure.Persistence.Configurations;
using ABCDMall.Modules.Users.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ABCDMall.Modules.Users.Infrastructure
{
    public class MallDbContext : DbContext
    {
        public MallDbContext(DbContextOptions<MallDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
        public DbSet<PasswordResetOtp> PasswordResetOtps => Set<PasswordResetOtp>();
        public DbSet<ForgotPasswordOtp> ForgotPasswordOtps => Set<ForgotPasswordOtp>();
        public DbSet<ProfileUpdateHistory> ProfileUpdateHistories => Set<ProfileUpdateHistory>();
        public DbSet<ShopInfo> ShopInfos => Set<ShopInfo>();
        public DbSet<RentalArea> RentalAreas => Set<RentalArea>();
        public DbSet<ShopMonthlyBill> ShopMonthlyBills => Set<ShopMonthlyBill>();
        public DbSet<PublicShop> PublicShops => Set<PublicShop>();
        public DbSet<PublicShopProduct> PublicShopProducts => Set<PublicShopProduct>();
        public DbSet<PublicShopVoucher> PublicShopVouchers => Set<PublicShopVoucher>();
        public DbSet<CarouselBid> CarouselBids => Set<CarouselBid>();
        public DbSet<MovieCarouselAd> MovieCarouselAds => Set<MovieCarouselAd>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration(new UserConfiguration());
            modelBuilder.ApplyConfiguration(new RefreshTokenConfiguration());
            modelBuilder.ApplyConfiguration(new PasswordResetOtpConfiguration());
            modelBuilder.ApplyConfiguration(new ForgotPasswordOtpConfiguration());
            modelBuilder.ApplyConfiguration(new ProfileUpdateHistoryConfiguration());
            modelBuilder.ApplyConfiguration(new RentalAreaConfiguration());
            modelBuilder.ApplyConfiguration(new ShopInfoConfiguration());
            modelBuilder.ApplyConfiguration(new ShopMonthlyBillConfiguration());
            modelBuilder.ApplyConfiguration(new PublicShopConfiguration());
            modelBuilder.ApplyConfiguration(new PublicShopProductConfiguration());
            modelBuilder.ApplyConfiguration(new PublicShopVoucherConfiguration());
            modelBuilder.ApplyConfiguration(new CarouselBidConfiguration());
            modelBuilder.ApplyConfiguration(new MovieCarouselAdConfiguration());
            base.OnModelCreating(modelBuilder);
        }
    }
}
