using System;
using System.Threading.Tasks;
using Commands.CommandsStuff;
using Commands.Utils;
using DSharpPlus;
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

        public override async Task<DiscordMessage[]> Run(DiscordMessage message, ArgumentCollector collector)
        {
            var pingMessage = await message.ReplyAsync($"The bot's ping is {Client.Ping}ms");
            return new[] {pingMessage};
        }

        public override async Task Run(DiscordInteraction interaction, ArgumentCollector argumentCollector)
        {
            await interaction.FollowUpAsync($"The bot's ping is {Client.Ping}ms");
        }
    }
}