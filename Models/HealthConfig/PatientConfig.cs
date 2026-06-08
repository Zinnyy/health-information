using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace health_information.Models.HealthConfig
{
    public class PatientConfig : IEntityTypeConfiguration<Patient>
    {
        public void Configure(EntityTypeBuilder<Patient> builder)
        {
            builder.ToTable("Patients");
            builder.HasKey(p => p.Id);

            builder.Property(p => p.PatientStudyId)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(p => p.Age)
                   .IsRequired();

            builder.Property(p => p.Gender)
                .IsRequired()
                .HasMaxLength(20);



            builder.HasOne(p => p.CreatedBy)
                .WithMany(u => u.Patients)
                .HasForeignKey(u => u.CreatedById)
                .OnDelete(DeleteBehavior.Restrict);

        }
    }
}
