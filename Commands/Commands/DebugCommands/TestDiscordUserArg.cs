#if DEBUG
using System.Threading.Channels;
using System.Threading.Tasks;
using Commands.CommandsStuff;
using Commands.Types;
using Commands.Utils;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;

namespace Commands.Commands.DebugCommands
{
    public class TestDiscordUserArg : Command
    {
        public override string GroupName => "Debug";
        public override string Description => "test";

        public override Argument[] Arguments => new Argument[]
        {
            new Argument<DiscordUserArgumentType>
            {
                Key = "User",
                Description = "yes"
            }
        };

        public override async Task<DiscordMessage[]> Run(DiscordMessage message, ArgumentCollector collector)
        {
            var user = collector.Get<DiscordUser>("User");
            var msg = await message.ReplyAsync(user?.Id.ToString() ?? "null");
            return new[] {msg};
        }

        public override async Task Run(DiscordInteraction interaction, ArgumentCollector collector)
        {
            var user = collector.Get<DiscordUser>("User");
            await interaction.FollowUpAsync(user?.Id.ToString() ?? "null");
        }
    }
}
#endif