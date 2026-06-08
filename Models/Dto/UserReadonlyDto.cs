namespace health_information.Models.Dto
{
    public class UserReadonlyDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? Email { get; set; }
    }
}
