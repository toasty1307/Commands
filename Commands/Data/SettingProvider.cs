using System.Threading.Tasks;
using DSharpPlus.Entities;

namespace Commands.Data
{
    public interface ISettingProvider
    {
        public void Set(GuildEntity entity);
        public GuildEntity Get(DiscordGuild guild);
        public void Remove(GuildEntity entity);
        public void Init();
    }
}