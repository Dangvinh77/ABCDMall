using Microsoft.EntityFrameworkCore;

namespace ABCDMall.Shared.Persistence
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }
    }
}
