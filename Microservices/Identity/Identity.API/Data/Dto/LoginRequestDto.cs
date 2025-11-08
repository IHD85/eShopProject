namespace Identity.API.Data.Dto
{
    public class LoginRequestDto
    {
        public required string Username { get; set; }
        public required string Password { get; set; }
    }
}
