namespace AuthApp.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } = default!;
        public string Password { get; set; } = default!;
        public bool IsActive { get; set; }
    }
}
