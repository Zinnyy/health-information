using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace health_information.Models.HealthConfig
{
    public class AdminConfig : IEntityTypeConfiguration<Admin>
    {
        public void Configure(EntityTypeBuilder<Admin> builder)
        {
            builder.ToTable("Admin");

            builder.HasKey(u => u.Id);
            builder.Property(u => u.Email)
                   .IsRequired()
                   .HasMaxLength(256);
            builder.Property(u => u.Username)
                   .IsRequired()
                   .HasMaxLength(50);
            builder.Property(u => u.FullName)
                    .IsRequired()
                    .HasMaxLength(150);
            builder.Property(u => u.Password)
                    .IsRequired();
            builder.Property(n => n.PasswordSalt)
                    .IsRequired();
            builder.Property(u => u.Role)
                    .IsRequired()
                    .HasMaxLength(50).HasDefaultValue("Admin");

            builder.HasIndex(u => u.Email)
                   .IsUnique();
        }
    }
}

