using Microsoft.EntityFrameworkCore;

namespace CommandsTest.Data
{
    public class GuildContext : DbContext
    {
        public DbSet<GuildEntity> Guilds { get; set; }
        public DbSet<BlacklistEntity> Blacklist { get; set; }
        public DbSet<TagEntity> Tags { get; set; }
        public DbSet<AfkEntity> Afk { get; set; }
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
            modelBuilder.Entity<GuildEntity>()
                .HasMany(x => x.Tags)
                .WithOne(x => x.GuildEntity)
                .HasForeignKey(x => x.GuildId);
            
            modelBuilder.Entity<BlacklistEntity>()
                .HasMany(x => x.Commands);
            modelBuilder.Entity<BlacklistEntity>()
                .HasMany(x => x.Groups);
        }
    }
}