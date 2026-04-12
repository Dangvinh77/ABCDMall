using ABCDMall.Modules.Movies.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ABCDMall.Modules.Movies.Infrastructure.Persistence.Booking.Configurations
{
    public class GuestCustomerConfiguration:IEntityTypeConfiguration<GuestCustomer>
    {
        public void Configure(EntityTypeBuilder<GuestCustomer> builder)
        {
            builder.ToTable("GuestCustomers");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.FullName)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(x => x.Email)
                .HasMaxLength(200)
                .IsRequired();

            builder.Property(x => x.PhoneNumber)
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(x => x.CreatedAtUtc).IsRequired();
            builder.Property(x => x.UpdatedAtUtc).IsRequired();

            builder.HasIndex(x => x.Email);
            builder.HasIndex(x => x.PhoneNumber);
        }
    }
}
