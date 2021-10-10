using System;
using System.Threading.Tasks;
using Commands.CommandsStuff;
using CommandsTest.Modules;
using CommandsTest.Utils;
using DSharpPlus;
using DSharpPlus.Entities;

namespace CommandsTest.Commands.Moderation
{
    public class Whitelist : Command
    {
        public Whitelist(DiscordClient client) : base(client) { }
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
            var isCommand = ctx.GetArg<Command>("CommandOrGroup", out var command);
            ctx.GetArg<Group>("CommandOrGroup", out var group);
            var blacklistModule = Client.GetBlacklistModule();
            var result = isCommand ? blacklistModule.WhiteListUser(user, ctx.Guild, command) : blacklistModule.WhiteListUser(user, ctx.Guild, group);
            var replyString = result switch
            {
                WhitelistResult.Removed => $"Removed {user.Nickname ?? user.Username} from the blacklist for {(isCommand ? $"Command {command.Name}" : $"Group {group.Name}")}",
                WhitelistResult.NotBlacklisted => $"blacklist them first you dum",
                _ => throw new ArgumentOutOfRangeException()
            };
            await ctx.ReplyAsync(replyString);
        }

        public override async Task Run(InteractionContext ctx)
        {
            var user = (DiscordMember)ctx.GetArg<DiscordUser>("User");
            var isCommand = ctx.GetArg<Command>("CommandOrGroup", out var command);
            ctx.GetArg<Group>("CommandOrGroup", out var group);
            var blacklistModule = Client.GetBlacklistModule();
            var result = isCommand ? blacklistModule.WhiteListUser(user, ctx.Guild, command) : blacklistModule.WhiteListUser(user, ctx.Guild, group);
            var replyString = result switch
            {
                WhitelistResult.Removed => $"Removed {user.Nickname ?? user.Username} from the blacklist for {(isCommand ? $"Command {command.Name}" : $"Group {group.Name}")}",
                WhitelistResult.NotBlacklisted => $"blacklist them first you dum",
                _ => throw new ArgumentOutOfRangeException()
            };
            await ctx.ReplyAsync(replyString);
        }
    }
}