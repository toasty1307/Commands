using System.Threading.Tasks;
using Commands.CommandsStuff;
using Commands.Types;
using DSharpPlus;

namespace Commands.Commands.Utils
{
    public class Prefix : Command
    {
        public override string GroupName => "Utils";
        public override string Description => "prefix command to chnage prefix ig idk";

        public override Argument[] Arguments => new Argument[]
        {
            new Argument<StringArgumentType>()
            {
                Key = "Prefix",
                Optional = true
            }
        };

        public override async Task Run(CommandContext ctx)
        {
            var prefix = ctx.GetArg<string>("Prefix");
            var shouldSetNewPrefix = !string.IsNullOrWhiteSpace(prefix);
            var provider = ctx.Extension.Provider;
            if (shouldSetNewPrefix)
            {
                if (provider is not null)
                {
                    if (ctx.Guild is not null)
                    {
                        var settings = await provider.Get(ctx.Guild);
                        settings.Prefix = prefix;
                        ctx.Extension.CommandPrefixChanged(ctx.Guild, prefix);
                        await ctx.ReplyAsync($"Set the command prefix to {prefix}");
                    }
                    else
                        await ctx.ReplyAsync("Cant Set a prefix in DMs!");
                }
                else
                    await ctx.ReplyAsync("No Settings Provider is registered!");
            }
            else if (provider is not null)
                await ctx.ReplyAsync(
                    $"The prefix in {(ctx.Guild is null ? "DMs" : ctx.Guild.Name)} is {(ctx.Guild is null ? $"`@{ctx.Client.CurrentUser.Username}#{ctx.Client.CurrentUser.Discriminator}`" : $"`{(await provider.Get(ctx.Guild))?.Prefix ?? ctx.Extension.CommandPrefix}` or `@{ctx.Client.CurrentUser.Username}#{ctx.Client.CurrentUser.Discriminator}`")}");
            else
                await ctx.ReplyAsync($"The prefix in {(ctx.Guild is null ? "DMs" : ctx.Guild.Name)} is {(ctx.Guild is null ? $"`@{ctx.Client.CurrentUser.Username}#{ctx.Client.CurrentUser.Discriminator}`" : $"`{ctx.Extension.CommandPrefix}` or `@{ctx.Client.CurrentUser.Username}#{ctx.Client.CurrentUser.Discriminator}`")}");
        }

        public override async Task Run(InteractionContext ctx)
        {
            var prefix = ctx.GetArg<string>("Prefix");
            var shouldSetNewPrefix = !string.IsNullOrWhiteSpace(prefix);
            var provider = ctx.Extension.Provider;
            if (shouldSetNewPrefix)
            {
                if (provider is not null)
                {
                    if (ctx.Guild is not null)
                    {
                        var settings = await provider.Get(ctx.Guild);
                        settings.Prefix = prefix;
                        ctx.Extension.CommandPrefixChanged(ctx.Guild, prefix);
                        await ctx.FollowUpAsync($"Set the command prefix to {prefix}");
                    }
                    else
                        await ctx.FollowUpAsync("Cant Set a prefix in DMs!");
                }
                else
                    await ctx.FollowUpAsync("No Settings Provider is registered!");
            }
            else if (provider is not null)
                await ctx.FollowUpAsync(
                    $"The prefix in {(ctx.Guild is null ? "DMs" : ctx.Guild.Name)} is `{(await provider.Get(ctx.Guild))?.Prefix ?? ctx.Extension.CommandPrefix}` or `@{ctx.Client.CurrentUser.Username}#{ctx.Client.CurrentUser.Discriminator}`");
            else 
                await ctx.FollowUpAsync($"The prefix in {(ctx.Guild is null ? "DMs" : ctx.Guild.Name)} is `{ctx.Extension.CommandPrefix}` or `@{ctx.Client.CurrentUser.Username}#{ctx.Client.CurrentUser.Discriminator}`");
        }

        public Prefix(DiscordClient client) : base(client)
        {
        }
    }
}