using System.Threading.Tasks;
using Commands.Utils;
using DSharpPlus;
using DSharpPlus.Entities;

namespace Commands.CommandsStuff
{
    public class CommandContext
    {
        public ArgumentCollector Collector { get; init; }
        public DiscordClient Client { get; init; }
        public CommandsExtension Extension => Client.GetCommandsExtension();
        public bool GetArg<T>(string key, out T t) => Collector.Get(key, out t);
        public T GetArg<T>(string key) => Collector.Get<T>(key);
    }
    public class MessageContext : CommandContext
    {
        public DiscordMessage Message { get; init; }
        public DiscordGuild Guild => Message.Channel.Guild;
        public DiscordChannel Channel => Message.Channel;
        public DiscordUser Author => Message.Author;
        public DiscordMember Member => Author as DiscordMember;
        public bool PrivateChannel => Guild is null;
        public string RawCommandString => Message.Content;
        public string RawArgString => string.Join(' ', Extension.Dispatcher.GetCommandString(Message)[1..]);
        public Task<DiscordMessage> ReplyAsync(string content) => Message.ReplyAsync(content);
        public Task<DiscordMessage> ReplyAsync(string content, DiscordEmbed embed) => Message.ReplyAsync(content, embed);
        public Task<DiscordMessage> ReplyAsync(DiscordEmbed embed) => Message.ReplyAsync(embed);
        public Task<DiscordMessage> ReplyAsync(DiscordMessageBuilder builder) => Message.ReplyAsync(builder);
        public void Deconstruct(out ArgumentCollector collector, out DiscordClient client, out CommandsExtension extension, out DiscordMessage message,
            out DiscordGuild guild, out DiscordChannel channel, out DiscordUser author, out DiscordMember member, out bool privateChannel, out string rawCommandString, out string rawArgString)
        {
            collector = Collector;
            client = Client;
            extension = Extension;
            message = Message;
            guild = Guild;
            channel = Channel;
            author = Author;
            member = Member;
            privateChannel = PrivateChannel;
            rawCommandString = RawCommandString;
            rawArgString = RawArgString;
        }
    }
    public class InteractionContext : CommandContext
    {
        public DiscordInteraction Interaction { get; init; }
        public DiscordGuild Guild => Interaction.Channel.Guild;
        public DiscordChannel Channel => Interaction.Channel;
        public DiscordUser Author => Interaction.User;
        public DiscordMember Member => Author as DiscordMember;
        public Task ReplyAsync(string content, bool ephemeral = true) => Interaction.FollowUpAsync(content, ephemeral);
        public Task ReplyAsync(string content, DiscordEmbed embed, bool ephemeral = true) => Interaction.FollowUpAsync(content, embed, ephemeral);
        public Task ReplyAsync(DiscordEmbed embed, bool ephemeral = true) => Interaction.FollowUpAsync(embed, ephemeral);
        public Task ReplyAsync(DiscordMessageBuilder builder, bool ephemeral = true) => Interaction.FollowUpAsync(builder, ephemeral);
        public void Deconstruct(out ArgumentCollector collector, out DiscordClient client, out CommandsExtension extension, out DiscordInteraction interaction,
            out DiscordGuild guild, out DiscordChannel channel, out DiscordUser author, out DiscordMember member)
        {
            collector = Collector;
            client = Client;
            extension = Extension;
            interaction = Interaction;
            guild = Guild;
            channel = Channel;
            author = Author;
            member = Member;
        }
    }
}