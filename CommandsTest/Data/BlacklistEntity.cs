using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CommandsTest.Data
{
    public class BlacklistEntity
    {
        [Key]
        public ulong Id { get; set; }
        
        public ulong GuildId { get; set; }
        
        public GuildEntity GuildEntity { get; set; }

        public List<CommandEntity> Commands { get; set; } = new();
        public List<GroupEntity> Groups { get; set; } = new();
    }
}