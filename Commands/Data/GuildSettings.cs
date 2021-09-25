using System;
using System.Collections.Generic;
using Commands.CommandsStuff;

namespace Commands.Data
{
    public class GuildSettings
    {
        public ulong Id { get; init; }
        public string Prefix { get; set; }
        public Dictionary<Command, bool> Commands { get; init; }
        public Dictionary<Group, bool> Groups { get; init; }

        public override bool Equals(object obj)
        {
            return obj is not (null or not GuildSettings) && Equals((GuildSettings) obj);
        }

        private bool Equals(GuildSettings other)
        {
            return Id == other.Id && Prefix == other.Prefix && Equals(Commands, other.Commands) && Equals(Groups, other.Groups);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id, Commands, Groups);
        }
        
        public static bool operator ==(GuildSettings obj1, GuildSettings obj2) => obj1?.Equals(obj2) ?? false;

        public static bool operator !=(GuildSettings obj1, GuildSettings obj2) => !(obj1 == obj2);
    }
}
