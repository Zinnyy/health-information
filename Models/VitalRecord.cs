using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace health_information.Models
{
    public class VitalRecord
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public int PatientId { get; set; }
        [ForeignKey("PatientId")]
        public Patient? Patient { get; set; }
        [Required]
        public DateTime DateRecorded { get; set; }
        [Required]
        public int HeartRate { get; set; }
        [Required]
        public int SystolicBp { get; set; }
        [Required]
        public int Glucose { get; set; }
    }
}
