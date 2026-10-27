using System.ComponentModel.DataAnnotations;

namespace AdventApi.Models
{
    public class Player
    {
        [Key]
        public required string Id { get; set; }
        public required string FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Email { get; set; }
        public string? ProfileImageUrl { get; set; }
        public ICollection<Answer> Answers { get; set; } = [];
    }
}
