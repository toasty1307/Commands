namespace Commands
{
    public class CommandsConfig
    {
        public ulong[] Owners { get; set; }
        public string Prefix { get; set; } = "!";
        public string Invite { get; set; } = "haha no server invite sadge";
        public bool NonCommandEditable { get; set; } = true;
    }
}