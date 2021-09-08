using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace Commands.Providers
{
    public abstract class SettingProvider : CommandsExtensionBase
    {
        public abstract Task Init();
        public abstract Task Clear(DiscordGuild guild);
        public abstract Task Destroy();
        public abstract Task<GuildSettingHelper> Get(DiscordGuild guild);
        public abstract Task Set(DiscordGuild guild, GuildSettingHelper helper);
    }
}