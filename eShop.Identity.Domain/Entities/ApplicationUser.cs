namespace eShop.Identity.Domain.Entities
{
    public class ApplicationUser
    {
        public int Id { get; set; }

        // Brugernavn — svarer til eShopOnWeb's ApplicationUser.UserName
        public string UserName { get; set; } = default!;
        public string NormalizedUserName { get; set; } = default!;

        // E-mail — valgfrit, men eShopOnWeb bruger det til login og visning
        public string Email { get; set; } = default!;
        public string NormalizedEmail { get; set; } = default!;

        // Hashet adgangskode
        public string PasswordHash { get; set; } = default!;

        // Rolle: "User" eller "Admin"
        public string Role { get; set; } = "User";

        // Tidspunkt for oprettelse
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
