using MessageSink.EntityFramework.Model;
using Microsoft.EntityFrameworkCore;

namespace MessageSink.EntityFramework.Data
{
    public class MSSqlDbContext : DbContext
    {
        public MSSqlDbContext(DbContextOptions options) : base(options) { }
        public DbSet<Tick> CarDetails => Set<Tick>();
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Tick>(eb => eb.HasKey(x => new { x.TickKey, x.TickTimeInUtc }));
        }
    }
}
