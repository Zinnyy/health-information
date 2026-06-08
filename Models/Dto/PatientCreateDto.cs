using System.ComponentModel.DataAnnotations;

namespace health_information.Models.Dto
{
    public class PatientCreateDto
    {
        
        public string PatientStudyId { get; set; } = string.Empty;
        [Required]
        
        public int Age { get; set; }
        [Required]
        public string Gender { get; set; } = string.Empty;
        [Required]
        public int CreatedById { get; set; }
    }
}
