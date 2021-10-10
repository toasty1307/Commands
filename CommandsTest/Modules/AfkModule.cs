using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Commands.Utils;
using CommandsTest.Data;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;

namespace CommandsTest.Modules
{
    public class AfkModule
    {
        private readonly List<AfkEntity> _afk;
        private readonly GuildContext _guildContext;

        public AfkModule()
        {
            _afk = (_guildContext = new GuildContext()).Afk.ToList();
        }
        
        public async Task OnMessage(DiscordClient sender, MessageCreateEventArgs e)
        {
            if (e.Author.IsBot) return;
            var first = _afk.FirstOrDefault(x => e.MentionedUsers.Any(x2 => x2.Id == x.UserId));
            if (first is not null)
                await e.Message.ReplyAsync($"{(await sender.GetUserAsync(first.UserId)).Username} is afk: {first.Message}, <t:{first.AfkSetTime.ConvertToUnixTimestamp()}:R>");
            if (_afk.Any(x => x.UserId == e.Author.Id))
            {
                await e.Message.ReplyAsync($"welcome back {e.Author.Username}, i removed your afk");
                RemoveAfk(e.Author);
            }
        }

        public async void SetAfk(DiscordUser user, string message)
        {
            var newAfk = new AfkEntity
            {
                UserId = user.Id,
                Message = message,
                AfkSetTime = DateTime.Now.ToUniversalTime()
            };
            _afk.Add(newAfk);
            _guildContext.Afk.Add(newAfk);
            await _guildContext.SaveChangesAsync();
        }

        public async void RemoveAfk(DiscordUser user)
        {
            var afkToRemove = _afk.FirstOrDefault(x => x.UserId == user.Id);
            if (afkToRemove is null) throw new FriendlyException("epic fail");
            _afk.Remove(afkToRemove);
            _guildContext.Afk.Remove(afkToRemove);
            await _guildContext.SaveChangesAsync();
        }
    }
}