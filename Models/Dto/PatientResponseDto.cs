namespace health_information.Models.Dto
{
    public class PatientResponseDto
    {
        public int Id { get; set; }
        public string PatientStudyId { get; set; }
        public int Age { get; set; }
        public string Gender { get; set; }
        public string CreatedBy { get; set; }
        public ICollection<VitalRecordResponseDto> VitalRecords { get; set; } = new List<VitalRecordResponseDto>();
    }
}
