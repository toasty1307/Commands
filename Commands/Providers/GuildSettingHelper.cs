using System.Collections.Generic;
using Commands.CommandsStuff;
using Commands.Utils;
using DSharpPlus;

namespace Commands.Providers
{
    public class GuildSettingHelper
    {
        public ulong GuildId { get; set; }
        public string Prefix { get; set; }
        public Dictionary<Command, bool> CommandStatuses { get; set; } = new();
        public Dictionary<Group, bool> GroupStatuses { get; set; } = new();

        public GuildSettingHelper(DiscordClient client, ulong guildId)
        {
            GuildId = guildId;
            var extension = client.GetCommandsExtension();
            foreach (var command in extension.Registry.Commands)
            {
                CommandStatuses.Add(command, true);
            }
            foreach (var group in extension.Registry.Groups)
            {
                GroupStatuses.Add(group, true);
            }
        }
        public GuildSettingHelper() { }
    }
}