namespace health_information.Models.Dto
{
    public class AdminPatientDto
    {
        public int Id { get; set; }
        public string PatientStudyId { get; set; }
        public int Age { get; set; }
        public string Gender { get; set; } = string.Empty;
        public int CreatedById { get; set; }
        public string OperatorName { get; set; } = string.Empty;

        public List<VitalRecordResponseDto> VitalRecords { get; set; } = new List<VitalRecordResponseDto>();
    }
}
