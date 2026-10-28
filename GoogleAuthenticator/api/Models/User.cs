namespace GoogleAuthenticator.Models
{
    public class User
    {
        public required string Id { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Email { get; set; }
        public string? ProfileImageUrl { get; set; }
        public int GroupId { get; set; }
    }
}
