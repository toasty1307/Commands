using System.Collections.Generic;

namespace CommandsTest.Data
{
    public class GuildEntity : global::Commands.Data.GuildEntity
    {
        public List<BlacklistEntity> Blacklist { get; set; } = new();
        public List<TagEntity> Tags { get; set; } = new();

        public GuildEntity(global::Commands.Data.GuildEntity other)
        {
            GuildId = other.GuildId;
            Commands = other.Commands;
            Groups = other.Groups;
            Prefix = other.Prefix;

            if (other is not GuildEntity ge) return;
            Blacklist = ge.Blacklist;
            Tags = ge.Tags;
        }

        public static GuildEntity FromGuildEntity(global::Commands.Data.GuildEntity other) => other is GuildEntity ge ? ge : new GuildEntity(other);

        public GuildEntity() { }
    }
}