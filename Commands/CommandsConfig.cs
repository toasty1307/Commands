using Commands.Data;

namespace Commands
{
    public class CommandsConfig
    {
        public ulong[] Owners { get; init; }
        public string Prefix { get; init; } = "!";
        public string Invite { get; init; } = "haha no server invite sadge";
        public bool NonCommandEditable { get; init; } = true;
        public ISettingProvider Provider { get; init; }
    }
}