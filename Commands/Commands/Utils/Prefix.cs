using System.Threading.Tasks;
using Commands.CommandsStuff;
using DSharpPlus;

namespace Commands.Commands.Utils
{
    public class Prefix : Command
    {
        public override string GroupName => "Utils";
        public override string Description => "prefix command to chnage prefix ig idk";

        public override Argument[] Arguments => new Argument[]
        {
            new()
            {
                Key = "Prefix",
                Optional = true,
                Types = new []{typeof(string)}
            }
        };

        public override async Task Run(MessageContext ctx)
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
                        _ = Task.Run(async () =>
                        {
                            ctx.Extension.CommandPrefixChanged(ctx.Guild, prefix);
                            await ctx.ReplyAsync($"Set the command prefix to {prefix}"); 
                        });
                    }
                    else
                        await ctx.ReplyAsync("Cant Set a prefix in DMs!");
                }
                else
                    await ctx.ReplyAsync("No Settings Provider is registered!");
            }
            else if (provider is not null)
                await ctx.ReplyAsync(
                    $"The prefix in {(ctx.Guild is null ? "DMs" : ctx.Guild.Name)} is {(ctx.Guild is null ? $"`@{ctx.Client.CurrentUser.Username}#{ctx.Client.CurrentUser.Discriminator}`" : $"`{(provider.Get(ctx.Guild))?.Prefix ?? ctx.Extension.CommandPrefix}` or `@{ctx.Client.CurrentUser.Username}#{ctx.Client.CurrentUser.Discriminator}`")}");
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
                        var settings = provider.Get(ctx.Guild);
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
                    $"The prefix in {(ctx.Guild is null ? "DMs" : ctx.Guild.Name)} is `{(provider.Get(ctx.Guild))?.Prefix ?? ctx.Extension.CommandPrefix}` or `@{ctx.Client.CurrentUser.Username}#{ctx.Client.CurrentUser.Discriminator}`");
            else 
                await ctx.ReplyAsync($"The prefix in {(ctx.Guild is null ? "DMs" : ctx.Guild.Name)} is `{ctx.Extension.CommandPrefix}` or `@{ctx.Client.CurrentUser.Username}#{ctx.Client.CurrentUser.Discriminator}`");
        }

        public Prefix(DiscordClient client) : base(client)
        {
        }
    }
}