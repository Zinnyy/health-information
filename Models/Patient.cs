using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace health_information.Models
{
    public class Patient
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string PatientStudyId { get; set; }
        [Required]
        public int Age { get; set; }
        [Required]
        public string Gender { get; set; }
        [Required]
        public int CreatedById { get; set; }
        [ForeignKey("CreatedById")]
        public User? CreatedBy { get; set; }
        public ICollection<VitalRecord>? VitalRecords { get; set; } = new List<VitalRecord>();

    }
}
