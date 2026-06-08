using System.ComponentModel.DataAnnotations;

namespace health_information.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [EmailAddress]
        public string? Email { get; set; }
        [Required]
        public string Username { get; set; } = string.Empty;
        [Required]
        public string? Password { get; set; }
        [Required]
        public string PasswordSalt { get; set; } = string.Empty; // Holds the Base64 Salt
        [Required]
        public string? FullName { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        [Required]
        public string Role { get; set; } = "ResearchAssistant";
        public ICollection<Patient>? Patients { get; set; } = new List<Patient>();
    }
}
