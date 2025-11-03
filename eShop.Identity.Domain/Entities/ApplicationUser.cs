namespace eShop.Identity.Domain.Entities
{
    public class ApplicationUser
    {
        public int Id { get; set; }
        public string Username { get; set; } = default!;
        public string PasswordHash { get; set; } = default!;
        public string Role { get; set; } = "User";
    }
}
