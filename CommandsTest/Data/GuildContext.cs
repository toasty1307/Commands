using Microsoft.EntityFrameworkCore;

namespace CommandsTest.Data
{
    public class GuildContext : DbContext
    {
        public DbSet<GuildEntity> Guilds { get; set; }
        public DbSet<BlacklistEntity> Blacklist { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(Constants.DatabaseConnectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<GuildEntity>()
                .HasMany(x => x.Blacklist)
                .WithOne(x => x.GuildEntity)
                .HasForeignKey(x => x.GuildId);
            modelBuilder.Entity<BlacklistEntity>()
                .HasMany(x => x.Commands);
            modelBuilder.Entity<BlacklistEntity>()
                .HasMany(x => x.Groups);
        }
    }
}