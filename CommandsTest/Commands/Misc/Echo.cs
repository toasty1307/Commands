using System.Runtime.InteropServices.ComTypes;
using System.Threading.Tasks;
using Commands.CommandsStuff;
using Commands.Types;
using Commands.Utils;
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
                Prompt = "Where",
                Optional = true
            },
            new Argument<StringArgumentType>()
            {
                Key = "Text",
                Prompt = "Text to say",
                Infinite = true
            }
        };

        public override async Task<DiscordMessage[]> Run(DiscordMessage message, ArgumentCollector collector)
        {
            var channel = collector.Get<DiscordChannel>("Channel") ?? message.Channel;
            await message.DeleteAsync();
            var text = collector.Get<string>("Text");
            var builder = new DiscordMessageBuilder().WithContent(text).WithAllowedMention(new UserMention());
            var echoMessage = await channel.SendMessageAsync(builder);
            return new[] {echoMessage};
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