using System.Threading.Tasks;
using Commands.CommandsStuff;
using Commands.Types;
using Commands.Utils;
using DSharpPlus.Entities;

namespace Commands.Commands.Commands
{
    public class Enabled : Command
    {
        public override string GroupName => "Commands";
        public override string Description => "Enable/disable command ig";
        public override bool GuildOnly => true;
        public override bool Guarded => true;

        public override Argument[] Arguments => new Argument[]
        {
            new Argument<CommandArgumentType>
            {
                Key = "Command"
            },
            new Argument<BoolArgumentType>
            {
                Key = "Enable",
                Optional = true
            }
        };

        public override async Task<DiscordMessage[]> Run(DiscordMessage message, ArgumentCollector collector)
        {
            var command = collector.Get<Command>("Command");
            var enable = collector.Get<bool?>("Enable");
            var provider = Extension.Provider;
            DiscordMessage replyMessage;
            if (provider is null)
                replyMessage = await message.ReplyAsync("No Provider is registered!");
            else
            {
                var enabled = (await provider.Get(message.Channel.Guild)).CommandStatuses[command];
                if (enable is null)
                    replyMessage = await message.ReplyAsync($"The Command {command.Name} is {(enabled ? "Enabled" : "Disabled")} in {message.Channel.Guild.Name}");
                else if (!command.Guarded)
                {
                    Extension.CommandStatusChanged(message.Channel.Guild, command, (bool) enable);
                    replyMessage = await message.ReplyAsync($"Command {command.Name} was {((bool) enable ? "Enabled" : "Disabled")}");
                }
                else
                    replyMessage = await message.ReplyAsync($"Command {command.Name} is guarded :|");
            }

            return new[] {replyMessage};
        }

        public override async Task Run(DiscordInteraction interaction, ArgumentCollector collector)
        {
            var command = collector.Get<Command>("Command");
            var enable = collector.Get<bool?>("Enable");
            var provider = Extension.Provider;
            if (provider is null)
                await interaction.FollowUpAsync("No Provider is registered!");
            else
            {
                var enabled = (await provider.Get(interaction.Channel.Guild)).CommandStatuses[command];
                if (enable is null)
                    await interaction.FollowUpAsync($"The Command {command.Name} is {(enabled ? "Enabled" : "Disabled")} in {interaction.Channel.Guild.Name}");
                else if (!command.Guarded)
                {
                    Extension.CommandStatusChanged(interaction.Channel.Guild, command, (bool) enable);
                    await interaction.FollowUpAsync($"Command {command.Name} was {((bool) enable ? "Enabled" : "Disabled")}");
                }
                else
                    await interaction.FollowUpAsync($"Command {command.Name} is guarded :|");
            }
        }
    }
}