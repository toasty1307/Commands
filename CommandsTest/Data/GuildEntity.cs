using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CommandsTest.Data
{
    public class GuildEntity
    {
        [Key]
        public ulong Id { get => GuildId; set => GuildId = value; }
        
        public ulong GuildId { get; set; }

        public List<BlacklistEntity> Blacklist { get; set; } = new();
    }
}