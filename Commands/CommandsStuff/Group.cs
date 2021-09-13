using System.Collections.Generic;
using System.Linq;

namespace Commands.CommandsStuff
{
    public class Group
    {
        public static List<Group> Groups { get; } = new();
        public string Name { get; init; }
        public string Description { get; init; }
        public bool Guarded { get; init; }
        public uint Id { get; set; }
        public List<Command> Commands { get; } = new();

        public override string ToString() => Id.ToString();
        public static implicit operator Group(string s) => Groups.First(x => x.ToString() == s);
    }
}