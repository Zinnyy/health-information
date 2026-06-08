using System.ComponentModel.DataAnnotations;

namespace health_information.Models.Dto
{
    public class PatientDto
    {
        public int Id { get; set; }
        [Required]
        public string PatientStudyId { get; set; }
        [Required]
        public int Age { get; set; }
        [Required]
        public string Gender { get; set; }
    }
}
