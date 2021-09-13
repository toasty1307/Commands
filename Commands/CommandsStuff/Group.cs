using System.Collections.Generic;
using System.Linq;

namespace Commands.CommandsStuff
{
    public class Group
    {
        public static List<Group> Groups { get; set; } = new();
        public string Name { get; set; }
        public string Description { get; set; }
        public uint Id { get; set; }
        public List<Command> Commands { get; set; } = new();

        public override string ToString() => Id.ToString();
        public static implicit operator Group(string s) => Groups.First(x => x.ToString() == s);
    }
}