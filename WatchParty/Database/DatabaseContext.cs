using Microsoft.EntityFrameworkCore;

namespace WatchParty.Database
{
    public class DatabaseContext: DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext> options)
            : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Room>()
                .HasIndex(b => b.Id);
        }

        public DbSet<Room> Rooms { get; set; }
    }
}