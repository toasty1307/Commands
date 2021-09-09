using System.Threading.Tasks;
using Commands.CommandsStuff;
using Commands.Utils;
using DSharpPlus.Entities;

namespace Commands.Commands.Utils
{
    public class Ping : Command
    {
        public override string GroupName => "Utils";
        public override string[] Aliases => new[] {"latency"};

        public override ThrottlingOptions ThrottlingOptions => new()
        {
            Usages = 2,
            Duration = 20
        };
        public override string Description => "Ping, thats about it, yeah";

        public override async Task Run(DiscordMessage message, ArgumentCollector collector)
        {
            await message.ReplyAsync($"The bot's ping is {Client.Ping}ms");
        }

        public override async Task Run(DiscordInteraction interaction, ArgumentCollector argumentCollector)
        {
            await interaction.FollowUpAsync($"The bot's ping is {Client.Ping}ms");
        }
    }
}