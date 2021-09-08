using System;
using System.Threading.Tasks;
using Commands.CommandsStuff;
using Commands.Utils;
using DSharpPlus.Entities;

namespace CommandsTest.Commands.Admin
{
    public class Shut : Command
    {
        public override string GroupName => "Admin";
        public override string Description => "close bot ig";
        public override bool OwnerOnly => true;
        public override bool Hidden => true;

        public override async Task<DiscordMessage[]> Run(DiscordMessage message, ArgumentCollector collector)
        {
            await message.ReplyAsync("cya");
            Environment.Exit(0);
            return null;
        }

        public override async Task Run(DiscordInteraction interaction, ArgumentCollector argumentCollector)
        {
            await interaction.FollowUpAsync("cya");
            Environment.Exit(0);
        }
    }
}