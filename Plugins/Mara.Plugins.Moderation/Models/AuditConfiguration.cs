using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Mara.Common.ValueConverters;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Mara.Plugins.Moderation.Models
{
    public sealed class AuditConfiguration : IEntityTypeConfiguration<Audit>
    {
        public void Configure(EntityTypeBuilder<Audit> builder)
        {
            builder.Property(a => a.Source).HasConversion<SnowflakeToNumberConverter>();
            builder.Property(a => a.Target).HasConversion<SnowflakeToNumberConverter>();

            builder.Property(a => a.EventType).HasConversion<EnumToNumberConverter<EventType, int>>();

            builder.HasMany(a => a.AuditActions).WithOne(aa => aa.Audit);
        }
    }
}
