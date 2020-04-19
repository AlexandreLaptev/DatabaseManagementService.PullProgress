using Microsoft.EntityFrameworkCore;
using JetBrains.Annotations;

namespace DatabaseManagement
{
    public class HangfireDbContext : DbContext
    {
        private readonly string _connectionString;

        public HangfireDbContext([NotNull] string connectionString)
        {
            _connectionString = connectionString;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite(_connectionString);
    }
}