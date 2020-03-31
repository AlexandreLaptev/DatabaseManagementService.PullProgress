using System.Data.Entity;

namespace DatabaseManagement
{
    public class HangfireDbContext : DbContext
    {
        public HangfireDbContext() : base("HangfireDb")
        {
        }
    }
}