using Microsoft.EntityFrameworkCore;

namespace DatabaseManagement
{
    public class HangfireContext : DbContext
    {
        public HangfireContext(DbContextOptions<HangfireContext> options)
        : base(options)
        {
            Database.EnsureDeleted();
            Database.EnsureCreated();
        }
    }
}