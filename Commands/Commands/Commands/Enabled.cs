using System.Threading.Tasks;
using Commands.CommandsStuff;
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
            new()
            {
                Key = "CommandOrGroup",
                Types = new []{typeof(Group), typeof(Command)}
            },
            new()
            {
                Key = "Enable",
                Types = new []{typeof(bool?)},
                Optional = true
            }
        };

        public override async Task Run(MessageContext ctx)
        {
            var group = ctx.GetArg<Group>("CommandOrGroup", out var isGroup);
            var enable = ctx.GetArg<bool?>("Enable");
            var provider = ctx.Extension.Provider;

            if (provider is null)
            {
                await ctx.ReplyAsync("No Provider is registered!");
                return;
            }

            if (isGroup)
            {
                var enabled = provider.Get(ctx.Guild).Groups[group];
                if (enable is null)
                    await ctx.ReplyAsync(
                        $"The Group {@group.Name} is {(enabled ? "Enabled" : "Disabled")} in {ctx.Guild.Name}");
                else if (!@group.Guarded)
                {
                    ctx.Extension.GroupStatusChanged(ctx.Guild, @group, (bool) enable);
                    await ctx.ReplyAsync($"Group {@group.Name} was {((bool) enable ? "Enabled" : "Disabled")}");
                }
                else
                    await ctx.ReplyAsync($"Group {@group.Name} is guarded :|");
            }
            else
            {
                var command = ctx.GetArg<Command>("CommandOrGroup");
                var enabled = provider.Get(ctx.Guild).Commands[command];
                if (enable is null)
                    await ctx.ReplyAsync(
                        $"The Command {command.Name} is {(enabled ? "Enabled" : "Disabled")} in {ctx.Guild.Name}");
                else if (!(command.Guarded || command.Group.Guarded))
                {
                    ctx.Extension.CommandStatusChanged(ctx.Guild, command, (bool) enable);
                    await ctx.ReplyAsync($"Command {command.Name} was {((bool) enable ? "Enabled" : "Disabled")}");
                }
                else
                    await ctx.ReplyAsync($"Command {command.Name} is guarded :|");
            }
        }

        public override async Task Run(InteractionContext ctx)
        {
            var group = ctx.GetArg<Group>("CommandOrGroup", out var isGroup);
            var enable = ctx.GetArg<bool?>("Enable");
            var provider = ctx.Extension.Provider;

            if (provider is null)
            {
                await ctx.ReplyAsync("No Provider is registered!");
                return;
            }

            if (isGroup)
            {
                var enabled = provider.Get(ctx.Guild).Groups[group];
                if (enable is null)
                    await ctx.ReplyAsync(
                        $"The Group {@group.Name} is {(enabled ? "Enabled" : "Disabled")} in {ctx.Guild.Name}");
                else if (!@group.Guarded)
                {
                    ctx.Extension.GroupStatusChanged(ctx.Guild, @group, (bool) enable);
                    await ctx.ReplyAsync($"Group {@group.Name} was {((bool) enable ? "Enabled" : "Disabled")}");
                }
                else
                    await ctx.ReplyAsync($"Group {@group.Name} is guarded :|");
            }
            else
            {
                var command = ctx.GetArg<Command>("CommandOrGroup");
                var enabled = provider.Get(ctx.Guild).Commands[command];
                if (enable is null)
                    await ctx.ReplyAsync(
                        $"The Command {command.Name} is {(enabled ? "Enabled" : "Disabled")} in {ctx.Guild.Name}");
                else if (!(command.Guarded || command.Group.Guarded))
                {
                    ctx.Extension.CommandStatusChanged(ctx.Guild, command, (bool) enable);
                    await ctx.ReplyAsync($"Command {command.Name} was {((bool) enable ? "Enabled" : "Disabled")}");
                }
                else
                    await ctx.ReplyAsync($"Command {command.Name} is guarded :|");
            }
        }

        public Enabled(DiscordClient client) : base(client)
        {
        }
    }
}