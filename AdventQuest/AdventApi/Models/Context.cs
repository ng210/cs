using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;

namespace AdventApi.Models
{
    public class Context : DbContext
    {
        public DbSet<Player> Players { get; set; }
        public DbSet<Question> Questions { get; set; }
    }
}
