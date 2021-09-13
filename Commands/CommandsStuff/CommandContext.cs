using System.Threading.Tasks;
using Commands.Utils;
using DSharpPlus;
using DSharpPlus.Entities;

namespace Commands.CommandsStuff
{
    public class CommandContext
    {
        public DiscordMessage Message { get; init; }
        public ArgumentCollector Collector { get; init; }
        public DiscordClient Client { get; init; }
        public CommandsExtension Extension => Client.GetCommandsExtension();
        public T GetArg<T>(string key) => Collector.Get<T>(key);
        public DiscordGuild Guild => Message.Channel.Guild;
        public DiscordChannel Channel => Message.Channel;
        public DiscordUser Author => Message.Author;
        public DiscordMember Member => Author as DiscordMember;
        public bool PrivateChannel => Guild is null;
        public Task<DiscordMessage> ReplyAsync(string content) => Message.ReplyAsync(content);
        public Task<DiscordMessage> ReplyAsync(string content, DiscordEmbed embed) => Message.ReplyAsync(content, embed);
        public Task<DiscordMessage> ReplyAsync(DiscordEmbed embed) => Message.ReplyAsync(embed);
        public Task<DiscordMessage> ReplyAsync(DiscordMessageBuilder builder) => Message.ReplyAsync(builder);
    }
    public class InteractionContext
    {
        public DiscordInteraction Interaction { get; init; }
        public DiscordClient Client { get; init; }
        public CommandsExtension Extension => Client.GetCommandsExtension();
        public ArgumentCollector Collector { get; init; }
        public T GetArg<T>(string key) => Collector.Get<T>(key);
        public DiscordGuild Guild => Interaction.Channel.Guild;
        public DiscordChannel Channel => Interaction.Channel;
        public DiscordUser Author => Interaction.User;
        public DiscordMember Member => Author as DiscordMember;
        public Task FollowUpAsync(string content, bool ephemeral = true) => Interaction.FollowUpAsync(content, ephemeral);
        public Task FollowUpAsync(string content, DiscordEmbed embed, bool ephemeral = true) => Interaction.FollowUpAsync(content, embed, ephemeral);
        public Task FollowUpAsync(DiscordEmbed embed, bool ephemeral = true) => Interaction.FollowUpAsync(embed, ephemeral);
        public Task FollowUpAsync(DiscordMessageBuilder builder, bool ephemeral = true) => Interaction.FollowUpAsync(builder, ephemeral);
    }
}