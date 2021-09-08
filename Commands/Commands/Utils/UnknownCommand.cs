using System;
using System.Threading.Tasks;
using Commands.CommandsStuff;
using Commands.Utils;
using DSharpPlus.Entities;

namespace Commands.Commands.Utils
{
    public class UnknownCommand : Command
    {
        public override string GroupName => "Utils";
        public override bool Unknown => true;
        public override bool Hidden => true;
        public override string Description => "Command for unknown idk";
        public override async Task<DiscordMessage[]> Run(DiscordMessage message, ArgumentCollector collector)
        {
            var privateChannel = message.Channel.Guild is null;
            var extension = Extension;
            var prefix = (await extension.Provider.Get(message.Channel.Guild)).Prefix;
            var command = privateChannel ? "`help`" : $"`{prefix}help` or `@{Client.CurrentUser.Username}#{Client.CurrentUser.Discriminator} help`";
            var replyString = $"Unknown Command. Use {command} to get the command list";
            var replyMessage = await message.ReplyAsync(replyString);
            return new[] { replyMessage };
        }

        public override Task<DiscordMessage[]> Run(DiscordInteraction interaction, ArgumentCollector argumentCollector)
        {
            throw new Exception("how");
        }
    }
}