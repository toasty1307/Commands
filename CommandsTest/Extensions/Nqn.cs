using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace CommandsTest.Extensions
{
    public class Nqn : BaseExtension
    {
        private readonly Dictionary<ulong, DiscordWebhook> _webhooks = new();
        private readonly DiscordWebhookClient _client = new();

        protected override async void Setup(DiscordClient client)
        {
            foreach (var discordChannel in client.Guilds.Values.SelectMany(x => x.Channels.Values).Where(x => x.Type == ChannelType.Text))
            {
                _webhooks.Add(discordChannel.Id, (await discordChannel.GetWebhooksAsync()).FirstOrDefault(x => x.ChannelId == discordChannel.Id));
            }

            client.MessageCreated += EmojiCheck;
        }

        private readonly Regex _emojiRegex = new(@":(.*):", RegexOptions.Compiled);
        
        public async Task EmojiCheck(DiscordClient client, MessageCreateEventArgs args)
        {
            var message = args.Message;
            if (message.Author.IsBot) return;
            var match = _emojiRegex.Match(message.Content);
            if (match.Success)
            {
                var canUseEmoji = DiscordEmoji.TryFromName(client, match.Value, out var emoji);
                if (!canUseEmoji) return;
                var builder = new DiscordWebhookBuilder()
                    .WithContent(message.Content.Replace(match.Value, emoji.ToString()))
                    .WithAvatarUrl(message.Author.AvatarUrl)
                    .WithUsername(message.Author is DiscordMember member ? member.Nickname : message.Author.Username);
                await message.DeleteAsync();
                await _client.AddWebhook(_webhooks[message.Channel.Id]).ExecuteAsync(builder);
                _client.RemoveWebhook(_webhooks[message.Channel.Id].Id);
            }
        }
    }
}