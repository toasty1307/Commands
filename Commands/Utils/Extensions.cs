using System;
using System.Collections.Generic;
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
        public static Task<DiscordMessage> ReplyAsync(this DiscordMessage message, DiscordEmbed embed)
        {
            var builder = new DiscordMessageBuilder().WithAllowedMention(new UserMention()).WithEmbed(embed);
            return message.RespondAsync(builder);
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
        
        public static Task FollowUpAsync(this DiscordInteraction message, string content, bool ephemeral = true)
        {
            return message.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder{Content = content, IsEphemeral = ephemeral});
        }
        public static Task FollowUpAsync(this DiscordInteraction message, DiscordEmbed embed, bool ephemeral = true)
        {
            return message.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder(){IsEphemeral = ephemeral}.AddEmbed(embed));
        }
        public static Task FollowUpAsync(this DiscordInteraction interaction, DiscordMessageBuilder builder, bool ephemeral = true)
        {
            return interaction.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder().AddEmbeds(builder.Embeds).AddComponents(builder.Components).AsEphemeral(ephemeral).WithContent(builder.Content));
        }
        public static Task FollowUpAsync(this DiscordInteraction message, string content, DiscordEmbed embed, bool ephemeral = true)
        {
            return message.CreateFollowupMessageAsync(new DiscordFollowupMessageBuilder{Content = content, IsEphemeral = ephemeral}.AddEmbed(embed));
        }
        
        public static CommandsExtension UseCommands(this DiscordClient client, CommandsExtension commandsExtension)
        {
            client.AddExtension(commandsExtension);
            return client.GetCommandsExtension();
        }

        public static CommandsExtension GetCommandsExtension(this DiscordClient client) =>
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
        
        public static void Error<T>(this ILogger<T> logger, Exception e) => logger.LogError($"{e.Message}\n{e.StackTrace}");

        public static void AddListener(this DiscordComponent component, Action<ComponentInteractionCreateEventArgs> listener, CommandDispatcher dispatcher)
        {
            dispatcher.ComponentActions.Add(component.CustomId, listener);
        }
        
        public static void RemoveListener(this DiscordComponent component, Action<ComponentInteractionCreateEventArgs> listener, CommandDispatcher dispatcher)
        {
            dispatcher.ComponentActions.Remove(component.CustomId);
        }

        public static List<T>[] Partition<T>(this List<T> list, int totalPartitions)
        {
            if (list == null)
                throw new ArgumentNullException(nameof(list));

            if (totalPartitions < 1)
                throw new ArgumentOutOfRangeException(nameof(totalPartitions));

            var partitions = new List<T>[totalPartitions];

            var maxSize = (int)Math.Ceiling(list.Count / (double)totalPartitions);
            var k = 0;

            for (var i = 0; i < partitions.Length; i++)
            {
                partitions[i] = new List<T>();
                for (var j = k; j < k + maxSize; j++)
                {
                    if (j >= list.Count)
                        break;
                    partitions[i].Add(list[j]);
                }
                k += maxSize;
            }

            return partitions;
        }
    }
}