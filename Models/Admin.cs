using System.ComponentModel.DataAnnotations;

namespace health_information.Models
{
    public class Admin
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty; // Store PBKDF2 hash here

        [Required]
        public string PasswordSalt { get; set; } = string.Empty;

        [Required]
        public string FullName { get; set; } = string.Empty;

        public string Role { get; set; } = "Admin"; // Hard-locked default role

        public bool IsActive { get; set; } = true;
    }
}
