using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Commands.Data
{
    public class GuildEntity
    {
        [Key]
        public ulong GuildId { get; set; }
        
        public string Prefix { get; set; }
        
        public Dictionary<DisabledCommandEntity, bool> Commands { get; set; } = new();
        public Dictionary<DisabledGroupEntity, bool> Groups { get; set; } = new();
        
        public override bool Equals(object obj)
        {
            return obj is not (null or not GuildEntity) && Equals((GuildEntity) obj);
        }

        private bool Equals(GuildEntity other)
        {
            return GuildId == other.GuildId && Prefix == other.Prefix && Equals(Commands, other.Commands) && Equals(Groups, other.Groups);
        }

        public override int GetHashCode()
        {
            // ReSharper disable once NonReadonlyMemberInGetHashCode
            return GuildId.GetHashCode();
        }
        
        public static bool operator ==(GuildEntity obj1, GuildEntity obj2) => obj1?.Equals(obj2) ?? false;

        public static bool operator !=(GuildEntity obj1, GuildEntity obj2) => !(obj1 == obj2);
    }
}