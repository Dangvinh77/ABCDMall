using ABCDMall.Modules.Movies.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ABCDMall.Modules.Movies.Infrastructure.Persistence.Booking.Configurations;

public class MovieFeedbackConfiguration : IEntityTypeConfiguration<MovieFeedback>
{
    public void Configure(EntityTypeBuilder<MovieFeedback> builder)
    {
        builder.ToTable("MovieFeedbacks");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Comment)
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(x => x.DisplayName)
            .HasMaxLength(120);

        builder.Property(x => x.CreatedByEmail)
            .HasMaxLength(200);

        builder.Property(x => x.TagsJson)
            .HasColumnType("nvarchar(max)");

        builder.Property(x => x.ModeratedBy)
            .HasMaxLength(120);

        builder.Property(x => x.ModerationReason)
            .HasMaxLength(500);

        builder.HasIndex(x => new { x.MovieId, x.IsVisible, x.ModerationStatus, x.CreatedAtUtc });
        builder.HasIndex(x => new { x.MovieId, x.Rating, x.CreatedAtUtc });
        builder.HasIndex(x => x.BookingId);
        builder.HasIndex(x => x.ShowtimeId);
        builder.HasIndex(x => x.FeedbackRequestId)
            .HasFilter("[FeedbackRequestId] IS NOT NULL");

        builder.HasOne(x => x.Booking)
            .WithMany()
            .HasForeignKey(x => x.BookingId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.FeedbackRequest)
            .WithMany(x => x.Feedbacks)
            .HasForeignKey(x => x.FeedbackRequestId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
