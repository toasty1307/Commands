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
            new Argument<DiscordTextChannelArgumentType>()
            {
                Key = "Channel",
                Optional = true
            },
            new Argument<StringArgumentType>()
            {
                Key = "Text",
                Infinite = true
            }
        };

        public override async Task Run(DiscordMessage message, ArgumentCollector collector)
        {
            var channel = collector.Get<DiscordChannel>("Channel") ?? message.Channel;
            await message.DeleteAsync();
            var text = collector.Get<string>("Text");
            var builder = new DiscordMessageBuilder().WithContent(text).WithAllowedMention(new UserMention());
            await channel.SendMessageAsync(builder);
        }

        public override async Task Run(DiscordInteraction interaction, ArgumentCollector collector)
        {
            var channel = collector.Get<DiscordChannel>("Channel") ?? interaction.Channel;
            var text = collector.Get<string>("Text");
            var original = await interaction.GetOriginalResponseAsync();
            if ((original.Flags & MessageFlags.Ephemeral) == 0) await interaction.DeleteOriginalResponseAsync();
            var builder = new DiscordMessageBuilder().WithContent(text).WithAllowedMention(new UserMention());
            await channel.SendMessageAsync(builder);
        }
    }
}