using System.Collections.Generic;
using System.Linq;
using Commands;
using Commands.CommandsStuff;
using CommandsTest.Data;
using DSharpPlus.Entities;
using Microsoft.EntityFrameworkCore;

namespace CommandsTest.Modules
{
    public class BlacklistModule
    {
        private List<GuildEntity> Cache { get; set; }

        public BlacklistModule() => UpdateCache();
 
        private bool CanUserUse(DiscordUser user, DiscordGuild guild, Command command)
        {
            var guildEntity = Cache.FirstOrDefault(x => x.GuildId == guild.Id);
            var blacklist = guildEntity?.Blacklist.FirstOrDefault(x => x.Id == user.Id);
            if (blacklist is null || !blacklist.Commands.Any() || !blacklist.Groups.Any()) return true;
            var commandEntity = blacklist.Commands.FirstOrDefault(x => x.Name == command.Name);
            if (commandEntity is not default(CommandEntity)) return false;
            var group = blacklist.Groups.FirstOrDefault(x => x.Name == command.Group.Name);
            return group is default(GroupEntity);
        }

        public BlacklistResult BlacklistUser(DiscordUser user, DiscordGuild guild, Command command)
        {
            using var guildContext = new GuildContext();
            var guildEntity = guildContext.Find<GuildEntity>(guild.Id);
            if (guildEntity is null)
            {
                var newGuildEntity = new GuildEntity
                {
                    GuildId = guild.Id
                };
                var blackListEntity = new BlacklistEntity
                {
                    Id = user.Id,
                    GuildId = guild.Id
                };
                var newCommandEntity = new CommandEntity {Name = command.Name};
                _ = newCommandEntity.Id;
                blackListEntity.Commands.Add(newCommandEntity);
                newGuildEntity.Blacklist.Add(blackListEntity);
                guildContext.Add(newGuildEntity);
                guildContext.Add(blackListEntity);
                guildContext.SaveChanges();
                Cache.Add(newGuildEntity);
                return BlacklistResult.Added;
            }

            var blacklistForUser = guildContext.Blacklist.Where(x => x.Id == user.Id).Include(x => x.Commands).Include(x => x.Groups).FirstOrDefault();
            if (blacklistForUser is default(BlacklistEntity))
            {
                blacklistForUser = new BlacklistEntity
                {
                    Commands = new List<CommandEntity>(),
                    Groups = new List<GroupEntity>(),
                    GuildId = guild.Id,
                    Id = user.Id
                };
                Cache.FirstOrDefault(x => x.GuildId == guild.Id)?.Blacklist.Add(blacklistForUser);
                guildContext.Blacklist.Add(blacklistForUser);
                guildContext.SaveChanges();
                return BlacklistResult.Added;
            }
            if (blacklistForUser.Commands.Exists(x => x.Name == command.Name))
                return BlacklistResult.AlreadyBlacklisted; 
            blacklistForUser.Commands.Add(new CommandEntity{Name = command.Name});
            guildContext.SaveChanges();
            Cache[Cache.IndexOf(Cache.First(x => x.GuildId == guildEntity.GuildId))] = guildEntity;
            return BlacklistResult.Added;
        }
        
        public BlacklistResult BlacklistUser(DiscordUser user, DiscordGuild guild, Group group)
        {
            using var guildContext = new GuildContext();
            var guildEntity = guildContext.Guilds.Find(guild.Id);
            if (guildEntity is null)
            {
                var newGuildEntity = new GuildEntity
                {
                    GuildId = guild.Id
                };
                var blackListEntity = new BlacklistEntity
                {
                    Id = user.Id
                };
                var groupEntity = new GroupEntity {Name = group.Name};
                blackListEntity.Groups.Add(groupEntity);
                newGuildEntity.Blacklist.Add(blackListEntity);
                guildContext.SaveChanges();
                Cache.Add(newGuildEntity);
                return BlacklistResult.Added;
            }

            var blacklistForUser = guildContext.Blacklist.Where(x => x.Id == user.Id).Include(x => x.Commands).Include(x => x.Groups).FirstOrDefault();
            if (blacklistForUser is default(BlacklistEntity))
            {
                blacklistForUser = new BlacklistEntity
                {
                    Commands = new List<CommandEntity>(),
                    Groups = new List<GroupEntity>(),
                    GuildId = guild.Id,
                    Id = user.Id
                };
                Cache.FirstOrDefault(x => x.GuildId == guild.Id)?.Blacklist.Add(blacklistForUser);
                guildContext.Blacklist.Add(blacklistForUser);
                guildContext.SaveChanges();
                return BlacklistResult.Added;
            }
            if (blacklistForUser.Groups.Exists(x => x.Name == group.Name))
                return BlacklistResult.AlreadyBlacklisted; 
            blacklistForUser.Groups.Add(new GroupEntity{Name = group.Name});
            guildContext.SaveChanges();
            Cache[Cache.IndexOf(Cache.First(x => x.GuildId == guildEntity.GuildId))] = guildEntity;
            return BlacklistResult.Added;
        }

        public WhitelistResult WhiteListUser(DiscordUser user, DiscordGuild guild, Command command)
        {
            if (!Cache.Find(x => x.GuildId == guild.Id)?.Blacklist?.Any(x => x.Id == user.Id && x.Commands.Any(cmd => cmd.Name == command.Name)) ?? true)
                return WhitelistResult.NotBlacklisted;
            using var context = new GuildContext();
            var blacklist = context.Blacklist.Include(x => x.Commands).Include(x => x.Groups).Include(x => x.GuildEntity).First(x => x.Id == user.Id);
            blacklist.Commands.Remove(blacklist.Commands.First(x => x.Name == command.Name));
            context.Guilds.First(x => x.Id == guild.Id).Blacklist.First(x => x.Id == user.Id).Commands = blacklist.Commands;
            context.SaveChanges();
            context.Dispose();
            var temp = Cache.First(x => x.Id == guild.Id).Blacklist.First(x => x.Id == user.Id);
            temp.Commands.Remove(temp.Commands.First(x => x.Name == command.Name));
            Cache.First(x => x.Id == guild.Id).Blacklist.First(x => x.Id == user.Id).Commands = temp.Commands;
            return WhitelistResult.Removed;
        }
        
        public WhitelistResult WhiteListUser(DiscordUser user, DiscordGuild guild, Group group)
        {
            if (!Cache.Find(x => x.GuildId == guild.Id)?.Blacklist?.Any(x => x.Id == user.Id && x.Groups.Any(cmd => cmd.Name == group.Name))?? true)
                return WhitelistResult.NotBlacklisted;
            using var context = new GuildContext();
            var blacklist = context.Blacklist.Include(x => x.Commands).Include(x => x.Groups).Include(x => x.GuildEntity).First(x => x.Id == user.Id);
            blacklist.Groups.Remove(blacklist.Groups.First(x => x.Name == group.Name));
            context.Guilds.First(x => x.Id == guild.Id).Blacklist.First(x => x.Id == user.Id).Groups = blacklist.Groups;
            context.SaveChanges();
            context.Dispose();
            var temp = Cache.First(x => x.Id == guild.Id).Blacklist.First(x => x.Id == user.Id);
            temp.Groups.Remove(temp.Groups.First(x => x.Name == group.Name));
            Cache.First(x => x.Id == guild.Id).Blacklist.First(x => x.Id == user.Id).Groups = temp.Groups;
            return WhitelistResult.Removed;
        }

        public Inhibition Check(DiscordInteraction interaction, Command command)
        {
            return CanUserUse(interaction.User, interaction.Channel.Guild, command)
                ? default
                : new Inhibition
                {
                    Reason = "BLACKLIST",
                    Response = "blacklist moment"
                };
        }
        
        public Inhibition Check(DiscordMessage message, Command command)
        {
            return CanUserUse(message.Author, message.Channel.Guild, command)
                ? default
                : new Inhibition
                {
                    Reason = "BLACKLIST",
                    Response = "blacklist moment"
                };
        }

        private void UpdateCache()
        {
            using var guildContext = new GuildContext();
            var blacklist = guildContext.Blacklist
                .Include(x => x.Commands)
                .Include(x => x.Groups)
                .Include(x => x.GuildEntity)
                .ToList();
            var guilds = guildContext.Guilds.Include(x => x.Blacklist).ToList();
            Cache = guilds.Select(x => new GuildEntity
            {
                Blacklist = blacklist.Where(blacklistEntity => blacklistEntity.GuildId == x.GuildId).ToList(),
            }).ToList();
            Cache.ForEach(x => x.GuildId = x.Blacklist.First().GuildId);
        }
    }

    public enum WhitelistResult
    {
        Removed,
        NotBlacklisted
    }

    public enum BlacklistResult
    {
        Added,
        AlreadyBlacklisted
    }
}