using System.Collections.Generic;
using Commands.CommandsStuff;

namespace Commands.Providers
{
    public class GuildSettingHelper : CommandsExtensionBase
    {
        public ulong GuildId { get; set; }
        public string Prefix { get; set; }
        public Dictionary<Command, bool> CommandStatuses { get; set; } = new();
        public Dictionary<Group, bool> GroupStatuses { get; set; } = new();

        public GuildSettingHelper(ulong guildId)
        {
            GuildId = guildId;
            var extension = Extension;
            foreach (var command in extension.Registry.Commands)
            {
                CommandStatuses.Add(command, true);
            }
            foreach (var group in extension.Registry.Groups)
            {
                GroupStatuses.Add(group, true);
            }
        }
    }
}