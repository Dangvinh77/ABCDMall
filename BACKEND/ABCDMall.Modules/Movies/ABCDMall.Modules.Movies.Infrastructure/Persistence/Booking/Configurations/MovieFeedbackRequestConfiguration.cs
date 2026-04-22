using ABCDMall.Modules.Movies.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ABCDMall.Modules.Movies.Infrastructure.Persistence.Booking.Configurations;

public class MovieFeedbackRequestConfiguration : IEntityTypeConfiguration<MovieFeedbackRequest>
{
    public void Configure(EntityTypeBuilder<MovieFeedbackRequest> builder)
    {
        builder.ToTable("MovieFeedbackRequests");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.PurchaserEmail)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.TokenHash)
            .HasMaxLength(128);

        builder.Property(x => x.LastEmailError)
            .HasColumnType("nvarchar(max)");

        builder.HasIndex(x => new { x.BookingId, x.ShowtimeId }).IsUnique();
        builder.HasIndex(x => x.MovieId);
        builder.HasIndex(x => new { x.Status, x.AvailableAtUtc });
        builder.HasIndex(x => x.ExpiresAtUtc);
        builder.HasIndex(x => x.TokenHash)
            .IsUnique()
            .HasFilter("[TokenHash] IS NOT NULL");

        builder.HasOne(x => x.Booking)
            .WithMany()
            .HasForeignKey(x => x.BookingId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
