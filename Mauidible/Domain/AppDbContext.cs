using Microsoft.EntityFrameworkCore;

namespace Mauidible.Domain
{
    public class AppDbContext : DbContext
    {

        public AppDbContext()
        {
            
        }

        protected override void OnConfiguring (DbContextOptionsBuilder optionsBuilder)
        {

            var connectionString = $"Filename={Constants.DatabasePath(Constants.DatabaseFilename)}";
            optionsBuilder.UseSqlite(connectionString);
        }

        public DbSet<Bookmark> Bookmarks { get; set; }
        public DbSet<AudioBook> AudioBooks { get; set; }
        public DbSet<Chapter> Chapters { get; set; }
    }
}
