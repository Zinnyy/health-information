using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace health_information.Models.HealthConfig
{
    public class VitalRecordConfig : IEntityTypeConfiguration<VitalRecord>
    {
        public void Configure(EntityTypeBuilder<VitalRecord> builder)
        {
            builder.ToTable("VitalRecords");
            builder.HasKey(v => v.Id);

            builder.Property(v => v.DateRecorded)
                   .IsRequired();
            builder.Property(v => v.HeartRate)
                   .IsRequired();
            builder.Property(v => v.SystolicBp)
                   .IsRequired();
            builder.Property(v => v.Glucose)
                   .IsRequired();

            builder.HasOne(v => v.Patient)
                   .WithMany(p => p.VitalRecords)
                   .HasForeignKey(v => v.PatientId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
