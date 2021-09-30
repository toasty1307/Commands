using System;
using System.Threading.Tasks;
using Commands.CommandsStuff;
using CommandsTest.Modules;
using CommandsTest.Utils;
using DSharpPlus;
using DSharpPlus.Entities;

namespace CommandsTest.Commands.Moderation
{
    public class Blacklist : Command
    {
        public Blacklist(DiscordClient client) : base(client) { }
        public override string GroupName => "Moderation";

        public override Permissions UserPermissions => Permissions.All;

        public override Argument[] Arguments => new Argument[]
        {
            new()
            {
                Key = "User",
                Types = new []{typeof(DiscordUser)}
            },
            new()
            {
                Key = "CommandOrGroup",
                Types = new []{typeof(Command), typeof(Group)}
            }
        };

        public override async Task Run(MessageContext ctx)
        {
            var user = (DiscordMember)ctx.GetArg<DiscordUser>("User");
            var command = ctx.GetArg<Command>("CommandOrGroup", out var isCommand);
            var group = ctx.GetArg<Group>("CommandOrGroup", out _);
            var blacklistModule = Client.GetBlacklistModule();
            var result = isCommand ? blacklistModule.BlacklistUser(user, ctx.Guild, command) : blacklistModule.BlacklistUser(user, ctx.Guild, @group);
            var replyString = result switch
            {
                BlacklistResult.Added => $"Added {user.Nickname ?? user.Username} to the blacklist for {(isCommand ? $"Command {command.Name}" : $"Group {group.Name}")}",
                BlacklistResult.AlreadyBlacklisted => $"but they already blacklisted :|",
                _ => throw new ArgumentOutOfRangeException()
            };
            await ctx.ReplyAsync(replyString);
        }

        public override async Task Run(InteractionContext ctx)
        {
            var user = (DiscordMember)ctx.GetArg<DiscordUser>("User");
            var command = ctx.GetArg<Command>("CommandOrGroup", out var isCommand);
            var group = ctx.GetArg<Group>("CommandOrGroup", out _);
            var blacklistModule = Client.GetBlacklistModule();
            var result = isCommand ? blacklistModule.BlacklistUser(user, ctx.Guild, command) : blacklistModule.BlacklistUser(user, ctx.Guild, @group);
            var replyString = result switch
            {
                BlacklistResult.Added => $"Added {user.Nickname ?? user.Username} to the blacklist for {(isCommand ? $"Command {command.Name}" : $"Group {group.Name}")}",
                BlacklistResult.AlreadyBlacklisted => $"but they already blacklisted :|",
                _ => throw new ArgumentOutOfRangeException()
            };
            await ctx.ReplyAsync(replyString);
        }
    }
}