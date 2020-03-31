using System.Data.Entity;

namespace DatabaseManagement
{
    public class HangfireDbContextInitializer<T> : IDatabaseInitializer<T> where T : DbContext
    {
        public void InitializeDatabase(T context)
        {
            Database db = context.Database;
            db.Delete();
            db.Create();
        }
    }
}