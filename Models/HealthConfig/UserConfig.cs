using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace health_information.Models.HealthConfig
{
    public class UserConfig : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users");

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
                    .HasMaxLength(50).HasDefaultValue("ResearchAssistant");

            builder.HasIndex(u => u.Email)
                   .IsUnique();
        }
    }
}
