using System;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;

namespace Commands.Utils
{
    public static class Extensions
    {
        public static Task<DiscordMessage> ReplyAsync(this DiscordMessage message, string content)
        {
            var builder = new DiscordMessageBuilder().WithAllowedMention(new UserMention()).WithContent(content);
            return message.RespondAsync(builder);
        }
        public static Task<DiscordMessage> ReplyAsync(this DiscordMessage message, string content, IMention[] allowedMentions)
        {
            var builder = new DiscordMessageBuilder().WithAllowedMentions(allowedMentions).WithContent(content);
            return message.RespondAsync(builder);
        }
        public static Task<DiscordMessage> ReplyAsync(this DiscordMessage message, DiscordEmbed embed)
        {
            var builder = new DiscordMessageBuilder().WithAllowedMention(new UserMention()).WithEmbed(embed);
            return message.RespondAsync(builder);
        }
        public static Task<DiscordMessage> ReplyAsync(this DiscordMessage message, DiscordEmbed embed, IMention[] allowedMentions)
        {
            var builder = new DiscordMessageBuilder().WithAllowedMentions(allowedMentions).WithEmbed(embed);
            return message.RespondAsync(builder);
        }
        public static Task<DiscordMessage> ReplyAsync(this DiscordMessage message, DiscordMessageBuilder builder, IMention[] allowedMentions)
        {
            return message.RespondAsync(builder.WithAllowedMentions(allowedMentions));
        }
        public static Task<DiscordMessage> ReplyAsync(this DiscordMessage message, DiscordMessageBuilder builder)
        {
            builder.WithAllowedMention(new UserMention());
            return message.RespondAsync(builder);
        }
        public static Task<DiscordMessage> ReplyAsync(this DiscordMessage message, string content, DiscordEmbed embed)
        {
            var builder = new DiscordMessageBuilder().WithAllowedMention(new UserMention()).WithContent(content).WithEmbed(embed);
            return message.RespondAsync(builder);
        }
        public static Task<DiscordMessage> ReplyAsync(this DiscordMessage message, string content, DiscordEmbed embed, IMention[] allowedMentions)
        {
            var builder = new DiscordMessageBuilder().WithAllowedMentions(allowedMentions).WithContent(content).WithEmbed(embed);
            return message.RespondAsync(builder);
        }
        
        public static CommandsExtension UseCommands(this DiscordClient client, CommandsExtension commandsExtension)
        {
            client.AddExtension(commandsExtension);
            return client.GetExtension<CommandsExtension>();
        }

        public static CommandsExtension CommandsExtension(this DiscordClient client) =>
            client.GetExtension<CommandsExtension>();
        
        
        public static DateTime ConvertFromUnixTimestamp(double timestamp)
        {
            var origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            return origin.AddSeconds(timestamp);
        }

        public static double ConvertToUnixTimestamp(this DateTime date)
        {
            var origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            var diff = date.ToUniversalTime() - origin;
            return Math.Floor(diff.TotalSeconds);
        }
        
        
        
        public static async Task FollowUpAsync(this DiscordInteraction message, string content, bool ephemeral = true)
        {
            await message.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder{Content = content, IsEphemeral = ephemeral});
        }
        public static async Task FollowUpAsync(this DiscordInteraction message, DiscordEmbed embed, bool ephemeral = true)
        {
            await message.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder(){IsEphemeral = ephemeral}.AddEmbed(embed));
        }
        public static async Task FollowUpAsync(this DiscordInteraction message, DiscordMessageBuilder builder, bool ephemeral = true)
        {
            builder.WithAllowedMention(new UserMention());
            await message.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder{Content = builder.Content, IsEphemeral = ephemeral}.AddEmbeds(builder.Embeds).AddComponents(builder.Components));
        }
        public static async Task FollowUpAsync(this DiscordInteraction message, string content, DiscordEmbed embed, bool ephemeral = true)
        {
            await message.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder{Content = content, IsEphemeral = ephemeral}.AddEmbed(embed));
        }

        public static void Error<T>(this ILogger<T> logger, Exception e) => logger.LogError($"{e.Message}\n{e.StackTrace}");

        public static void AddListener(this DiscordComponent component, Action<ComponentInteractionCreateEventArgs> listener)
        {
            CommandsExtensionBase.Extension.Dispatcher.ComponentActions.Add(component.CustomId, listener);
        }
    }
}