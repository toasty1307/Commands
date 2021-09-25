using DSharpPlus;
using DSharpPlus.Entities;

namespace Commands.Data
{
    public abstract class SettingProvider : CommandsBase
    {
        protected SettingProvider(DiscordClient client) : base(client) { }

        public abstract void Init();
        public abstract GuildSettings Get(DiscordGuild guild);
        public abstract void Set(DiscordGuild guild, GuildSettings settings, bool bypassChecks = false);
        public abstract void Clear(DiscordGuild guild);
        public abstract void Update();
        public abstract void Update(DiscordGuild guild);
        public abstract void Destroy();
    }
}