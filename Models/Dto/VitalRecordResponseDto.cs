namespace health_information.Models.Dto
{
    public class VitalRecordResponseDto
    {
        public int ? Id { get; set; }
        public int PatientId { get; set; }
        public DateTime DateRecorded { get; set; }
        public int HeartRate { get; set; }
        public int SystolicBp { get; set; }
        public int Glucose { get; set; }
    }
}
