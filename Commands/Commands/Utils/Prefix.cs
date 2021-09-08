using System.Threading.Tasks;
using Commands.CommandsStuff;
using Commands.Types;
using Commands.Utils;
using DSharpPlus.Entities;

namespace Commands.Commands.Utils
{
    public class Prefix : Command
    {
        public override string GroupName => "Utils";
        public override string Description => "prefix command to chnage prefix ig idk";

        public override Argument[] Arguments => new Argument[]
        {
            new Argument<StringArgumentType>()
            {
                Key = "Prefix",
                Optional = true
            }
        };

        public override async Task<DiscordMessage[]> Run(DiscordMessage message, ArgumentCollector collector)
        {
            var prefix = collector.Get<string>("Prefix");
            var shouldSetNewPrefix = !string.IsNullOrWhiteSpace(prefix);
            var provider = Extension.Provider;
            DiscordMessage replyMessage;
            if (shouldSetNewPrefix)
            {
                if (provider is not null)
                {
                    if (message.Channel.Guild is not null)
                    {
                        var settings = await provider.Get(message.Channel.Guild);
                        settings.Prefix = prefix;
                        Extension.CommandPrefixChanged(message.Channel.Guild, prefix);
                        replyMessage = await message.ReplyAsync($"Set the command prefix to {prefix}");
                    }
                    else
                        replyMessage = await message.ReplyAsync("Cant Set a prefix in DMs!");
                }
                else
                    replyMessage = await message.ReplyAsync("No Settings Provider is registered!");
            }
            else if (provider is not null)
                replyMessage = await message.ReplyAsync(
                    $"The prefix in {(message.Channel.Guild is null ? "DMs" : message.Channel.Guild.Name)} is {(message.Channel.Guild is null ? $"`@{Client.CurrentUser.Username}#{Client.CurrentUser.Discriminator}`" : $"`{(await provider.Get(message.Channel.Guild))?.Prefix ?? Extension.CommandPrefix}` or `@{Client.CurrentUser.Username}#{Client.CurrentUser.Discriminator}`")}");
            else
                replyMessage = await message.ReplyAsync($"The prefix in {(message.Channel.Guild is null ? "DMs" : message.Channel.Guild.Name)} is {(message.Channel.Guild is null ? $"`@{Client.CurrentUser.Username}#{Client.CurrentUser.Discriminator}`" : $"`{Extension.CommandPrefix}` or `@{Client.CurrentUser.Username}#{Client.CurrentUser.Discriminator}`")}");

            return new[] {replyMessage};
        }

        public override async Task Run(DiscordInteraction interaction, ArgumentCollector collector)
        {
            var prefix = collector.Get<string>("Prefix");
            var shouldSetNewPrefix = !string.IsNullOrWhiteSpace(prefix);
            var provider = Extension.Provider;
            if (shouldSetNewPrefix)
            {
                if (provider is not null)
                {
                    if (interaction.Channel.Guild is not null)
                    {
                        var settings = await provider.Get(interaction.Channel.Guild);
                        settings.Prefix = prefix;
                        Extension.CommandPrefixChanged(interaction.Channel.Guild, prefix);
                        await interaction.FollowUpAsync($"Set the command prefix to {prefix}");
                    }
                    else
                        await interaction.FollowUpAsync("Cant Set a prefix in DMs!");
                }
                else
                    await interaction.FollowUpAsync("No Settings Provider is registered!");
            }
            else if (provider is not null)
                await interaction.FollowUpAsync(
                    $"The prefix in {(interaction.Channel.Guild is null ? "DMs" : interaction.Channel.Guild.Name)} is {(interaction.Channel.Guild is null ? $"`@{Client.CurrentUser.Username}#{Client.CurrentUser.Discriminator}`" : $"`{(await provider.Get(interaction.Channel.Guild))?.Prefix ?? Extension.CommandPrefix}` or `@{Client.CurrentUser.Username}#{Client.CurrentUser.Discriminator}`")}");
            else 
                await interaction.FollowUpAsync($"The prefix in {(interaction.Channel.Guild is null ? "DMs" : interaction.Channel.Guild.Name)} is {(interaction.Channel.Guild is null ? $"`@{Client.CurrentUser.Username}#{Client.CurrentUser.Discriminator}`" : $"`{Extension.CommandPrefix}` or `@{Client.CurrentUser.Username}#{Client.CurrentUser.Discriminator}`")}");
        }
    }
}