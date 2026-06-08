namespace health_information.Models.Dto
{
    public class LoginResponseDto
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
        public string Issuer { get; set; }
        public string Audience { get; set; }
    }
}
