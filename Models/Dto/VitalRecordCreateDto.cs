using System.ComponentModel.DataAnnotations;

namespace health_information.Models.Dto
{
    public class VitalRecordCreateDto
    {
        [Required]
        public int PatientId { get; set; }
        
        [Required]
        [Range(30, 250, ErrorMessage = "Heart rate value must be biologically realistic.")]
        public int HeartRate { get; set; }
        
        [Required]
        [Range(60, 220, ErrorMessage = "Systolic Bp  must be biologically realistic.")]
        public int SystolicBp { get; set; }

        [Required]
        [Range(20, 500, ErrorMessage = "Glucose level must be biologically realistic.")]
        public int Glucose { get; set; }
    }
}
