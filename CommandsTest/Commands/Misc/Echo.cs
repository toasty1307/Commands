using System.Threading.Tasks;
using Commands.CommandsStuff;
using Commands.Types;
using DSharpPlus;
using DSharpPlus.Entities;

namespace CommandsTest.Commands.Misc
{
    public class Echo : Command
    {
        public override string GroupName => "Misc";
        public override string[] Aliases => new[] {"say"};
        public override Permissions ClientPermissions => Permissions.ManageMessages;
        public override string Description => "drinking milk";
        public override bool GuildOnly => true;

        public override Argument[] Arguments => new Argument[]
        {
            new()
            {
                Key = "Channel",
                Optional = true,
                Types = new []{typeof(DiscordTextChannelArgumentType)}
            },
            new()
            {
                Key = "Text",
                Infinite = true,
                Types = new []{typeof(string)}
            }
        };

        public override async Task Run(MessageContext ctx)
        {
            var channel = ctx.GetArg<DiscordChannel>("Channel") ?? ctx.Channel;
            await ctx.Message.DeleteAsync();
            var text = ctx.GetArg<string>("Text");
            var builder = new DiscordMessageBuilder().WithContent(text).WithAllowedMention(new UserMention());
            await channel.SendMessageAsync(builder);
        }

        public override async Task Run(InteractionContext ctx)
        {
            var channel = ctx.GetArg<DiscordChannel>("Channel") ?? ctx.Channel;
            var text = ctx.GetArg<string>("Text");
            var original = await ctx.Interaction.GetOriginalResponseAsync();
            if ((original.Flags & MessageFlags.Ephemeral) == 0) await ctx.Interaction.DeleteOriginalResponseAsync();
            var builder = new DiscordMessageBuilder().WithContent(text).WithAllowedMention(new UserMention());
            await channel.SendMessageAsync(builder);
        }

        public Echo(DiscordClient client) : base(client)
        {
        }
    }
}