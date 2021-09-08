namespace Commands
{
    public class CommandsConfig
    {
        public ulong[] Owners { get; set; }
        public string Prefix { get; set; } = "!";
        public string Invite { get; set; } = "haha no server invite sadge";
        public int CommandEditableDuration { get; set; } = 30;
        public bool NonCommandEditable { get; set; } = true;
    }
}