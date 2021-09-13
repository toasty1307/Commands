using System.Threading.Tasks;
using Commands.CommandsStuff;
using Commands.Types;
using DSharpPlus;

namespace Commands.Commands.Commands
{
    public class Enabled : Command
    {
        public override string GroupName => "Commands";
        public override string Description => "Enable/disable command ig";
        public override bool GuildOnly => true;
        public override bool Guarded => true;

        public override Argument[] Arguments => new Argument[]
        {
            new Argument<CommandArgumentType>
            {
                Key = "Command",
                Optional = true
            },
            new Argument<GroupArgumentType>
            {
                Key = "Group",
                Optional = true
            },
            new Argument<BoolArgumentType>
            {
                Key = "Enable",
                Optional = true
            }
        };

        public override async Task Run(CommandContext ctx)
        {
            var command = ctx.GetArg<Command>("Command");
            var group = ctx.GetArg<Group>("Group");
            var enable = ctx.GetArg<bool?>("Enable");
            var provider = ctx.Extension.Provider;

            if (provider is null)
            {
                await ctx.ReplyAsync("No Provider is registered!");
                return;
            }
            if (group is null)
            {
                var enabled = (await provider.Get(ctx.Guild)).CommandStatuses[command];
                if (enable is null)
                    await ctx.ReplyAsync($"The Command {command.Name} is {(enabled ? "Enabled" : "Disabled")} in {ctx.Guild.Name}");
                else if (!(command.Guarded || command.Group.Guarded))
                {
                    ctx.Extension.CommandStatusChanged(ctx.Guild, command, (bool) enable);
                    await ctx.ReplyAsync($"Command {command.Name} was {((bool) enable ? "Enabled" : "Disabled")}");
                }
                else
                    await ctx.ReplyAsync($"Command {command.Name} is guarded :|");
            }
            else
            {
                var enabled = (await provider.Get(ctx.Guild)).GroupStatuses[group];
                if (enable is null)
                    await ctx.ReplyAsync($"The Group {group.Name} is {(enabled ? "Enabled" : "Disabled")} in {ctx.Guild.Name}");
                else if (!group.Guarded)
                {
                    ctx.Extension.GroupStatusChanged(ctx.Guild, group, (bool) enable);
                    await ctx.ReplyAsync($"Group {group.Name} was {((bool) enable ? "Enabled" : "Disabled")}");
                }
                else
                    await ctx.ReplyAsync($"Group {group.Name} is guarded :|");
            }
        }

        public override async Task Run(InteractionContext ctx)
        {
            var command = ctx.GetArg<Command>("Command");
            var group = ctx.GetArg<Group>("Group");
            var enable = ctx.GetArg<bool?>("Enable");
            var provider = ctx.Extension.Provider;

            if (provider is null)
            {
                await ctx.FollowUpAsync("No Provider is registered!");
                return;
            }
            if (group is null)
            {
                var enabled = (await provider.Get(ctx.Guild)).CommandStatuses[command];
                if (enable is null)
                    await ctx.FollowUpAsync($"The Command {command.Name} is {(enabled ? "Enabled" : "Disabled")} in {ctx.Guild.Name}");
                else if (!(command.Guarded || command.Group.Guarded))
                {
                    ctx.Extension.CommandStatusChanged(ctx.Guild, command, (bool) enable);
                    await ctx.FollowUpAsync($"Command {command.Name} was {((bool) enable ? "Enabled" : "Disabled")}");
                }
                else
                    await ctx.FollowUpAsync($"Command {command.Name} is guarded :|");
            }
            else
            {
                var enabled = (await provider.Get(ctx.Guild)).GroupStatuses[group];
                if (enable is null)
                    await ctx.FollowUpAsync($"The Group {group.Name} is {(enabled ? "Enabled" : "Disabled")} in {ctx.Guild.Name}");
                else if (!group.Guarded)
                {
                    ctx.Extension.GroupStatusChanged(ctx.Guild, group, (bool) enable);
                    await ctx.FollowUpAsync($"Group {group.Name} was {((bool) enable ? "Enabled" : "Disabled")}");
                }
                else
                    await ctx.FollowUpAsync($"Group {group.Name} is guarded :|");
            }
        }

        public Enabled(DiscordClient client) : base(client)
        {
        }
    }
}