using System.Collections.Generic;
using System.Linq;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Newtonsoft.Json;

namespace Commands.Data
{
    public class GuildContext : DbContext
    {
        public List<GuildEntity> Cache { get; private set; } = new();

        public DbSet<GuildEntity> Guilds { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql(Constants.CommandsDbConnectionString);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
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

        public void DoCacheStuff()
        {
            Cache = Guilds
                .ToList();
        }

        public GuildEntity Get(DiscordGuild guild) => Cache.FirstOrDefault(x => x.Id == guild.Id);

        public void Update(DiscordGuild guild, GuildEntity entity)
        {
            var cacheEntity = Cache.FirstOrDefault(x => x.Id == guild.Id);
            if (cacheEntity is null)
            {
                Cache.Add(entity);
                Guilds.Add(entity);
                SaveChanges();
                return;
            }

            cacheEntity.Prefix = entity.Prefix;
            cacheEntity.Commands = entity.Commands;
            cacheEntity.Groups = entity.Groups;
            Guilds.Update(entity);
            SaveChanges();
        }
    }
}