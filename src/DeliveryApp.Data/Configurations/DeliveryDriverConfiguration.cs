
using DeliveryApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DeliveryApp.Data.Configurations;
public class DeliveryDriverConfiguration : IEntityTypeConfiguration<DeliveryDriver>
{
    public void Configure(EntityTypeBuilder<DeliveryDriver> builder)
    {
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Name).IsRequired().HasMaxLength(200);
        builder.Property(d => d.Phone).IsRequired().HasMaxLength(20);
        builder.Property(d => d.VehicleType).IsRequired();
        builder.Property(d => d.IsAvailable).IsRequired().HasDefaultValue(true);
        builder.Property(d => d.CreatedAt).IsRequired();

        builder.HasMany(d => d.Orders)
               .WithOne(o => o.DeliveryDriver)
               .HasForeignKey(o => o.DeliveryDriverId)
               .OnDelete(DeleteBehavior.SetNull);
    }
}
