using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using Commands.Data;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Newtonsoft.Json;

namespace CommandsTest.Data
{
    public class GuildContext : DbContext, ISettingProvider
    {
        [NotMapped] public List<GuildEntity> Cache { get; } = new();

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
            
            modelBuilder.Entity<GuildEntity>()
                .Property(x => x.Commands)
                .HasConversion(x => JsonConvert.SerializeObject(x),
                    x => JsonConvert.DeserializeObject<Dictionary<DisabledCommandEntity, bool>>(x),
                    ValueComparer.CreateDefault(typeof(Dictionary<DisabledCommandEntity, bool>), false));
            modelBuilder.Entity<GuildEntity>()
                .Property(x => x.Groups)
                .HasConversion(x => JsonConvert.SerializeObject(x),
                    x => JsonConvert.DeserializeObject<Dictionary<DisabledGroupEntity, bool>>(x),
                    ValueComparer.CreateDefault(typeof(Dictionary<DisabledGroupEntity, bool>), false));
        }

        public void Set(global::Commands.Data.GuildEntity entity)
        {
            Cache.Remove(Cache.FirstOrDefault(x => x.GuildId == entity.GuildId));
            Cache.Add(GuildEntity.FromGuildEntity(entity));
            var guildEntity = Guilds.Find(entity.GuildId);
            if (guildEntity is not null)
            {
                Guilds.Update(guildEntity!);
            }
            else
                Guilds.Add(GuildEntity.FromGuildEntity(entity));
            SaveChanges();
        }

        public global::Commands.Data.GuildEntity Get(DiscordGuild guild) =>
            Cache.FirstOrDefault(x => x.GuildId == guild.Id);

        public void Remove(global::Commands.Data.GuildEntity entity)
        {
            if (Cache.All(x => x.GuildId != entity.GuildId)) return;
            Cache.RemoveAll(x => x.GuildId == entity.GuildId);
            var guildEntity = Guilds.Find(entity.GuildId);
            if (guildEntity is not null)
                Guilds.Remove(guildEntity!);
            SaveChanges();
        }

        public async void Init()
        {
            Cache.Clear();
            Cache.AddRange(await Guilds.ToListAsync());
        }
    }
}