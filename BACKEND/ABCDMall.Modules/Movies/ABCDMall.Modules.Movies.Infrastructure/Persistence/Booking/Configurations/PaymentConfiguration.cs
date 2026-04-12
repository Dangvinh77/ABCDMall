using ABCDMall.Modules.Movies.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ABCDMall.Modules.Movies.Infrastructure.Persistence.Booking.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Currency)
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(x => x.PaymentIntentId)
            .HasMaxLength(100);

        builder.Property(x => x.ProviderTransactionId)
            .HasMaxLength(100);

        builder.Property(x => x.FailureReason)
            .HasMaxLength(500);

        builder.Property(x => x.CallbackPayloadJson)
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.Amount)
            .HasColumnType("decimal(18,2)");

        builder.HasIndex(x => x.BookingId);
        builder.HasIndex(x => x.Status);

        builder.HasIndex(x => x.ProviderTransactionId)
            .IsUnique()
            .HasFilter("[ProviderTransactionId] IS NOT NULL");
    }
}
