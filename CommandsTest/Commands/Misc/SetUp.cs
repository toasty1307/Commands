using System;
using System.Threading.Tasks;
using Commands.CommandsStuff;
using Commands.Utils;
using DSharpPlus.Entities;

namespace CommandsTest.Commands.Misc
{
    public class SetUp : Command
    {
        public override string GroupName => "Misc"; 
        public override string Description => "setup stuff idk";
        public override bool GuildOnly => true;

        public override async Task Run(DiscordMessage message, ArgumentCollector collector)
        {
            var msg = await message.ReplyAsync("i'll try");
            await Extension.Registry.RegisterSlashCommands(Extension.Registry.Commands.ToArray(), message.Channel.Guild);
            await msg.ModifyAsync("done ig");
        }

        public override Task Run(DiscordInteraction interaction, ArgumentCollector argumentCollector)
        {
            throw new Exception("how did you manage to run this");
        }
    }
}