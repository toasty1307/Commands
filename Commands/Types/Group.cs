using System.Linq;
using Commands.CommandsStuff;
using DSharpPlus;

namespace Commands.Types
{
    [ArgumentType]
    public class GroupArgumentType : ArgumentType<Group>
    {
        public override bool Validate(string argString) =>
            Extension.Registry.Groups.Any(x => x.Name.ToLower().Contains(argString.ToLower()) || x.Id.ToString() == argString);

        public override Group Parse(string argString) =>
            Extension.Registry.Groups.Find(x => x.Name.ToLower().Contains(argString.ToLower()) || x.Id.ToString() == argString);

        public override bool IsEmpty(Group arg) => 
            false;

        public GroupArgumentType(DiscordClient client) : base(client)
        {
        }
    }
}